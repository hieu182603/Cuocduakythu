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
            ("Đầm Lầy Choáng Váng",    "Nhân vật bị mắc kẹt bùn sâu, mất lượt ở vòng sau."),
            ("Bảo Táp Gió Ngược",      "Gió bão cản trở! Xúc xắc lăn tối đa chỉ được 3 trong 2 lượt tới."),
            ("Cổng Dịch Chuyển Lỗi",   "Cổng không gian lỗi! Tráo đổi vị trí của bạn với một người chơi ngẫu nhiên."),
            ("Hố Đen Vũ Trụ (Hiếm)",   "Trôi dạt không gian! Bị hút trực tiếp quay về vạch Start.")
        };

        // ── Reward Definitions ──
        private static readonly (string Name, string Detail)[] Rewards =
        {
            ("Cà Rốt Siêu Cấp",     "Tăng tốc lực! Tiến lên thêm 3 ô."),
            ("Khiên Thần Bảo Hộ",    "Nhận một lớp lá chắn miễn nhiễm hoàn toàn hình phạt từ Bẫy tiếp theo."),
            ("Động Cơ Phản Lực",     "Quá phấn khích! Đi thêm một lượt xúc xắc ngay lập tức."),
            ("Nhân Đôi Động Cơ",     "Double Dice! Lượt tiếp theo xúc xắc của bạn sẽ được nhân đôi khoảng cách."),
            ("Quà Troll Bí Mật",     "Mở quà... Ồ không! Bạn bị troll, lùi lại 2 ô."),
            ("Hộp Quà Trống",        "Mở hộp quà... Không có gì cả! Chúc bạn may mắn lần sau.")
        };

        // ── Wheel Definitions ──
        private static readonly (string Label, string Desc, bool IsReward)[] WheelSectors =
        {
            ("Lùi 3 ô",       "Lùi ngay 3 ô trên bảng.",                          false),
            ("Tiến 3 ô",      "Tiến lên 3 ô trên bảng.",                           true),
            ("Mất lượt",      "Mất lượt chơi tiếp theo.",                           false),
            ("Thêm lượt",     "Được xoay xúc xắc thêm lượt nữa!",                 true),
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

        public void MovePlayer(Player player, int totalMove)
        {
            int oldIndex = player.TileIndex;
            player.TileIndex = (player.TileIndex + totalMove) % TotalTiles;

            // Detect lap completion (passed or landed on Start after leaving it)
            if (player.TileIndex < oldIndex || (oldIndex > 0 && player.TileIndex == 0))
            {
                player.LapCount++;
            }
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
            if (room.CachedQuestions.Count == 0) return null;
            int idx = _random.Next(room.CachedQuestions.Count);
            return room.CachedQuestions[idx];
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
                int rType = _random.Next(3);

                if (rType == 0)
                {
                    int backSteps = _random.Next(1, 4); // 1–3
                    player.TileIndex = Math.Max(0, player.TileIndex - backSteps);
                    penaltyText = $"lùi lại {backSteps} ô";
                }
                else if (rType == 1)
                {
                    player.SkipTurn = true;
                    penaltyText = "bị choáng mất lượt vòng kế";
                }
                else
                {
                    int seconds = 10 + (player.WrongStreak * 5);
                    penaltyText = $"phạt thêm {seconds} giây chờ";
                }
            }

            return (isCorrect, penaltyText);
        }

        // ════════════════════════════════════════
        // TRAPS
        // ════════════════════════════════════════

        public (string name, string detail) ApplyTrap(Player player, List<Player> allPlayers)
        {
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
                case 2: // Mất lượt
                    player.SkipTurn = true;
                    break;
                case 3: // Dice max 3 for 2 turns
                    player.DiceModifier = 2;
                    break;
                case 4: // Swap positions
                    var others = allPlayers.Where(p => p.Id != player.Id).ToList();
                    if (others.Count > 0)
                    {
                        var target = others[_random.Next(others.Count)];
                        (player.TileIndex, target.TileIndex) = (target.TileIndex, player.TileIndex);
                    }
                    break;
                case 5: // Return to Start
                    player.TileIndex = 0;
                    break;
            }

            return (name, detail);
        }

        // ════════════════════════════════════════
        // REWARDS
        // ════════════════════════════════════════

        public (string name, string detail, bool isExtraTurn) ApplyReward(Player player)
        {
            int rewardIdx = _random.Next(Rewards.Length);
            var (name, detail) = Rewards[rewardIdx];
            bool isExtraTurn = false;

            switch (rewardIdx)
            {
                case 0: // Tiến 3
                    player.TileIndex = Math.Min(TotalTiles - 1, player.TileIndex + 3);
                    break;
                case 1: // Shield
                    player.Shield = true;
                    break;
                case 2: // Extra turn
                    player.IsExtraTurn = true;
                    isExtraTurn = true;
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

            return (name, detail, isExtraTurn);
        }

        // ════════════════════════════════════════
        // SPIN WHEEL
        // ════════════════════════════════════════

        public (int sectorIndex, string label, string desc, bool isReward) SpinWheel(Player player)
        {
            int idx = _random.Next(WheelSectors.Length);
            var (label, desc, isReward) = WheelSectors[idx];

            switch (idx)
            {
                case 0:  player.TileIndex = Math.Max(0, player.TileIndex - 3); break;                  // Lùi 3
                case 1:  player.TileIndex = Math.Min(TotalTiles - 1, player.TileIndex + 3); break;      // Tiến 3
                case 2:  player.SkipTurn = true; break;                                                 // Mất lượt
                case 3:  player.IsExtraTurn = true; break;                                              // Thêm lượt
                case 4:  player.TileIndex = Math.Max(0, player.TileIndex - 2); break;                   // Lùi 2
                case 5:  player.Shield = true; break;                                                   // Nhận Khiên
                case 6:  player.DoubleDice = true; break;                                               // x2 xúc xắc
                case 7:  player.TileIndex = 0; break;                                                   // Quay về Start
                case 8:  player.TileIndex = Math.Min(TotalTiles - 1, player.TileIndex + 2); break;      // Tiến 2
                case 9:  player.Shield = false; break;                                                  // Mất Khiên
                case 10: player.TileIndex = Math.Min(TotalTiles - 1, player.TileIndex + 5); break;      // Tiến 5
                case 11: player.TileIndex = Math.Max(0, player.TileIndex - 5); break;                   // Lùi 5
            }

            return (idx, label, desc, isReward);
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
