using Backend.Models;

namespace Backend.Services
{
    public class GameService : IGameService
    {
        private readonly Random _random = new();

        // ── Board Layout (41 tiles) ──
        private static readonly string[] TileTypes =
        {
            "start",      // 0
            "trap",       // 1
            "question",   // 2
            "trap",       // 3
            "question",   // 4
            "trap",       // 5 (bomb)
            "question",   // 6
            "question",   // 7
            "reward",     // 8
            "question",   // 9
            "trap",       // 10
            "question",   // 11
            "question",   // 12
            "question",   // 13
            "wheel",      // 14
            "question",   // 15
            "trap",       // 16 (bomb)
            "question",   // 17
            "trap",       // 18
            "question",   // 19
            "question",   // 20
            "question",   // 21
            "reward",     // 22
            "question",   // 23
            "question",   // 24
            "trap",       // 25
            "question",   // 26
            "trap",       // 27
            "question",   // 28
            "trap",       // 29 (bomb)
            "wheel",      // 30
            "question",   // 31
            "question",   // 32
            "question",   // 33
            "trap",       // 34
            "question",   // 35
            "reward",     // 36
            "trap",       // 37 (bomb)
            "question",   // 38
            "trap",       // 39
            "question"    // 40
        };

        private const int TotalTiles = 41;

        // ── Trap Definitions ──
        private static readonly (string Name, string Detail)[] Traps =
        {
            ("Hố Bùn Lầy",            "Nhựa bị trượt chân! Lùi ngay 2 ô."),
            ("Động Đất Trượt Dốc",     "Sự cố địa chấn hung bạo! Lùi ngay 5 ô."),
            ("Đầm Lầy Choáng Váng",    "Nhân vật bị mắc kẹt bùn sâu, bị đóng băng cấm tung xúc xắc trong 5 giây."),
            ("Bảo Táp Gió Ngược",      "Gió bão cản trở! Xúc xắc lăn tối đa chỉ được 3 trong 2 lượt tới."),
            ("Cổng Dịch Chuyển Lỗi",   "Cổng không gian bị lỗi! Bị tịch thu lá chắn đang có (nếu có) và lùi lại 3 ô."),
            ("Hố Đen Vũ Trụ (Hiếm)",   "Trôi dạt không gian! Bị hút trực tiếp quay về vạch Start.")
        };

        // ── Reward Definitions ──
        private static readonly (string Name, string Detail)[] Rewards =
        {
            ("Cà Rốt Siêu Cấp",     "Tăng tốc lực! Tiến lên thêm 3 ô."),
            ("Khiên Thần Bảo Hộ",    "Nhận một lớp lá chắn miễn nhiễm hoàn toàn hình phạt từ Bẫy tiếp theo."),
            ("Động Cơ Phản Lực",     "Quá phấn khích! Tự động tung thêm 1 lần xúc xắc nữa ngay lập tức."),
            ("Nhân Đôi Động Cơ",     "Double Dice! Lượt tiếp theo xúc xắc của bạn sẽ được nhân đôi khoảng cách."),
            ("Quà Troll Bí Mật",     "Mở quà... Ồ không! Bạn bị troll, lùi lại 2 ô."),
            ("Hộp Quà Trống",        "Mở hộp quà... Không có gì cả! Chúc bạn may mắn lần sau.")
        };

        // ── Wheel Definitions ──
        private static readonly (string Label, string Desc, bool IsReward)[] WheelSectors =
        {
            ("Lùi 3 ô",       "Lùi ngay 3 ô trên bảng.",                          false),
            ("Tiến 3 ô",      "Tiến lên 3 ô trên bảng.",                           true),
            ("Đóng Băng",     "Bị đóng băng không thể tung xúc xắc trong 5s.",     false),
            ("Auto Roll",     "Tự động tung xúc xắc thêm lượt nữa!",              true),
            ("Lùi 2 ô",       "Lùi lại 2 ô.",                                      false),
            ("Nhận Khiên",    "Nhận 1 lá chắn bảo hộ.",                             true),
            ("Xúc xắc x2",   "Lượt tới khoảng cách xúc xắc nhân đôi.",            true),
            ("Quay về Start", "Ôi xui xẻo! Quay về vạch xuất phát.",               false),
            ("Tiến 2 ô",      "Tiến lên 2 ô.",                                      true),
            ("Mất Khiên",    "Bị tịch thu lá chắn đang có (nếu có).",              false),
            ("Tiến 5 ô",      "Bứt tốc cực mạnh! Tiến lên 5 ô.",                   true),
            ("Lùi 5 ô",       "Trượt dài ngã ngựa! Lùi lại 5 ô.",                  false)
        };

