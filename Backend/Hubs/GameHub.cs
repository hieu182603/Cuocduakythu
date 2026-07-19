using Microsoft.AspNetCore.SignalR;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;

namespace Backend.Hubs
{
    /// <summary>
    /// SignalR hub for real-time game synchronization.
    /// All gameplay logic is delegated to IGameService.
    /// All data access is delegated to repositories.
    /// This hub only handles: connection management, RPC dispatch, and event broadcasting.
    /// </summary>
    public class GameHub : Hub
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IPlayerRepository _playerRepo;
        private readonly IQuestionBank _questionBank;
        private readonly IGameService _gameService;
        private readonly IRoomStateStore _stateStore;
        private readonly IRoomTimerService _timerService;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            IRoomRepository roomRepo,
            IPlayerRepository playerRepo,
            IQuestionBank questionBank,
            IGameService gameService,
            IRoomStateStore stateStore,
            IRoomTimerService timerService,
            IHubContext<GameHub> hubContext,
            ILogger<GameHub> logger)
        {
            _roomRepo = roomRepo;
            _playerRepo = playerRepo;
            _questionBank = questionBank;
            _gameService = gameService;
            _stateStore = stateStore;
            _timerService = timerService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // ════════════════════════════════════════
        // ROOM MANAGEMENT
        // ════════════════════════════════════════

        public async Task CreateRoom(string playerName, string horseId, string sessionToken)
        {
            playerName = NormalizePlayerName(playerName);
            horseId = ValidateHorseId(horseId);
            sessionToken = ValidateSessionToken(sessionToken);
            await LeaveExistingRoomAsync(sessionToken);

            string roomCode = _gameService.GenerateRoomCode();
            while (_roomRepo.RoomExists(roomCode))
                roomCode = _gameService.GenerateRoomCode();

            var room = _roomRepo.CreateRoom(roomCode);

            Player host;
            lock (room.SyncRoot)
            {
                host = new Player
                {
                    Id = room.NextPlayerId++,
                    Name = playerName,
                    HorseId = horseId,
                    ConnectionId = Context.ConnectionId,
                    IsHost = true,
                    SessionToken = sessionToken
                };
                room.Players.Add(host);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Caller.SendAsync("RoomCreated", roomCode, room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
            _stateStore.MarkDirty(room);
            _logger.LogInformation("✨ [CreateRoom] Host {PlayerName} created room {RoomCode}. ConnID: {ConnectionId}", playerName, roomCode, Context.ConnectionId);
        }

        public async Task JoinRoom(string roomCode, string playerName, string horseId, string sessionToken)
        {
            roomCode = NormalizeRoomCode(roomCode);
            playerName = NormalizePlayerName(playerName);
            horseId = ValidateHorseId(horseId);
            sessionToken = ValidateSessionToken(sessionToken);
            var room = _roomRepo.GetRoom(roomCode);

            if (room == null)
            {
                throw new HubException("Phòng không tồn tại.");
            }

            // A session can never remain attached to a second room.
            await LeaveExistingRoomAsync(sessionToken, roomCode);

            // Check if player is rejoining an existing room (even if started)
            Player? existingPlayer;
            lock (room.SyncRoot)
            {
                existingPlayer = room.Players.FirstOrDefault(p => p.SessionToken == sessionToken);
                if (existingPlayer != null) existingPlayer.ConnectionId = Context.ConnectionId;
            }
            if (existingPlayer != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

                if (room.IsStarted)
                {
                    var playersSnapshot = room.GetPlayersSnapshot();
                    int remainingMinutes = room.GameDurationMinutes;
                    if (room.GameStartTime.HasValue)
                    {
                        var elapsed = System.DateTime.UtcNow - room.GameStartTime.Value;
                        remainingMinutes = (int)System.Math.Max(1, room.GameDurationMinutes - elapsed.TotalMinutes);
                    }

                    var roomState = new
                    {
                        RoomCode = room.RoomCode,
                        IsStarted = room.IsStarted,
                        GameDurationMinutes = remainingMinutes,
                        Players = playersSnapshot.Select(PlayerSnapshot.From),
                        ActivePlayerIndex = room.ActivePlayerIndex
                    };

                    await Clients.Group(roomCode).SendAsync("StatusUpdate", $"[Hệ thống] {existingPlayer.Name} đã kết nối lại.", "log-reward");
                    await Clients.Caller.SendAsync("Rejoined", roomState);
                }
                else
                {
                    var playersSnapshot = room.GetPlayersSnapshot();
                    var publicPlayers = playersSnapshot.Select(PlayerSnapshot.From).ToList();
                    await Clients.Group(roomCode).SendAsync("PlayerJoined", publicPlayers);
                    await Clients.Caller.SendAsync("RoomCreated", room.RoomCode, publicPlayers);
                }
                _stateStore.MarkDirty(room);
                return;
            }

            Player newPlayer;
            lock (room.SyncRoot)
            {
                if (room.IsStarted) throw new HubException("Trận đấu đã bắt đầu.");
                if (room.Players.Count >= 50) throw new HubException("Phòng đã đầy.");
                newPlayer = new Player
                {
                    Id = room.NextPlayerId++,
                    Name = playerName,
                    HorseId = horseId,
                    ConnectionId = Context.ConnectionId,
                    SessionToken = sessionToken
                };
                room.Players.Add(newPlayer);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
            _stateStore.MarkDirty(room);
            _logger.LogInformation("👤 [JoinRoom] Player {PlayerName} (Horse: {HorseId}) joined room {RoomCode}. ConnID: {ConnectionId}", playerName, horseId, roomCode, Context.ConnectionId);
        }

        public async Task UpdatePlayerName(string roomCode, string newName)
        {
            roomCode = NormalizeRoomCode(roomCode);
            newName = NormalizePlayerName(newName);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null) return;

            lock (room.SyncRoot)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player == null) return;
                player.Name = newName;
            }
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
            _stateStore.MarkDirty(room);
        }

        public async Task UpdatePlayerHorse(string roomCode, string horseId)
        {
            roomCode = NormalizeRoomCode(roomCode);
            horseId = ValidateHorseId(horseId);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || room.IsStarted || string.IsNullOrWhiteSpace(horseId)) return;

            lock (room.SyncRoot)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player == null) return;
                player.HorseId = horseId.Trim();
            }
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
            _stateStore.MarkDirty(room);
        }

        public async Task StartGame(string roomCode, bool isHostSpectator, int durationMinutes)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null)
            {
                throw new HubException("Phòng không tồn tại.");
            }

            // Only host can start
            var host = room.GetPlayersSnapshot().FirstOrDefault(p => p.IsHost);
            if (host == null || host.ConnectionId != Context.ConnectionId)
            {
                await Clients.Caller.SendAsync("Error", "Chỉ chủ phòng mới có quyền bắt đầu trận đấu.");
                return;
            }

            lock (room.SyncRoot)
            {
                if (room.IsStarted)
                    throw new HubException("Cuộc đua đã bắt đầu.");
                if (room.IsStarting)
                    throw new HubException("Máy chủ đang tải câu hỏi. Vui lòng chờ.");
                room.IsStarting = true;
            }

            // Apply host spectator mode
            host.IsSpectator = isHostSpectator;

            if (!room.GetPlayersSnapshot().Any(p => !p.IsSpectator))
            {
                room.IsStarting = false;
                throw new HubException("Cần ít nhất một tay đua để bắt đầu.");
            }

            // Setup duration limit
            if (durationMinutes is < 1 or > 180)
            {
                room.IsStarting = false;
                throw new HubException("Game duration must be between 1 and 180 minutes.");
            }
            room.GameDurationMinutes = durationMinutes;

            // Only wait for the first batch. Remaining batches continue loading
            // in the background while the race is running.
            try
            {
                using var questionTimeout = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted);
                questionTimeout.CancelAfter(TimeSpan.FromSeconds(20));
                await _questionBank.EnsureLoadedAsync(questionTimeout.Token);
            }
            catch (OperationCanceledException ex)
            {
                room.IsStarting = false;
                _logger.LogError(ex, "[StartGame] Timed out waiting for the first question batch in room {RoomCode}.", roomCode);
                throw new HubException("Batch câu hỏi đầu tiên chưa tải xong. Vui lòng thử lại.");
            }
            catch (System.Exception ex)
            {
                room.IsStarting = false;
                _logger.LogError(ex, "[StartGame] Failed to load questions from database for room {RoomCode}.", roomCode);
                throw new HubException("Không thể tải câu hỏi từ database. Cuộc đua chưa bắt đầu.");
            }

            lock (room.SyncRoot)
            {
                room.IsStarted = true;
                room.IsStarting = false;
                room.IsFinished = false;
                room.GameStartTime = System.DateTime.UtcNow;
                room.GameEndTimeUtc = room.GameStartTime.Value.AddMinutes(room.GameDurationMinutes);
            }

            // Set active index to first non-spectator player
            var playersAtStart = room.GetPlayersSnapshot();
            var firstRacer = playersAtStart.FirstOrDefault(p => !p.IsSpectator);
            if (firstRacer != null)
            {
                room.ActivePlayerIndex = playersAtStart.IndexOf(firstRacer);
            }
            else
            {
                room.ActivePlayerIndex = 0;
            }

            await Clients.Group(roomCode).SendAsync("GameStarted", playersAtStart.Select(PlayerSnapshot.From), room.ActivePlayerIndex, room.GameDurationMinutes, room.GameEndTimeUtc);
            _logger.LogInformation("🚀 [StartGame] Room {RoomCode} has started! Players count: {Count} | Duration: {Duration} min", roomCode, playersAtStart.Count, room.GameDurationMinutes);

            _timerService.Schedule(room);
            _stateStore.MarkDirty(room);
        }

        // ════════════════════════════════════════
        // GAMEPLAY ACTIONS
        // ════════════════════════════════════════

        public async Task RollDice(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted || room.IsFinished)
                throw new HubException("Cuộc đua chưa bắt đầu hoặc đã kết thúc.");

            var player = room.GetPlayersSnapshot().FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
                throw new HubException("Không tìm thấy tay đua của kết nối hiện tại.");
            if (player.IsSpectator)
                throw new HubException("Khán giả không thể tung xúc xắc.");
            var remainingFreezeMs = player.GetRemainingFreezeTimeMs();
            if (remainingFreezeMs > 0)
                throw new HubException($"Bạn đang bị đóng băng, còn {Math.Ceiling(remainingFreezeMs / 1000d)} giây.");
            if (!string.IsNullOrEmpty(player.CurrentQuestionId))
                throw new HubException("Bạn cần hoàn thành câu hỏi hiện tại trước.");
            if (player.PendingTileEventType != null)
                throw new HubException("Bạn cần hoàn thành sự kiện hiện tại trước.");

            lock (player)
            {
                if (player.Phase != PlayerPhase.Ready || player.IsRolling || DateTime.UtcNow < player.NextRollAllowedUtc)
                    throw new HubException("Đang xử lý xúc xắc, vui lòng chờ giây lát...");
                player.IsRolling = true;
                player.Phase = PlayerPhase.Rolling;
            }

            try
            {
                player.IsAutoRoll = false;

                // Calculate roll
                var (rollVal1, rollVal2, totalMove) = _gameService.CalculateDiceRoll(player);
                await Clients.Caller.SendAsync("DiceRolled", player.Name, rollVal1, rollVal2, totalMove);

                // Move player
                _gameService.MovePlayer(player, totalMove);
                await BroadcastPlayerMovement(roomCode, player, "forward", totalMove, 1500);
                _logger.LogDebug("🎲 [RollDice] Room {RoomCode} | Player {PlayerName} rolled ({Val1}, {Val2}) -> Moved {Total} steps to Tile {Tile}", roomCode, player.Name, rollVal1, rollVal2, totalMove, player.TileIndex);

                // Process tile landing
                await ProcessTileLanding(roomCode, room, player);
            }
            finally
            {
                lock (player)
                {
                    player.IsRolling = false;
                    if (player.Phase == PlayerPhase.Rolling)
                    {
                        player.Phase = PlayerPhase.Ready;
                        player.NextRollAllowedUtc = DateTime.UtcNow.AddSeconds(0.5);
                    }
                }
                _stateStore.MarkDirty(room);
            }
        }

        public async Task ProcessNewTileLanding(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.IsSpectator) return;

            lock (player)
            {
                if (player.Phase != PlayerPhase.Landing || !player.CanProcessNewTileLanding)
                    throw new HubException("Không có ô mới nào cần xử lý.");
                player.CanProcessNewTileLanding = false;
                player.Phase = PlayerPhase.Rolling;
            }

            await ProcessTileLanding(roomCode, room, player);
            _stateStore.MarkDirty(room);
        }

        public async Task ResolveTrap(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            int landingTileIndex;
            string trapName, trapDetail, movementDirection;
            int movementSteps;
            lock (player)
            {
                if (player.Phase != PlayerPhase.Trap || player.PendingTileEventType != "trap")
                    throw new HubException("Trap already resolved.");
                landingTileIndex = player.PendingTileIndex;
                (trapName, trapDetail, movementDirection, movementSteps) = _gameService.ApplyTrap(player);
                player.PendingTileEventType = null;
                player.CanProcessNewTileLanding = player.TileIndex != landingTileIndex && player.TileIndex != 0;
                player.Phase = player.CanProcessNewTileLanding ? PlayerPhase.Landing : PlayerPhase.ResultAcknowledgement;
            }

            await Clients.Caller.SendAsync(
                "TrapResolved",
                player.Id,
                player.Name,
                trapName,
                trapDetail,
                player.TileIndex,
                player.FreezeTimeMs,
                landingTileIndex);
            await BroadcastPlayerMovement(roomCode, player, movementDirection, movementSteps);
            await Clients.Group(roomCode).SendAsync(
                "StatusUpdate",
                $"[Bẫy] {player.Name} kích hoạt: {trapName} ({trapDetail})",
                "log-trap");
            _stateStore.MarkDirty(room);
        }

        public async Task ResolveReward(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            int landingTileIndex;
            string rewardName, rewardDetail, movementDirection;
            bool isAutoRoll;
            int movementSteps;
            lock (player)
            {
                if (player.Phase != PlayerPhase.Reward || player.PendingTileEventType != "reward")
                    throw new HubException("Reward already resolved.");
                landingTileIndex = player.PendingTileIndex;
                (rewardName, rewardDetail, isAutoRoll, movementDirection, movementSteps) = _gameService.ApplyReward(player);
                player.PendingTileEventType = null;
                player.CanProcessNewTileLanding = player.TileIndex != landingTileIndex && player.TileIndex != 0;
                player.Phase = player.CanProcessNewTileLanding ? PlayerPhase.Landing : PlayerPhase.ResultAcknowledgement;
            }

            await Clients.Caller.SendAsync(
                "RewardResolved",
                player.Id,
                player.Name,
                rewardName,
                rewardDetail,
                player.TileIndex,
                player.Shield,
                player.DoubleDice,
                isAutoRoll,
                landingTileIndex);
            await BroadcastPlayerMovement(roomCode, player, movementDirection, movementSteps);
            await Clients.Group(roomCode).SendAsync(
                "StatusUpdate",
                $"[Thưởng] {player.Name} nhận được: {rewardName} ({rewardDetail})",
                "log-reward");
            _stateStore.MarkDirty(room);
        }

        public async Task SubmitAnswer(string roomCode, int answerIndex)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            McqQuestion answeredQuestion;
            lock (player)
            {
                if (player.Phase != PlayerPhase.Question || string.IsNullOrEmpty(player.CurrentQuestionId))
                    throw new HubException("Question already answered.");
                answeredQuestion = _questionBank.GetById(player.CurrentQuestionId)
                    ?? throw new HubException("Current question is unavailable.");
                if (answerIndex < 0 || answerIndex >= answeredQuestion.Options.Count)
                    throw new HubException("Invalid answer.");
                player.CurrentQuestionId = null;
                player.Phase = PlayerPhase.Rolling;
            }

            int previousTileIndex = player.TileIndex;
            var (isCorrect, penaltyText) = _gameService.ProcessAnswer(player, answeredQuestion, answerIndex);

            // Find correct index for UI highlight
            int correctIndex = answeredQuestion.Options
                .OrderBy(o => o.OptionLetter)
                .ToList()
                .FindIndex(o => o.IsCorrect);

            // Only broadcast a movement when the answer penalty actually changed position.
            if (player.TileIndex != previousTileIndex)
            {
                await BroadcastPlayerMovement(
                    roomCode,
                    player,
                    "backward",
                    previousTileIndex - player.TileIndex);
                
                await ProcessTileLanding(roomCode, room, player);
            }
            else
            {
                player.Phase = PlayerPhase.ResultAcknowledgement;
            }

            // Tell the client whether its result popup can be acknowledged now.
            await Clients.Caller.SendAsync("AnswerOutcome", player.Id, player.Name, isCorrect, correctIndex, player.WrongStreak, penaltyText, player.Phase == PlayerPhase.ResultAcknowledgement);

            // Log status update
            string logMsg = isCorrect ? $"[Hệ thống] {player.Name} trả lời ĐÚNG câu hỏi!" : $"[Hệ thống] {player.Name} trả lời SAI câu hỏi! {penaltyText}";
            await Clients.Group(roomCode).SendAsync("StatusUpdate", logMsg, isCorrect ? "log-question" : "log-trap");
            _logger.LogDebug("📝 [SubmitAnswer] Room {RoomCode} | Player {PlayerName} -> Option: {AnswerIndex} | IsCorrect: {IsCorrect} | Tile: {Tile}", roomCode, player.Name, answerIndex, isCorrect, player.TileIndex);
            _stateStore.MarkDirty(room);
        }

        public async Task SpinWheel(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.IsSpectator) return;

            int landingTileIndex, sectorIdx, movementSteps;
            string label, desc, movementDirection;
            bool isReward;
            lock (player)
            {
                if (player.Phase != PlayerPhase.Wheel || player.PendingTileEventType != "wheel")
                    throw new HubException("Wheel already resolved.");
                landingTileIndex = player.PendingTileIndex;
                player.PendingTileEventType = null;
                (sectorIdx, label, desc, isReward, movementDirection, movementSteps) = _gameService.SpinWheel(player);
                player.CanProcessNewTileLanding = player.TileIndex != landingTileIndex && player.TileIndex != 0;
                player.Phase = player.CanProcessNewTileLanding ? PlayerPhase.Landing : PlayerPhase.ResultAcknowledgement;
            }

            // Send WheelSpun ONLY to caller
            await Clients.Caller.SendAsync("WheelSpun",
                player.Id, player.Name, sectorIdx, label, desc, isReward,
                player.TileIndex, player.FreezeTimeMs, player.Shield,
                player.IsAutoRoll);

            // Broadcast movement and status log to group
            await BroadcastPlayerMovement(roomCode, player, movementDirection, movementSteps, 3000);
            
            string logMsg = $"[Vòng Quay] {player.Name} quay trúng: {label} ({desc})";
            await Clients.Group(roomCode).SendAsync("StatusUpdate", logMsg, isReward ? "log-reward" : "log-trap");
            _logger.LogDebug("🎡 [SpinWheel] Room {RoomCode} | Player {PlayerName} spun -> Sector {Idx}: {Label} ({Desc}) | Tile: {Tile}", roomCode, player.Name, sectorIdx, label, desc, player.TileIndex);
            _stateStore.MarkDirty(room);
        }

        public async Task CloseModal(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            var player = room?.GetPlayersSnapshot().FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (room == null || player == null || !room.IsStarted) return;

            lock (player)
            {
                if (player.Phase != PlayerPhase.ResultAcknowledgement)
                    throw new HubException("No result is waiting for acknowledgement.");
                player.Phase = PlayerPhase.Ready;
                player.NextRollAllowedUtc = DateTime.UtcNow;
            }

            await Clients.Caller.SendAsync("EnableRollDice");
            _stateStore.MarkDirty(room);
        }

        // ════════════════════════════════════════
        // INTERNAL: TILE PROCESSING
        // ════════════════════════════════════════

        private Task BroadcastPlayerMovement(
            string roomCode,
            Player player,
            string movementDirection,
            int movementSteps,
            int delayBeforeMs = 0)
        {
            return Clients.Group(roomCode).SendAsync(
                "PlayerMoved",
                player.Id,
                player.TileIndex,
                player.LapCount,
                movementDirection,
                movementSteps,
                delayBeforeMs);
        }

        private async Task ProcessTileLanding(string roomCode, GameRoom room, Player player)
        {
            await Task.Delay(1000); // Wait for hop animation on clients

            string tileType = _gameService.GetTileType(player.TileIndex);

            switch (tileType)
            {
                case "start":
                    player.Phase = PlayerPhase.Ready;
                    player.NextRollAllowedUtc = DateTime.UtcNow;
                    await Clients.Group(roomCode).SendAsync("StatusUpdate", $"[Hệ thống] {player.Name} dừng tại ô Xuất Phát.", "log-win");
                    await Clients.Client(player.ConnectionId).SendAsync("EnableRollDice");
                    break;

                case "question":
                    if (_questionBank.Count == 0)
                        await _questionBank.EnsureLoadedAsync();
                    var question = _questionBank.GetRandom();
                    if (question == null)
                    {
                        _logger.LogError("No database question available for room {RoomCode}.", roomCode);
                        await Clients.Client(player.ConnectionId).SendAsync("Error", "Không có câu hỏi hợp lệ trong database.");
                        player.Phase = PlayerPhase.ResultAcknowledgement;
                        await Clients.Client(player.ConnectionId).SendAsync("EnableRollDice");
                        return;
                    }
                    
                    player.CurrentQuestionId = question.Id;
                    player.Phase = PlayerPhase.Question;
                    var answers = question.Options.OrderBy(o => o.OptionLetter).Select(o => o.OptionText).ToList();
                    await Clients.Client(player.ConnectionId).SendAsync("TriggerQuestion", player.Id, player.Name, question.QuestionText, answers, player.WrongStreak);
                    break;

                case "trap":
                    {
                        if (player.Shield)
                        {
                            player.Shield = false;
                            player.Phase = PlayerPhase.ResultAcknowledgement;
                            await Clients.Client(player.ConnectionId).SendAsync("TriggerShieldBlock", player.Id, player.Name);
                        }
                        else
                        {
                            player.PendingTileEventType = "trap";
                            player.PendingTileIndex = player.TileIndex;
                            player.Phase = PlayerPhase.Trap;
                            await Clients.Client(player.ConnectionId).SendAsync("TriggerTrap", player.Id, player.Name);
                        }
                    }
                    break;

                case "reward":
                    {
                        player.PendingTileEventType = "reward";
                        player.PendingTileIndex = player.TileIndex;
                        player.Phase = PlayerPhase.Reward;
                        await Clients.Client(player.ConnectionId).SendAsync("TriggerReward", player.Id, player.Name);
                    }
                    break;

                case "wheel":
                    player.PendingTileEventType = "wheel";
                    player.PendingTileIndex = player.TileIndex;
                    player.Phase = PlayerPhase.Wheel;
                    await Clients.Client(player.ConnectionId).SendAsync("TriggerWheel", player.Id, player.Name);
                    break;
            }
        }



        public async Task ForceEndGame(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            // Only allow host to force end the game
            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || !player.IsHost) return;

            _logger.LogInformation("⚠️ [ForceEndGame] Host {PlayerName} requested to end the game in Room {RoomCode}.", player.Name, roomCode);
            await _timerService.EndGameAsync(roomCode);
        }

        private static async Task EndGameDueToTimeoutInternal(
            string roomCode, 
            IHubContext<GameHub> hubContext, 
            IRoomRepository roomRepo, 
            ILogger logger)
        {
            var room = roomRepo.GetRoom(roomCode);
            if (room == null) return;

            List<Player> activeRacers;
            lock (room.SyncRoot)
            {
                if (!room.IsStarted) return;
                room.IsStarted = false;
                room.IsFinished = true;
                activeRacers = room.Players.Where(p => !p.IsSpectator).ToList();
            }

            if (room.GameTimer != null)
            {
                room.GameTimer.Dispose();
                room.GameTimer = null;
            }

            if (activeRacers.Count > 0)
            {
                var winner = activeRacers
                    .OrderByDescending(p => p.LapCount)
                    .ThenByDescending(p => p.TileIndex)
                    .First();
                logger.LogInformation("⏰ [Timer] Room {RoomCode} timeout reached. Ending game automatically.", roomCode);
                await hubContext.Clients.Group(room.RoomCode).SendAsync("StatusUpdate", $"[Server] Hết giờ chơi! Trận đấu kết thúc.", "log-trap");
                var finalRanking = activeRacers
                    .OrderByDescending(p => p.LapCount)
                    .ThenByDescending(p => p.TileIndex)
                    .Select(PlayerSnapshot.From)
                    .ToList();
                await hubContext.Clients.Group(room.RoomCode).SendAsync("GameFinished", PlayerSnapshot.From(winner), finalRanking);
            }
        }

        public async Task EndGameDueToTimeout(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            var caller = room?.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (room == null || caller == null || !caller.IsHost)
                throw new HubException("Chỉ chủ phòng mới có quyền kết thúc trận đấu.");

            await _timerService.EndGameAsync(roomCode);
        }

        public async Task RejoinRoom(string roomCode, string sessionToken)
        {
            roomCode = NormalizeRoomCode(roomCode);
            sessionToken = ValidateSessionToken(sessionToken);
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Phòng không tồn tại hoặc đã bị xóa.");
                return;
            }

            var player = room.GetPlayersSnapshot().FirstOrDefault(p => p.SessionToken == sessionToken);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Không tìm thấy thông tin người chơi trong phòng này.");
                return;
            }

            if (room.RestoredAtUtc.HasValue && DateTime.UtcNow - room.RestoredAtUtc.Value > TimeSpan.FromMinutes(10))
            {
                await Clients.Caller.SendAsync("Error", "Thời gian kết nối lại phòng đã hết.");
                return;
            }

            // Update connection ID
            await LeaveExistingRoomAsync(sessionToken, roomCode);
            lock (room.SyncRoot)
            {
                player.ConnectionId = Context.ConnectionId;
            }

            // Re-add to Hub Group
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            // Keep the absolute deadline. The browser must not restart the timer
            // from the reconnect moment.
            var remainingMinutes = room.GameEndTimeUtc.HasValue
                ? Math.Max(0, (int)Math.Ceiling((room.GameEndTimeUtc.Value - DateTime.UtcNow).TotalMinutes))
                : room.GameDurationMinutes;

            // Broadcast that player reconnected
            await Clients.Group(roomCode).SendAsync("StatusUpdate", $"[Hệ thống] {player.Name} đã kết nối lại.", "log-reward");

            var rejoinPlayersSnapshot = room.GetPlayersSnapshot();
            var roomState = new
            {
                RoomCode = room.RoomCode,
                IsStarted = room.IsStarted,
                IsFinished = room.IsFinished,
                GameDurationMinutes = remainingMinutes,
                GameEndTimeUtc = room.GameEndTimeUtc,
                Players = rejoinPlayersSnapshot.Select(PlayerSnapshot.From),
                ActivePlayerIndex = room.ActivePlayerIndex
            };

            await Clients.Caller.SendAsync("Rejoined", roomState);

            // Restore an interrupted personal event after reconnect. Without
            // this, RollDice is correctly rejected by the server but the client
            // has no modal available to finish the pending action.
            if (room.IsStarted)
            {
                if (!string.IsNullOrEmpty(player.CurrentQuestionId))
                {
                    // Re-send the current question
                    var question = _questionBank.GetById(player.CurrentQuestionId);
                    if (question == null)
                    {
                        player.CurrentQuestionId = null;
                        player.Phase = PlayerPhase.Ready;
                        await Clients.Caller.SendAsync("EnableRollDice");
                        return;
                    }
                    var answers = question.Options
                        .OrderBy(o => o.OptionLetter)
                        .Select(o => o.OptionText)
                        .ToList();
                    await Clients.Caller.SendAsync(
                        "TriggerQuestion",
                        player.Id,
                        player.Name,
                        question.QuestionText,
                        answers,
                        player.WrongStreak);
                }
                else if (player.PendingTileEventType == "trap")
                {
                    await Clients.Caller.SendAsync("TriggerTrap", player.Id, player.Name);
                }
                else if (player.PendingTileEventType == "reward")
                {
                    await Clients.Caller.SendAsync("TriggerReward", player.Id, player.Name);
                }
                else if (player.PendingTileEventType == "wheel")
                {
                    await Clients.Caller.SendAsync("TriggerWheel", player.Id, player.Name);
                }
            }
            _logger.LogInformation("🔄 [RejoinRoom] Player {PlayerName} reconnected to Room {RoomCode}. ConnID: {ConnectionId}", player.Name, roomCode, Context.ConnectionId);
        }

        // ════════════════════════════════════════
        // DISCONNECT HANDLER
        // ════════════════════════════════════════

        public async Task LeaveRoom(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode);
            var player = room?.GetPlayersSnapshot()
                .FirstOrDefault(item => item.ConnectionId == Context.ConnectionId);
            if (room == null || player == null) return;
            await RemovePlayerFromRoomAsync(room, player);
        }

        public async Task ResetGame(string roomCode)
        {
            roomCode = NormalizeRoomCode(roomCode);
            var room = _roomRepo.GetRoom(roomCode)
                ?? throw new HubException("Room does not exist.");
            var host = room.GetPlayersSnapshot()
                .FirstOrDefault(player => player.ConnectionId == Context.ConnectionId && player.IsHost);
            if (host == null)
                throw new HubException("Only the host can reset the game.");

            lock (room.SyncRoot)
            {
                if (!room.IsFinished || room.IsStarted)
                    throw new HubException("The game can only be reset after it finishes.");
                _timerService.Cancel(room);
                foreach (var player in room.Players)
                    player.ResetForRematch();
                room.IsStarted = false;
                room.IsStarting = false;
                room.IsFinished = false;
                room.ActivePlayerIndex = 0;
                room.GameStartTime = null;
                room.GameEndTimeUtc = null;
                room.RestoredAtUtc = null;
            }

            var players = room.GetPlayersSnapshot().Select(PlayerSnapshot.From).ToList();
            await Clients.Group(roomCode).SendAsync("GameReset", players);
            _stateStore.MarkDirty(room);
        }

        private async Task LeaveExistingRoomAsync(string sessionToken, string? exceptRoomCode = null)
        {
            var room = _roomRepo.FindRoomByConnection(Context.ConnectionId)
                ?? _roomRepo.FindRoomBySession(sessionToken);
            if (room == null || room.RoomCode == exceptRoomCode) return;

            var player = room.GetPlayersSnapshot().FirstOrDefault(item =>
                item.ConnectionId == Context.ConnectionId || item.SessionToken == sessionToken);
            if (player != null)
                await RemovePlayerFromRoomAsync(room, player);
        }

        private async Task RemovePlayerFromRoomAsync(GameRoom room, Player player)
        {
            List<Player> remainingPlayers;
            lock (room.SyncRoot)
            {
                if (!room.Players.Remove(player)) return;
                if (player.IsHost && room.Players.Count > 0)
                    room.Players[0].IsHost = true;
                remainingPlayers = room.Players.ToList();
            }

            if (!string.IsNullOrWhiteSpace(player.ConnectionId))
                await Groups.RemoveFromGroupAsync(player.ConnectionId, room.RoomCode);

            if (remainingPlayers.Count == 0)
            {
                _timerService.Cancel(room);
                _roomRepo.RemoveRoom(room.RoomCode);
                await _stateStore.DeleteAsync(room.RoomCode);
                return;
            }

            await Clients.Group(room.RoomCode).SendAsync(
                "PlayerDisconnected", player.Name, remainingPlayers.Select(PlayerSnapshot.From));
            _stateStore.MarkDirty(room);
        }

        private static string NormalizeRoomCode(string roomCode)
        {
            var normalized = (roomCode ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.Length != 5 || normalized.Any(character => character is < 'A' or > 'Z'))
                throw new HubException("Room code must contain exactly five letters.");
            return normalized;
        }

        private static string NormalizePlayerName(string playerName)
        {
            var normalized = (playerName ?? string.Empty).Trim();
            if (normalized.Length == 0 || normalized.Length > 24 || normalized.Any(char.IsControl))
                throw new HubException("Player name must contain 1 to 24 characters without control characters.");
            return normalized;
        }

        private static string ValidateHorseId(string horseId)
        {
            var normalized = (horseId ?? string.Empty).Trim();
            if (normalized.Length != 2 || !int.TryParse(normalized, out var value) || value < 1 || value > 10)
                throw new HubException("Horse ID must be between 01 and 10.");
            return value.ToString("00");
        }

        private static string ValidateSessionToken(string sessionToken)
        {
            var normalized = (sessionToken ?? string.Empty).Trim();
            if (normalized.Length is < 16 or > 128 || normalized.Any(char.IsControl))
                throw new HubException("Invalid session token.");
            return normalized;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            foreach (var room in _roomRepo.GetAllRooms())
            {
                var player = _playerRepo.GetPlayer(room, Context.ConnectionId);
                if (player != null)
                {
                    if (!room.IsStarted)
                    {
                        // 3 seconds grace period to allow refreshing without leaving the lobby
                        string connId = Context.ConnectionId;
                        string rCode = room.RoomCode;
                        string pName = player.Name;

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            var currentRoom = _roomRepo.GetRoom(rCode);
                            if (currentRoom != null && !currentRoom.IsStarted)
                            {
                                // Check if they haven't reconnected (ConnectionId is still the disconnected one)
                                Player? p;
                                List<Player> remainingPlayers;
                                lock (currentRoom.SyncRoot)
                                {
                                    p = currentRoom.Players.FirstOrDefault(x => x.ConnectionId == connId);
                                    if (p != null)
                                    {
                                        bool wasHost = p.IsHost;
                                        currentRoom.Players.Remove(p);
                                        if (wasHost && currentRoom.Players.Count > 0)
                                        {
                                            var newHost = currentRoom.Players.First();
                                            newHost.IsHost = true;
                                            _logger.LogWarning("👑 [HostMigration] Host {OldHost} left. Reassigned host role to player {NewHost}.", pName, newHost.Name);
                                        }
                                    }
                                    remainingPlayers = currentRoom.Players.ToList();
                                }
                                if (p != null)
                                {
                                    await Clients.Group(rCode).SendAsync("PlayerDisconnected", pName, remainingPlayers.Select(PlayerSnapshot.From));
                                    _logger.LogWarning("❌ [Disconnect] Player {PlayerName} left room {RoomCode} (Lobby).", pName, rCode);

                                    if (remainingPlayers.Count == 0)
                                    {
                                        _roomRepo.RemoveRoom(rCode);
                                        await _stateStore.DeleteAsync(rCode);
                                        _logger.LogWarning("🧹 [Cleanup] Room {RoomCode} has no active players. Removing room.", rCode);
                                    }
                                    else
                                    {
                                        _stateStore.MarkDirty(currentRoom);
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        // In game: mark player disconnected, do not remove
                        lock (player) player.ConnectionId = string.Empty;
                        _stateStore.MarkDirty(room);
                        await Clients.Group(room.RoomCode).SendAsync("PlayerDisconnected", player.Name, room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
                        _logger.LogWarning("🔌 [Disconnect] Player {PlayerName} disconnected from active room {RoomCode}.", player.Name, room.RoomCode);

                        // If all connections are lost, clean up the room after a 10 seconds grace period
                        string rCode = room.RoomCode;
                        _ = Task.Run(async () =>
                        {
                            var gracePeriod = room.RestoredAtUtc.HasValue
                                ? TimeSpan.FromMinutes(10)
                                : TimeSpan.FromSeconds(10);
                            await Task.Delay(gracePeriod);
                            var currentRoom = _roomRepo.GetRoom(rCode);
                            if (currentRoom != null && currentRoom.IsStarted)
                            {
                                if (currentRoom.GetPlayersSnapshot().All(p => string.IsNullOrEmpty(p.ConnectionId)))
                                {
                                    _roomRepo.RemoveRoom(rCode);
                                    await _stateStore.DeleteAsync(rCode);
                                    _logger.LogWarning("🧹 [Cleanup] Room {RoomCode} has no active players. Removing room.", rCode);
                                }
                            }
                        });
                    }
                    break;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
