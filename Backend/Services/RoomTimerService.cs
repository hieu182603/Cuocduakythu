using System.Collections.Concurrent;
using Backend.Hubs;
using Backend.Models;
using Backend.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Services
{
    public interface IRoomTimerService
    {
        void Schedule(GameRoom room);
        void Cancel(GameRoom room);
        Task EndGameAsync(string roomCode, CancellationToken cancellationToken = default);
    }

    public sealed class RoomTimerService : IRoomTimerService, IHostedService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly IRoomStateStore _stateStore;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<RoomTimerService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ConcurrentDictionary<string, Task> _runningTimers = new();
        private Task? _watchTask;

        public RoomTimerService(
            IRoomRepository roomRepository,
            IRoomStateStore stateStore,
            IHubContext<GameHub> hubContext,
            ILogger<RoomTimerService> logger,
            IHostApplicationLifetime applicationLifetime)
        {
            _roomRepository = roomRepository;
            _stateStore = stateStore;
            _hubContext = hubContext;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var room in _roomRepository.GetAllRooms().Where(room => room.IsStarted))
                Schedule(room);
            _watchTask = WatchRestoredRoomsAsync(_applicationLifetime.ApplicationStopping);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var room in _roomRepository.GetAllRooms())
                Cancel(room);

            var running = _runningTimers.Values.ToArray();
            if (running.Length > 0)
                await Task.WhenAll(running).WaitAsync(cancellationToken);
            if (_watchTask is not null)
                await _watchTask.WaitAsync(cancellationToken);
        }

        private async Task WatchRestoredRoomsAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    foreach (var room in _roomRepository.GetAllRooms())
                    {
                        if (room.IsStarted && room.GameTimerCancellation is null)
                            Schedule(room);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
        }

        public void Schedule(GameRoom room)
        {
            Cancel(room);
            var endTime = room.GameEndTimeUtc
                ?? DateTime.UtcNow.AddMinutes(Math.Clamp(room.GameDurationMinutes, 1, 180));
            room.GameEndTimeUtc = endTime;
            var cancellation = new CancellationTokenSource();
            room.GameTimerCancellation = cancellation;
            var task = RunTimerAsync(room.RoomCode, endTime, cancellation.Token);
            _runningTimers[room.RoomCode] = task;
            _ = task.ContinueWith(
                ignored =>
                {
                    _runningTimers.TryRemove(room.RoomCode, out var removedTask);
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        public void Cancel(GameRoom room)
        {
            var cancellation = room.GameTimerCancellation;
            room.GameTimerCancellation = null;
            if (cancellation == null) return;
            cancellation.Cancel();
            cancellation.Dispose();
            _runningTimers.TryRemove(room.RoomCode, out _);
        }

        private async Task RunTimerAsync(string roomCode, DateTime endTimeUtc, CancellationToken cancellationToken)
        {
            try
            {
                var delay = endTimeUtc - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, cancellationToken);
                await EndGameAsync(roomCode, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Room timer failed for {RoomCode}.", roomCode);
            }
        }

        public async Task EndGameAsync(string roomCode, CancellationToken cancellationToken = default)
        {
            var room = _roomRepository.GetRoom(roomCode);
            if (room == null) return;

            List<Player> activeRacers;
            lock (room.SyncRoot)
            {
                if (!room.IsStarted) return;
                room.IsStarted = false;
                room.IsFinished = true;
                room.GameEndTimeUtc = DateTime.UtcNow;
                activeRacers = room.Players.Where(player => !player.IsSpectator).ToList();
            }

            if (activeRacers.Count == 0)
            {
                _stateStore.MarkDirty(room);
                return;
            }

            var finalRanking = activeRacers
                .OrderByDescending(player => player.LapCount)
                .ThenByDescending(player => player.TileIndex)
                .Select(PlayerSnapshot.From)
                .ToList();
            var winner = finalRanking[0];

            await _hubContext.Clients.Group(room.RoomCode).SendAsync(
                "StatusUpdate",
                "[Server] Hết giờ chơi! Trận đấu kết thúc.",
                "log-trap",
                cancellationToken);
            await _hubContext.Clients.Group(room.RoomCode).SendAsync(
                "GameFinished", winner, finalRanking, cancellationToken);

            _stateStore.MarkDirty(room);
            _logger.LogInformation("Room {RoomCode} finished by timer.", roomCode);
        }
    }
}
