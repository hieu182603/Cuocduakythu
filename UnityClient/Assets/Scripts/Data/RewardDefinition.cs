namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// Static definitions for the 6 reward types.
    /// Matches GDD: Tiến 3, Lá chắn, Thêm lượt, Double Dice, Troll, Hộp trống.
    /// </summary>
    public static class RewardDefinition
    {
        public static readonly RewardInfo[] Rewards =
        {
            new RewardInfo("tiến",         "Cà Rốt Siêu Cấp",     "Tăng tốc lực! Tiến lên thêm 3 ô.",                                                 3),
            new RewardInfo("lá-chắn",      "Khiên Thần Bảo Hộ",    "Nhận một lớp lá chắn miễn nhiễm hoàn toàn hình phạt từ Bẫy tiếp theo.",             1),
            new RewardInfo("thêm-lượt",    "Động Cơ Phản Lực",     "Quá phấn khích! Đi thêm một lượt xúc xắc ngay lập tức.",                            1),
            new RewardInfo("double-dice",  "Nhân Đôi Động Cơ",     "Double Dice! Lượt tiếp theo xúc xắc của bạn sẽ được nhân đôi khoảng cách.",         1),
            new RewardInfo("troll",        "Quà Troll Bí Mật",     "Mở quà... Ồ không! Bạn bị troll, lùi lại 2 ô.",                                    -2),
            new RewardInfo("troll-nothing","Hộp Quà Trống",        "Mở hộp quà... Không có gì cả! Chúc bạn may mắn lần sau.",                           0)
        };
    }

    [System.Serializable]
    public class RewardInfo
    {
        public string type;
        public string name;
        public string detail;
        public int value;

        public RewardInfo(string type, string name, string detail, int value)
        {
            this.type = type;
            this.name = name;
            this.detail = detail;
            this.value = value;
        }
    }
}
