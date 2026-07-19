using System.Collections.Concurrent;
using System.Text.Json;
using Backend.Models;
using Backend.Repositories;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Services
{
    public interface IRoomStateStore
    {
        void MarkDirty(GameRoom room);
        Task DeleteAsync(string roomCode, CancellationToken cancellationToken = default);
        Task FlushAsync(CancellationToken cancellationToken = default);
    }

    public sealed class RoomStateStore : BackgroundService, IRoomStateStore
    {
        private static readonly TimeSpan DebounceDelay = TimeSpan.FromSeconds(2);
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly ConcurrentDictionary<string, DirtyRoom> _dirtyRooms = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IRoomRepository _roomRepository;
        private readonly ILogger<RoomStateStore> _logger;
        private readonly IMongoCollection<RoomSnapshotDocument> _snapshots;

        public RoomStateStore(
            IServiceScopeFactory scopeFactory,
            IRoomRepository roomRepository,
            ILogger<RoomStateStore> logger,
            IMongoDatabase db)
        {
            _scopeFactory = scopeFactory;
            _roomRepository = roomRepository;
            _logger = logger;
            _snapshots = db.GetCollection<RoomSnapshotDocument>("game_room_snapshots");
        }

        public void MarkDirty(GameRoom room)
        {
            _dirtyRooms.AddOrUpdate(
                room.RoomCode,
                _ => new DirtyRoom(1, DateTime.UtcNow),
                (_, existing) => existing with { Version = existing.Version + 1 });
        }

        public async Task DeleteAsync(string roomCode, CancellationToken cancellationToken = default)
        {
            _dirtyRooms.TryRemove(roomCode, out _);
            await _snapshots.DeleteOneAsync(x => x.Id == roomCode, cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await EnsureTableAsync(stoppingToken);
                await RestoreRoomsAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Snapshot restore was skipped because MongoDB is temporarily unavailable.");
            }

            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await FlushReadyAsync(stoppingToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "Snapshot flush failed; queued room state will be retried.");
                }
            }
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in _dirtyRooms.ToArray())
                await PersistIfCurrentAsync(entry.Key, entry.Value, cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await FlushAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }

        private async Task FlushReadyAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in _dirtyRooms.ToArray())
            {
                if (now - entry.Value.FirstDirtyUtc >= DebounceDelay)
                    await PersistIfCurrentAsync(entry.Key, entry.Value, cancellationToken);
            }
        }

        private async Task PersistIfCurrentAsync(string roomCode, DirtyRoom observed, CancellationToken cancellationToken)
        {
            var room = _roomRepository.GetRoom(roomCode);
            if (room == null)
            {
                _dirtyRooms.TryRemove(roomCode, out _);
                return;
            }

            var snapshot = RoomSnapshot.From(room);
            var payload = JsonSerializer.Serialize(snapshot, JsonOptions);
            var expiresAt = room.IsFinished ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow.AddHours(6);

            var doc = new RoomSnapshotDocument
            {
                Id = roomCode,
                Payload = payload,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt
            };

            await _snapshots.ReplaceOneAsync(
                x => x.Id == roomCode, 
                doc, 
                new ReplaceOptions { IsUpsert = true }, 
                cancellationToken);

            if (_dirtyRooms.TryGetValue(roomCode, out var current) && current.Version == observed.Version)
            {
                _dirtyRooms.TryRemove(roomCode, out _);
            }
            else if (current is not null)
            {
                _dirtyRooms.TryUpdate(roomCode, current with { FirstDirtyUtc = DateTime.UtcNow }, current);
            }
        }

        private async Task EnsureTableAsync(CancellationToken cancellationToken)
        {
            var keys = Builders<RoomSnapshotDocument>.IndexKeys.Ascending(x => x.ExpiresAt);
            var options = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
            var model = new CreateIndexModel<RoomSnapshotDocument>(keys, options);
            await _snapshots.Indexes.CreateOneAsync(model, cancellationToken: cancellationToken);
        }

        private async Task RestoreRoomsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var cursor = await _snapshots.FindAsync(x => x.ExpiresAt > now, cancellationToken: cancellationToken);
            var docs = await cursor.ToListAsync(cancellationToken);
            var restored = 0;
            foreach(var doc in docs)
            {
                var snapshot = JsonSerializer.Deserialize<RoomSnapshot>(doc.Payload, JsonOptions);
                if (snapshot == null) continue;
                var room = snapshot.ToRoom();
                room.RestoredAtUtc = DateTime.UtcNow;
                _roomRepository.AddOrUpdateRoom(room);
                restored++;
            }

            if (restored > 0)
                _logger.LogInformation("Restored {Count} game rooms from MongoDB snapshots.", restored);
        }

        private sealed record DirtyRoom(long Version, DateTime FirstDirtyUtc);
    }

    public class RoomSnapshotDocument
    {
        [BsonId]
        public string Id { get; set; } = string.Empty; // Room code
        public string Payload { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public sealed record RoomSnapshot(
        int Version,
        string RoomCode,
        int NextPlayerId,
        int ActivePlayerIndex,
        bool IsStarted,
        bool IsStarting,
        bool IsFinished,
        int GameDurationMinutes,
        DateTime? GameStartTime,
        DateTime? GameEndTimeUtc,
        List<PlayerStateSnapshot> Players)
    {
        public static RoomSnapshot From(GameRoom room)
        {
            lock (room.SyncRoot)
            {
                return new RoomSnapshot(
                    1,
                    room.RoomCode,
                    room.NextPlayerId,
                    room.ActivePlayerIndex,
                    room.IsStarted,
                    room.IsStarting,
                    room.IsFinished,
                    room.GameDurationMinutes,
                    room.GameStartTime,
                    room.GameEndTimeUtc,
                    room.Players.Select(PlayerStateSnapshot.From).ToList());
            }
        }

        public GameRoom ToRoom()
        {
            return new GameRoom
            {
                RoomCode = RoomCode,
                NextPlayerId = NextPlayerId,
                ActivePlayerIndex = ActivePlayerIndex,
                IsStarted = IsStarted,
                IsStarting = false,
                IsFinished = IsFinished,
                GameDurationMinutes = GameDurationMinutes,
                GameStartTime = GameStartTime,
                GameEndTimeUtc = GameEndTimeUtc,
                Players = Players.Select(player => player.ToPlayer()).ToList()
            };
        }
    }

    public sealed record PlayerStateSnapshot(
        int Id,
        string SessionToken,
        string Name,
        string HorseId,
        int TileIndex,
        int WrongStreak,
        bool Shield,
        int FreezeTimeMs,
        bool DoubleDice,
        int DiceModifier,
        bool IsAutoRoll,
        int LapCount,
        bool IsSpectator,
        bool IsHost,
        DateTime? FrozenUntilUtc,
        string? CurrentQuestionId,
        string? PendingTileEventType,
        int PendingTileIndex,
        bool CanProcessNewTileLanding,
        PlayerPhase Phase,
        DateTime NextRollAllowedUtc)
    {
        public static PlayerStateSnapshot From(Player player)
        {
            lock (player)
            {
                return new PlayerStateSnapshot(
                    player.Id, player.SessionToken, player.Name, player.HorseId,
                    player.TileIndex, player.WrongStreak, player.Shield, player.FreezeTimeMs,
                    player.DoubleDice, player.DiceModifier, player.IsAutoRoll, player.LapCount,
                    player.IsSpectator, player.IsHost, player.FrozenUntilUtc,
                    player.CurrentQuestionId, player.PendingTileEventType, player.PendingTileIndex,
                    player.CanProcessNewTileLanding, player.Phase, player.NextRollAllowedUtc);
            }
        }

        public Player ToPlayer()
        {
            return new Player
            {
                Id = Id,
                SessionToken = SessionToken,
                ConnectionId = string.Empty,
                Name = Name,
                HorseId = HorseId,
                TileIndex = TileIndex,
                WrongStreak = WrongStreak,
                Shield = Shield,
                FreezeTimeMs = FreezeTimeMs,
                DoubleDice = DoubleDice,
                DiceModifier = DiceModifier,
                IsAutoRoll = IsAutoRoll,
                LapCount = LapCount,
                IsSpectator = IsSpectator,
                IsHost = IsHost,
                FrozenUntilUtc = FrozenUntilUtc,
                CurrentQuestionId = CurrentQuestionId,
                PendingTileEventType = PendingTileEventType,
                PendingTileIndex = PendingTileIndex,
                CanProcessNewTileLanding = CanProcessNewTileLanding,
                Phase = Phase == PlayerPhase.Rolling ? PlayerPhase.Ready : Phase,
                NextRollAllowedUtc = NextRollAllowedUtc
            };
        }
    }
}