        // ════════════════════════════════════════
        // DICE
        // ════════════════════════════════════════

        public (int rollVal1, int rollVal2, int totalMove) CalculateDiceRoll(Player player)
        {
            int rollVal1 = _random.Next(1, 7); // 1–6
            int rollVal2 = 0; // Only 1 dice

            if (player.DiceModifier > 0)
            {
                rollVal1 = Math.Min(3, rollVal1);
                player.DiceModifier--;
            }

            int rollSum = rollVal1;
            int totalMove = rollSum;
            if (player.DoubleDice)
            {
                totalMove = rollSum * 2;
                player.DoubleDice = false;
            }

            return (rollVal1, rollVal2, totalMove);
        }

        // ════════════════════════════════════════
        // MOVEMENT
        // ════════════════════════════════════════

        private void MovePlayerForward(Player player, int steps)
        {
            if (steps <= 0) return;
            int oldIndex = player.TileIndex;
            player.TileIndex = (player.TileIndex + steps) % TotalTiles;

            // Detect lap completion (passed or landed on Start after leaving it)
            if (player.TileIndex < oldIndex || (oldIndex > 0 && player.TileIndex == 0))
            {
                player.LapCount++;
            }
        }

        public void MovePlayer(Player player, int totalMove)
        {
            MovePlayerForward(player, totalMove);
        }

        public string GetTileType(int tileIndex)
        {
            return TileTypes[tileIndex % TotalTiles];
        }

        public bool CheckVictory(Player player)
        {
            return false;
        }

        // ════════════════════════════════════════
        // QUESTIONS
        // ════════════════════════════════════════

        public McqQuestion? GetRandomQuestion(GameRoom room)
        {
            lock (room.SyncRoot)
            {
                if (room.CachedQuestions.Count == 0) return null;
                int idx = _random.Next(room.CachedQuestions.Count);
                return room.CachedQuestions[idx];
            }
        }

