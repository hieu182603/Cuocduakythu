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
        private readonly IRoomQuestionLoader _questionLoader;
        private readonly IGameService _gameService;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            IRoomRepository roomRepo,
            IPlayerRepository playerRepo,
            IRoomQuestionLoader questionLoader,
            IGameService gameService,
            IHubContext<GameHub> hubContext,
            ILogger<GameHub> logger)
        {
            _roomRepo = roomRepo;
            _playerRepo = playerRepo;
            _questionLoader = questionLoader;
            _gameService = gameService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // ════════════════════════════════════════
        // ROOM MANAGEMENT
        // ════════════════════════════════════════

        public async Task CreateRoom(string playerName, string horseId, string sessionToken)
        {
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

            // Start loading questions immediately. The first batch is usually
            // ready before the host finishes configuring the lobby.
            _questionLoader.Start(room);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Caller.SendAsync("RoomCreated", roomCode, room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
            _logger.LogInformation("✨ [CreateRoom] Host {PlayerName} created room {RoomCode}. ConnID: {ConnectionId}", playerName, roomCode, Context.ConnectionId);
        }

        public async Task JoinRoom(string roomCode, string playerName, string horseId, string sessionToken)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);

            if (room == null)
            {
                throw new HubException("Phòng không tồn tại.");
            }

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
            _logger.LogInformation("👤 [JoinRoom] Player {PlayerName} (Horse: {HorseId}) joined room {RoomCode}. ConnID: {ConnectionId}", playerName, horseId, roomCode, Context.ConnectionId);
        }

        public async Task UpdatePlayerName(string roomCode, string newName)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null) return;

            lock (room.SyncRoot)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player == null) return;
                player.Name = newName.Trim();
            }
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
        }

        public async Task UpdatePlayerHorse(string roomCode, string horseId)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || room.IsStarted || string.IsNullOrWhiteSpace(horseId)) return;

            lock (room.SyncRoot)
            {
                var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player == null) return;
                player.HorseId = horseId.Trim();
            }
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
        }

        public async Task StartGame(string roomCode, bool isHostSpectator, int durationMinutes)
        {
            roomCode = roomCode.ToUpper().Trim();
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
            room.GameDurationMinutes = durationMinutes > 0 ? durationMinutes : 30;

            // Only wait for the first batch. Remaining batches continue loading
            // in the background while the race is running.
            try
            {
                using var questionTimeout = CancellationTokenSource.CreateLinkedTokenSource(Context.ConnectionAborted);
                questionTimeout.CancelAfter(TimeSpan.FromSeconds(20));
                await _questionLoader.WaitForFirstBatchAsync(room, questionTimeout.Token);
            }
            catch (HubException)
            {
                room.IsStarting = false;
                throw;
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

            await Clients.Group(roomCode).SendAsync("GameStarted", playersAtStart.Select(PlayerSnapshot.From), room.ActivePlayerIndex, room.GameDurationMinutes);
            _logger.LogInformation("🚀 [StartGame] Room {RoomCode} has started! Players count: {Count} | Duration: {Duration} min", roomCode, playersAtStart.Count, room.GameDurationMinutes);

            // Setup server-side timer to end game when time expires
            if (room.GameTimer != null)
            {
                room.GameTimer.Dispose();
                room.GameTimer = null;
            }

            room.GameTimer = new System.Threading.Timer(async _ =>
            {
                try
                {
                    await EndGameDueToTimeoutInternal(roomCode, _hubContext, _roomRepo, _logger);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Error in server-side game timer callback for room {RoomCode}", roomCode);
                }
            }, null, System.TimeSpan.FromMinutes(room.GameDurationMinutes), System.Threading.Timeout.InfiniteTimeSpan);
        }

        // ════════════════════════════════════════
        // GAMEPLAY ACTIONS
        // ════════════════════════════════════════

        public async Task RollDice(string roomCode)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted)
                throw new HubException("Cuộc đua chưa bắt đầu hoặc đã kết thúc.");

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
                throw new HubException("Không tìm thấy tay đua của kết nối hiện tại.");
            if (player.IsSpectator)
                throw new HubException("Khán giả không thể tung xúc xắc.");
            var remainingFreezeMs = player.GetRemainingFreezeTimeMs();
            if (remainingFreezeMs > 0)
                throw new HubException($"Bạn đang bị đóng băng, còn {Math.Ceiling(remainingFreezeMs / 1000d)} giây.");
            if (player.CurrentQuestion != null)
                throw new HubException("Bạn cần hoàn thành câu hỏi hiện tại trước.");
            if (player.PendingTileEventType != null)
                throw new HubException("Bạn cần hoàn thành sự kiện hiện tại trước.");

            lock (player)
            {
                if (player.IsRolling)
                    throw new HubException("Xúc xắc đang được xử lý.");
                player.IsRolling = true;
            }

            try
            {
                player.IsAutoRoll = false;

                // Calculate roll
                var (rollVal1, rollVal2, totalMove) = _gameService.CalculateDiceRoll(player);
                await Clients.Group(roomCode).SendAsync("DiceRolled", player.Name, rollVal1, rollVal2, totalMove);

                // Move player
                _gameService.MovePlayer(player, totalMove);
                await BroadcastPlayerMovement(roomCode, player, "forward", totalMove, 1500);
                _logger.LogInformation("🎲 [RollDice] Room {RoomCode} | Player {PlayerName} rolled ({Val1}, {Val2}) -> Moved {Total} steps to Tile {Tile}", roomCode, player.Name, rollVal1, rollVal2, totalMove, player.TileIndex);

                // Process tile landing
                await ProcessTileLanding(roomCode, room, player);
            }
            finally
            {
                lock (player)
                {
                    player.IsRolling = false;
                }
            }
        }

        public async Task ProcessNewTileLanding(string roomCode)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.IsSpectator) return;

            lock (player)
            {
                if (!player.CanProcessNewTileLanding)
                    throw new HubException("Không có ô mới nào cần xử lý.");
                player.CanProcessNewTileLanding = false;
            }

            await ProcessTileLanding(roomCode, room, player);
        }

        public async Task ResolveTrap(string roomCode)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            int landingTileIndex;
            string trapName, trapDetail, movementDirection;
            int movementSteps;
            lock (player)
            {
                if (player.PendingTileEventType != "trap") return;
                landingTileIndex = player.PendingTileIndex;
                (trapName, trapDetail, movementDirection, movementSteps) = _gameService.ApplyTrap(player);
                player.PendingTileEventType = null;
                player.CanProcessNewTileLanding = player.TileIndex != landingTileIndex && player.TileIndex != 0;
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
        }

        public async Task ResolveReward(string roomCode)
        {
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
                if (player.PendingTileEventType != "reward") return;
                landingTileIndex = player.PendingTileIndex;
                (rewardName, rewardDetail, isAutoRoll, movementDirection, movementSteps) = _gameService.ApplyReward(player);
                player.PendingTileEventType = null;
                player.CanProcessNewTileLanding = player.TileIndex != landingTileIndex && player.TileIndex != 0;
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
        }

        public async Task SubmitAnswer(string roomCode, int answerIndex)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            McqQuestion answeredQuestion;
            lock (player)
            {
                if (player.CurrentQuestion == null) return;
                answeredQuestion = player.CurrentQuestion;
                player.CurrentQuestion = null;
            }

            int previousTileIndex = player.TileIndex;
            var (isCorrect, penaltyText) = _gameService.ProcessAnswer(player, answeredQuestion, answerIndex);

            // Find correct index for UI highlight
            int correctIndex = answeredQuestion.Options
                .OrderBy(o => o.OptionLetter)
                .ToList()
                .FindIndex(o => o.IsCorrect);

            // Send outcome ONLY to caller
            await Clients.Caller.SendAsync("AnswerOutcome", player.Id, player.Name, isCorrect, correctIndex, player.WrongStreak, penaltyText);
            
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

            // Log status update
            string logMsg = isCorrect ? $"[Hệ thống] {player.Name} trả lời ĐÚNG câu hỏi!" : $"[Hệ thống] {player.Name} trả lời SAI câu hỏi! {penaltyText}";
            await Clients.Group(roomCode).SendAsync("StatusUpdate", logMsg, isCorrect ? "log-question" : "log-trap");
            _logger.LogInformation("📝 [SubmitAnswer] Room {RoomCode} | Player {PlayerName} -> Option: {AnswerIndex} | IsCorrect: {IsCorrect} | Tile: {Tile}", roomCode, player.Name, answerIndex, isCorrect, player.TileIndex);
        }

        public async Task SpinWheel(string roomCode)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.IsSpectator) return;

            int landingTileIndex, sectorIdx, movementSteps;
            string label, desc, movementDirection;
            bool isReward;
            lock (player)
            {
                if (player.PendingTileEventType != "wheel") return;
                landingTileIndex = player.PendingTileIndex;
                player.PendingTileEventType = null;
                (sectorIdx, label, desc, isReward, movementDirection, movementSteps) = _gameService.SpinWheel(player);
                player.CanProcessNewTileLanding = player.TileIndex != landingTileIndex && player.TileIndex != 0;
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
            _logger.LogInformation("🎡 [SpinWheel] Room {RoomCode} | Player {PlayerName} spun -> Sector {Idx}: {Label} ({Desc}) | Tile: {Tile}", roomCode, player.Name, sectorIdx, label, desc, player.TileIndex);
        }

        public async Task CloseModal(string roomCode)
        {
            await Task.CompletedTask;
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
                    await Clients.Group(roomCode).SendAsync("StatusUpdate", $"[Hệ thống] {player.Name} dừng tại ô Xuất Phát.", "log-win");
                    await Clients.Client(player.ConnectionId).SendAsync("EnableRollDice");
                    break;

                case "question":
                    var question = _gameService.GetRandomQuestion(room);
                    if (question == null)
                    {
                        _logger.LogError("No database question available for room {RoomCode}.", roomCode);
                        await Clients.Client(player.ConnectionId).SendAsync("Error", "Không có câu hỏi hợp lệ trong database.");
                        await Clients.Client(player.ConnectionId).SendAsync("EnableRollDice");
                        return;
                    }
                    
                    player.CurrentQuestion = question;
                    var answers = question.Options.OrderBy(o => o.OptionLetter).Select(o => o.OptionText).ToList();
                    await Clients.Client(player.ConnectionId).SendAsync("TriggerQuestion", player.Id, player.Name, question.QuestionText, answers, player.WrongStreak);
                    break;

                case "trap":
                    {
                        if (player.Shield)
                        {
                            player.Shield = false;
                            await Clients.Client(player.ConnectionId).SendAsync("TriggerShieldBlock", player.Id, player.Name);
                        }
                        else
                        {
                            player.PendingTileEventType = "trap";
                            player.PendingTileIndex = player.TileIndex;
                            await Clients.Client(player.ConnectionId).SendAsync("TriggerTrap", player.Id, player.Name);
                        }
                    }
                    break;

                case "reward":
                    {
                        player.PendingTileEventType = "reward";
                        player.PendingTileIndex = player.TileIndex;
                        await Clients.Client(player.ConnectionId).SendAsync("TriggerReward", player.Id, player.Name);
                    }
                    break;

                case "wheel":
                    player.PendingTileEventType = "wheel";
                    player.PendingTileIndex = player.TileIndex;
                    await Clients.Client(player.ConnectionId).SendAsync("TriggerWheel", player.Id, player.Name);
                    break;
            }
        }



        public async Task ForceEndGame(string roomCode)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            // Only allow host to force end the game
            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || !player.IsHost) return;

            _logger.LogInformation("⚠️ [ForceEndGame] Host {PlayerName} requested to end the game in Room {RoomCode}.", player.Name, roomCode);
            await EndGameDueToTimeoutInternal(roomCode, _hubContext, _roomRepo, _logger);
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
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            var caller = room?.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (room == null || caller == null || !caller.IsHost)
                throw new HubException("Chỉ chủ phòng mới có quyền kết thúc trận đấu.");

            await EndGameDueToTimeoutInternal(roomCode, _hubContext, _roomRepo, _logger);
        }

        public async Task RejoinRoom(string roomCode, string sessionToken)
        {
            roomCode = roomCode.ToUpper().Trim();
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

            // Update connection ID
            player.ConnectionId = Context.ConnectionId;

            // Re-add to Hub Group
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            // Calculate remaining game time if started
            int remainingMinutes = room.GameDurationMinutes;
            if (room.IsStarted && room.GameStartTime.HasValue)
            {
                var elapsed = System.DateTime.UtcNow - room.GameStartTime.Value;
                remainingMinutes = (int)System.Math.Max(1, room.GameDurationMinutes - elapsed.TotalMinutes);
            }

            // Broadcast that player reconnected
            await Clients.Group(roomCode).SendAsync("StatusUpdate", $"[Hệ thống] {player.Name} đã kết nối lại.", "log-reward");

            var rejoinPlayersSnapshot = room.GetPlayersSnapshot();
            var roomState = new
            {
                RoomCode = room.RoomCode,
                IsStarted = room.IsStarted,
                IsFinished = room.IsFinished,
                GameDurationMinutes = remainingMinutes,
                Players = rejoinPlayersSnapshot.Select(PlayerSnapshot.From),
                ActivePlayerIndex = room.ActivePlayerIndex
            };

            await Clients.Caller.SendAsync("Rejoined", roomState);

            // Restore an interrupted personal event after reconnect. Without
            // this, RollDice is correctly rejected by the server but the client
            // has no modal available to finish the pending action.
            if (room.IsStarted)
            {
                if (player.CurrentQuestion != null)
                {
                    var question = player.CurrentQuestion;
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
                                        _logger.LogWarning("🧹 [Cleanup] Room {RoomCode} has no active players. Removing room.", rCode);
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        // In game: mark player disconnected, do not remove
                        lock (player) player.ConnectionId = string.Empty;
                        await Clients.Group(room.RoomCode).SendAsync("PlayerDisconnected", player.Name, room.GetPlayersSnapshot().Select(PlayerSnapshot.From));
                        _logger.LogWarning("🔌 [Disconnect] Player {PlayerName} disconnected from active room {RoomCode}.", player.Name, room.RoomCode);

                        // If all connections are lost, clean up the room after a 10 seconds grace period
                        string rCode = room.RoomCode;
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(10000);
                            var currentRoom = _roomRepo.GetRoom(rCode);
                            if (currentRoom != null && currentRoom.IsStarted)
                            {
                                if (currentRoom.GetPlayersSnapshot().All(p => string.IsNullOrEmpty(p.ConnectionId)))
                                {
                                    _roomRepo.RemoveRoom(rCode);
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
