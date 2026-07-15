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
        private readonly IQuestionRepository _questionRepo;
        private readonly IGameService _gameService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(
            IRoomRepository roomRepo,
            IPlayerRepository playerRepo,
            IQuestionRepository questionRepo,
            IGameService gameService,
            ILogger<GameHub> logger)
        {
            _roomRepo = roomRepo;
            _playerRepo = playerRepo;
            _questionRepo = questionRepo;
            _gameService = gameService;
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

            var host = new Player
            {
                Id = room.NextPlayerId++,
                Name = playerName,
                HorseId = horseId,
                ConnectionId = Context.ConnectionId,
                IsHost = true,
                SessionToken = sessionToken
            };
            _playerRepo.AddPlayer(room, host);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Caller.SendAsync("RoomCreated", roomCode, room.Players);
            _logger.LogInformation("✨ [CreateRoom] Host {PlayerName} created room {RoomCode}. ConnID: {ConnectionId}", playerName, roomCode, Context.ConnectionId);
        }

        public async Task JoinRoom(string roomCode, string playerName, string horseId, string sessionToken)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);

            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Phòng không tồn tại.");
                return;
            }

            // Check if player is rejoining an existing room (even if started)
            var existingPlayer = room.Players.FirstOrDefault(p => p.SessionToken == sessionToken);
            if (existingPlayer != null)
            {
                existingPlayer.ConnectionId = Context.ConnectionId;
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

                if (room.IsStarted)
                {
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
                        Players = room.Players.Select(p => new
                        {
                            p.Id,
                            p.ConnectionId,
                            p.Name,
                            p.HorseId,
                            p.TileIndex,
                            p.WrongStreak,
                            p.Shield,
                            p.SkipTurn,
                            p.DoubleDice,
                            p.DiceModifier,
                            p.LapCount,
                            p.IsSpectator,
                            p.IsHost,
                            p.SessionToken
                        }),
                        ActivePlayerIndex = room.ActivePlayerIndex
                    };

                    await Clients.Group(roomCode).SendAsync("StatusUpdate", $"[Hệ thống] {existingPlayer.Name} đã kết nối lại.", "log-reward");
                    await Clients.Caller.SendAsync("Rejoined", roomState);
                }
                else
                {
                    await Clients.Group(roomCode).SendAsync("PlayerJoined", room.Players);
                    await Clients.Caller.SendAsync("RoomCreated", room.RoomCode, room.Players);
                }
                return;
            }

            if (room.IsStarted)
            {
                await Clients.Caller.SendAsync("Error", "Trận đấu đã bắt đầu.");
                return;
            }
            if (room.Players.Count >= 50)
            {
                await Clients.Caller.SendAsync("Error", "Phòng đã đầy.");
                return;
            }
            var newPlayer = new Player
            {
                Id = room.NextPlayerId++,
                Name = playerName,
                HorseId = horseId,
                ConnectionId = Context.ConnectionId,
                SessionToken = sessionToken
            };
            _playerRepo.AddPlayer(room, newPlayer);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.Players);
            _logger.LogInformation("👤 [JoinRoom] Player {PlayerName} (Horse: {HorseId}) joined room {RoomCode}. ConnID: {ConnectionId}", playerName, horseId, roomCode, Context.ConnectionId);
        }

        public async Task UpdatePlayerName(string roomCode, string newName)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            player.Name = newName.Trim();
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.Players);
        }

        public async Task UpdatePlayerHorse(string roomCode, string horseId)
        {
            roomCode = roomCode.ToUpper().Trim();
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || room.IsStarted || string.IsNullOrWhiteSpace(horseId)) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null) return;

            // Nhiều người chơi trong cùng phòng được phép dùng chung một nhân vật.
            player.HorseId = horseId.Trim();
            await Clients.Group(roomCode).SendAsync("PlayerJoined", room.Players);
        }

        public async Task StartGame(string roomCode, bool isHostSpectator, int durationMinutes)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null) return;

            // Only host can start
            var host = room.Players.FirstOrDefault(p => p.IsHost);
            if (host == null || host.ConnectionId != Context.ConnectionId)
            {
                await Clients.Caller.SendAsync("Error", "Chỉ chủ phòng mới có quyền bắt đầu trận đấu.");
                return;
            }

            // Apply host spectator mode
            host.IsSpectator = isHostSpectator;

            // Setup duration limit
            room.GameDurationMinutes = durationMinutes > 0 ? durationMinutes : 30;
            room.GameStartTime = System.DateTime.UtcNow;

            // Load questions from Supabase into room cache with a robust fallback
            try
            {
                room.CachedQuestions = await _questionRepo.GetAllAsync();
                if (room.CachedQuestions == null || room.CachedQuestions.Count == 0)
                {
                    room.CachedQuestions = GetFallbackQuestions();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "❌ [StartGame] Failed to load questions from database. Loading fallback questions.");
                room.CachedQuestions = GetFallbackQuestions();
            }
            room.IsStarted = true;

            // Set active index to first non-spectator player
            var firstRacer = room.Players.FirstOrDefault(p => !p.IsSpectator);
            if (firstRacer != null)
            {
                room.ActivePlayerIndex = room.Players.IndexOf(firstRacer);
            }
            else
            {
                room.ActivePlayerIndex = 0;
            }

            await Clients.Group(roomCode).SendAsync("GameStarted", room.Players, room.ActivePlayerIndex, room.GameDurationMinutes);
            _logger.LogInformation("🚀 [StartGame] Room {RoomCode} has started! Players count: {Count} | Duration: {Duration} min", roomCode, room.Players.Count, room.GameDurationMinutes);
        }

        // ════════════════════════════════════════
        // GAMEPLAY ACTIONS
        // ════════════════════════════════════════

        public async Task RollDice(string roomCode)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.IsSpectator) return;

            player.IsExtraTurn = false;

            // Calculate roll
            var (rollVal1, rollVal2, totalMove) = _gameService.CalculateDiceRoll(player);
            await Clients.Group(roomCode).SendAsync("DiceRolled", player.Name, rollVal1, rollVal2, totalMove);

            // Move player
            _gameService.MovePlayer(player, totalMove);
            await Clients.Group(roomCode).SendAsync("PlayerMoved", player.Id, player.TileIndex, player.LapCount);
            _logger.LogInformation("🎲 [RollDice] Room {RoomCode} | Player {PlayerName} rolled ({Val1}, {Val2}) -> Moved {Total} steps to Tile {Tile}", roomCode, player.Name, rollVal1, rollVal2, totalMove, player.TileIndex);

            // Process tile landing
            await ProcessTileLanding(roomCode, room, player);
        }

        public async Task ProcessNewTileLanding(string roomCode)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.IsSpectator) return;

            await ProcessTileLanding(roomCode, room, player);
        }

        public async Task SubmitAnswer(string roomCode, int answerIndex)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.CurrentQuestion == null) return;

            var (isCorrect, penaltyText) = _gameService.ProcessAnswer(player, player.CurrentQuestion, answerIndex);

            // Find correct index for UI highlight
            int correctIndex = player.CurrentQuestion.Options
                .OrderBy(o => o.OptionLetter)
                .ToList()
                .FindIndex(o => o.IsCorrect);

            // Send outcome ONLY to caller
            await Clients.Caller.SendAsync("AnswerOutcome", player.Name, isCorrect, correctIndex, player.WrongStreak, penaltyText);
            
            // Broadcast movement in case of penalty
            await Clients.Group(roomCode).SendAsync("PlayerMoved", player.Id, player.TileIndex, player.LapCount);

            // Log status update
            string logMsg = isCorrect ? $"[Hệ thống] {player.Name} trả lời ĐÚNG câu hỏi!" : $"[Hệ thống] {player.Name} trả lời SAI câu hỏi! {penaltyText}";
            await Clients.Group(roomCode).SendAsync("StatusUpdate", logMsg, isCorrect ? "log-question" : "log-trap");
            _logger.LogInformation("📝 [SubmitAnswer] Room {RoomCode} | Player {PlayerName} -> Option: {AnswerIndex} | IsCorrect: {IsCorrect} | Tile: {Tile}", roomCode, player.Name, answerIndex, isCorrect, player.TileIndex);

            player.CurrentQuestion = null;
        }

        public async Task SpinWheel(string roomCode)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || player.IsSpectator) return;

            var (sectorIdx, label, desc, isReward) = _gameService.SpinWheel(player);

            // Send WheelSpun ONLY to caller
            await Clients.Caller.SendAsync("WheelSpun",
                player.Name, sectorIdx, label, desc, isReward,
                player.TileIndex, player.SkipTurn, player.Shield,
                player.IsExtraTurn);

            // Broadcast movement and status log to group
            await Clients.Group(roomCode).SendAsync("PlayerMoved", player.Id, player.TileIndex, player.LapCount);
            
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

        private async Task ProcessTileLanding(string roomCode, GameRoom room, Player player)
        {
            await Task.Delay(1000); // Wait for hop animation on clients

            string tileType = _gameService.GetTileType(player.TileIndex);

            switch (tileType)
            {
                case "start":
                    await Clients.Group(roomCode).SendAsync("StatusUpdate", $"[Server] {player.Name} dừng tại ô Xuất Phát.", "log-win");
                    break;

                case "question":
                    var question = _gameService.GetRandomQuestion(room);
                    if (question == null) return;
                    
                    player.CurrentQuestion = question;
                    var answers = question.Options.OrderBy(o => o.OptionLetter).Select(o => o.OptionText).ToList();
                    await Clients.Client(player.ConnectionId).SendAsync("TriggerQuestion", player.Name, question.QuestionText, answers, player.WrongStreak);
                    break;

                case "trap":
                    if (player.Shield)
                    {
                        player.Shield = false;
                        await Clients.Client(player.ConnectionId).SendAsync("TriggerShieldBlock", player.Name);
                    }
                    else
                    {
                        var (trapName, trapDetail) = _gameService.ApplyTrap(player, room.Players);
                        await Clients.Client(player.ConnectionId).SendAsync("TriggerTrap", player.Name, trapName, trapDetail, player.TileIndex, player.SkipTurn);
                    }
                    break;

                case "reward":
                    var (rewardName, rewardDetail, isExtraTurn) = _gameService.ApplyReward(player);
                    await Clients.Client(player.ConnectionId).SendAsync("TriggerReward", player.Name, rewardName, rewardDetail, player.TileIndex, player.Shield, player.DoubleDice, isExtraTurn);
                    break;

                case "wheel":
                    await Clients.Client(player.ConnectionId).SendAsync("TriggerWheel", player.Name);
                    break;
            }
        }



        public async Task EndGameDueToTimeout(string roomCode)
        {
            var room = _roomRepo.GetRoom(roomCode);
            if (room == null || !room.IsStarted) return;
            
            var activeRacers = room.Players.Where(p => !p.IsSpectator).ToList();
            if (activeRacers.Count > 0)
            {
                var winner = activeRacers
                    .OrderByDescending(p => p.LapCount)
                    .ThenByDescending(p => p.TileIndex)
                    .First();
                await Clients.Group(room.RoomCode).SendAsync("StatusUpdate", $"[Server] Hết giờ chơi! Trận đấu kết thúc.", "log-trap");
                await Clients.Group(room.RoomCode).SendAsync("GameFinished", winner);
            }
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

            var player = room.Players.FirstOrDefault(p => p.SessionToken == sessionToken);
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

            var roomState = new
            {
                RoomCode = room.RoomCode,
                IsStarted = room.IsStarted,
                GameDurationMinutes = remainingMinutes,
                Players = room.Players.Select(p => new
                {
                    p.Id,
                    p.ConnectionId,
                    p.Name,
                    p.HorseId,
                    p.TileIndex,
                    p.WrongStreak,
                    p.Shield,
                    p.SkipTurn,
                    p.DoubleDice,
                    p.DiceModifier,
                    p.LapCount,
                    p.IsSpectator,
                    p.IsHost,
                    p.SessionToken
                }),
                ActivePlayerIndex = room.ActivePlayerIndex
            };

            await Clients.Caller.SendAsync("Rejoined", roomState);
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
                                var p = currentRoom.Players.FirstOrDefault(x => x.ConnectionId == connId);
                                if (p != null)
                                {
                                    _playerRepo.RemovePlayer(currentRoom, connId);
                                    await Clients.Group(rCode).SendAsync("PlayerDisconnected", pName, currentRoom.Players);
                                    _logger.LogWarning("❌ [Disconnect] Player {PlayerName} left room {RoomCode} (Lobby).", pName, rCode);

                                    if (currentRoom.Players.Count == 0)
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
                        player.ConnectionId = string.Empty;
                        await Clients.Group(room.RoomCode).SendAsync("PlayerDisconnected", player.Name, room.Players);
                        _logger.LogWarning("🔌 [Disconnect] Player {PlayerName} disconnected from active room {RoomCode}.", player.Name, room.RoomCode);

                        // If all connections are lost, clean up the room after a 10 seconds grace period
                        string rCode = room.RoomCode;
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(10000);
                            var currentRoom = _roomRepo.GetRoom(rCode);
                            if (currentRoom != null && currentRoom.IsStarted)
                            {
                                if (currentRoom.Players.All(p => string.IsNullOrEmpty(p.ConnectionId)))
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

        private List<McqQuestion> GetFallbackQuestions()
        {
            var fallback = new List<McqQuestion>
            {
                new McqQuestion {
                    Id = 1,
                    QuestionNumber = 1,
                    QuestionText = "Thủ đô của Việt Nam là thành phố nào?",
                    CorrectAnswer = "C",
                    Options = new List<McqOption> {
                        new McqOption { OptionLetter = "A", OptionText = "TP. Hồ Chí Minh", IsCorrect = false },
                        new McqOption { OptionLetter = "B", OptionText = "Đà Nẵng", IsCorrect = false },
                        new McqOption { OptionLetter = "C", OptionText = "Hà Nội", IsCorrect = true },
                        new McqOption { OptionLetter = "D", OptionText = "Hải Phòng", IsCorrect = false }
                    }
                },
                new McqQuestion {
                    Id = 2,
                    QuestionNumber = 2,
                    QuestionText = "Trái Đất tự quay quanh trục mất bao lâu?",
                    CorrectAnswer = "B",
                    Options = new List<McqOption> {
                        new McqOption { OptionLetter = "A", OptionText = "12 giờ", IsCorrect = false },
                        new McqOption { OptionLetter = "B", OptionText = "24 giờ", IsCorrect = true },
                        new McqOption { OptionLetter = "C", OptionText = "365 ngày", IsCorrect = false },
                        new McqOption { OptionLetter = "D", OptionText = "30 ngày", IsCorrect = false }
                    }
                },
                new McqQuestion {
                    Id = 3,
                    QuestionNumber = 3,
                    QuestionText = "Số nguyên tố nhỏ nhất là số nào?",
                    CorrectAnswer = "C",
                    Options = new List<McqOption> {
                        new McqOption { OptionLetter = "A", OptionText = "0", IsCorrect = false },
                        new McqOption { OptionLetter = "B", OptionText = "1", IsCorrect = false },
                        new McqOption { OptionLetter = "C", OptionText = "2", IsCorrect = true },
                        new McqOption { OptionLetter = "D", OptionText = "3", IsCorrect = false }
                    }
                },
                new McqQuestion {
                    Id = 4,
                    QuestionNumber = 4,
                    QuestionText = "Kim loại nào dẫn điện tốt nhất ở điều kiện thường?",
                    CorrectAnswer = "C",
                    Options = new List<McqOption> {
                        new McqOption { OptionLetter = "A", OptionText = "Vàng", IsCorrect = false },
                        new McqOption { OptionLetter = "B", OptionText = "Đồng", IsCorrect = false },
                        new McqOption { OptionLetter = "C", OptionText = "Bạc", IsCorrect = true },
                        new McqOption { OptionLetter = "D", OptionText = "Nhôm", IsCorrect = false }
                    }
                },
                new McqQuestion {
                    Id = 5,
                    QuestionNumber = 5,
                    QuestionText = "Đất nước mặt trời mọc là quốc gia nào?",
                    CorrectAnswer = "D",
                    Options = new List<McqOption> {
                        new McqOption { OptionLetter = "A", OptionText = "Hàn Quốc", IsCorrect = false },
                        new McqOption { OptionLetter = "B", OptionText = "Trung Quốc", IsCorrect = false },
                        new McqOption { OptionLetter = "C", OptionText = "Việt Nam", IsCorrect = false },
                        new McqOption { OptionLetter = "D", OptionText = "Nhật Bản", IsCorrect = true }
                    }
                }
            };
            return fallback;
        }
    }
}