        public (bool isCorrect, string penaltyText) ProcessAnswer(Player player, McqQuestion question, int answerIndex)
        {
            // Map answerIndex (0–3) to letter ("A"–"D")
            string selectedLetter = ((char)('A' + answerIndex)).ToString();
            bool isCorrect = string.Equals(selectedLetter, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

            string penaltyText = "";

            if (isCorrect)
            {
                player.WrongStreak = 0;
            }
            else
            {
                player.WrongStreak++;
                int backSteps = player.WrongStreak;
                player.TileIndex = Math.Max(0, player.TileIndex - backSteps);
                penaltyText = $"lùi lại {backSteps} ô";
            }

            return (isCorrect, penaltyText);
        }

        // ════════════════════════════════════════
        // TRAPS
        // ════════════════════════════════════════

        public (string name, string detail, string movementDirection, int movementSteps) ApplyTrap(Player player)
        {
            int previousTileIndex = player.TileIndex;
            int trapIdx = _random.Next(Traps.Length);

            // Reduce rare "quay-start" (index 5) chance to ~15%
            if (trapIdx == 5 && _random.NextDouble() > 0.15)
            {
                trapIdx = 0; // fallback to "lùi 2"
            }

            var (name, detail) = Traps[trapIdx];

            switch (trapIdx)
            {
                case 0: // Lùi 2
                    player.TileIndex = Math.Max(0, player.TileIndex - 2);
                    break;
                case 1: // Lùi 5
                    player.TileIndex = Math.Max(0, player.TileIndex - 5);
                    break;
                case 2: // Mất lượt -> Freeze 5s
                    player.FreezeTimeMs = 5000;
                    player.FrozenUntilUtc = DateTime.UtcNow.AddMilliseconds(player.FreezeTimeMs);
                    break;
                case 3: // Dice max 3 for 2 turns
                    player.DiceModifier = 2;
                    break;
                case 4: // Cổng Dịch Chuyển Lỗi: Mất khiên & Lùi 3 ô
                    player.Shield = false;
                    player.TileIndex = Math.Max(0, player.TileIndex - 3);
                    break;
                case 5: // Return to Start
                    player.TileIndex = 0;
                    break;
            }

            string movementDirection = trapIdx == 5 && player.TileIndex != previousTileIndex
                ? "teleport"
                : player.TileIndex != previousTileIndex
                    ? "backward"
                    : "sync";
            int movementSteps = movementDirection == "backward"
                ? previousTileIndex - player.TileIndex
                : 0;

            return (name, detail, movementDirection, movementSteps);
        }

        // ════════════════════════════════════════
        // REWARDS
        // ════════════════════════════════════════

        public (string name, string detail, bool isAutoRoll, string movementDirection, int movementSteps) ApplyReward(Player player)
        {
            int previousTileIndex = player.TileIndex;
            int rewardIdx = _random.Next(Rewards.Length);
            var (name, detail) = Rewards[rewardIdx];
            bool isAutoRoll = false;

            switch (rewardIdx)
            {
                case 0: // Tiến 3
                    player.TileIndex = (player.TileIndex + 3) % TotalTiles;
                    if (player.TileIndex < previousTileIndex) player.LapCount++;
                    break;
                case 1: // Khiên
                    player.Shield = true;
                    break;
                case 2: // Thêm lượt
                    isAutoRoll = true;
                    player.IsAutoRoll = true;
                    break;
                case 3: // Double Dice
                    player.DoubleDice = true;
                    break;
                case 4: // Troll: lùi 2
                    player.TileIndex = Math.Max(0, player.TileIndex - 2);
                    break;
                case 5: // Empty box (no effect)
                    break;
            }

            string movementDirection = rewardIdx == 0
                ? "forward"
                : rewardIdx == 4 && player.TileIndex != previousTileIndex
                    ? "backward"
                    : "sync";
            int movementSteps = rewardIdx == 0
                ? 3
                : rewardIdx == 4
                    ? previousTileIndex - player.TileIndex
                    : 0;

            return (name, detail, isAutoRoll, movementDirection, movementSteps);
        }

        // ════════════════════════════════════════
        // SPIN WHEEL
        // ════════════════════════════════════════

        public (int sectorIndex, string label, string desc, bool isReward, string movementDirection, int movementSteps) SpinWheel(Player player)
        {
            int previousTileIndex = player.TileIndex;
            int idx = _random.Next(WheelSectors.Length);
            var (label, desc, isReward) = WheelSectors[idx];

            switch (idx)
            {
                case 0:  player.TileIndex = Math.Max(0, player.TileIndex - 3); break;                  // Lùi 3
                case 1:  MovePlayerForward(player, 3); break;                                           // Tiến 3
                case 2:
                    player.FreezeTimeMs = 5000;
                    player.FrozenUntilUtc = DateTime.UtcNow.AddMilliseconds(player.FreezeTimeMs);
                    break;                                                                                // Đóng băng
                case 3:  player.IsAutoRoll = true; break;                                               // Auto Roll
                case 4:  player.TileIndex = Math.Max(0, player.TileIndex - 2); break;                   // Lùi 2
                case 5:  player.Shield = true; break;                                                   // Nhận Khiên
                case 6:  player.DoubleDice = true; break;                                               // x2 xúc xắc
                case 7:  player.TileIndex = 0; break;                                                   // Quay về Start
                case 8:  MovePlayerForward(player, 2); break;                                           // Tiến 2
                case 9:  player.Shield = false; break;                                                  // Mất Khiên
                case 10: MovePlayerForward(player, 5); break;                                           // Tiến 5
                case 11: player.TileIndex = Math.Max(0, player.TileIndex - 5); break;                   // Lùi 5
            }

            string movementDirection = idx switch
            {
                1 or 8 or 10 => "forward",
                0 or 4 or 11 => "backward",
                7 when player.TileIndex != previousTileIndex => "teleport",
                _ => "sync"
            };
            int movementSteps = movementDirection switch
            {
                "forward" => (player.TileIndex - previousTileIndex + TotalTiles) % TotalTiles,
                "backward" => previousTileIndex - player.TileIndex,
                _ => 0
            };

            return (idx, label, desc, isReward, movementDirection, movementSteps);
        }

        // ════════════════════════════════════════
        // ROOM UTILS
        // ════════════════════════════════════════

        public string GenerateRoomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}
