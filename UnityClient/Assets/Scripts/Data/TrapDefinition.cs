namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// Static definitions for the 6 trap types.
    /// Matches GDD: Lùi 2, Lùi 5, Mất lượt, Giảm xúc xắc, Đổi vị trí, Quay về Start.
    /// </summary>
    public static class TrapDefinition
    {
        public static readonly TrapInfo[] Traps =
        {
            new TrapInfo("lùi",           "Hố Bùn Lầy",            "Nhựa bị trượt chân! Lùi ngay 2 ô.",                                              2),
            new TrapInfo("lùi-lớn",       "Động Đất Trượt Dốc",     "Sự cố địa chấn hung bạo! Lùi ngay 5 ô.",                                         5),
            new TrapInfo("mất-lượt",      "Đầm Lầy Choáng Váng",    "Nhân vật bị mắc kẹt bùn sâu, mất lượt ở vòng sau.",                               1),
            new TrapInfo("giảm-xúc-xắc",  "Bảo Táp Gió Ngược",      "Gió bão cản trở! Xúc xắc lăn tối đa chỉ được 3 trong 2 lượt tới.",               2),
            new TrapInfo("đổi-vị-trí",    "Cổng Dịch Chuyển Lỗi",   "Cổng không gian lỗi! Tráo đổi vị trí của bạn với một người chơi ngẫu nhiên.",    0),
            new TrapInfo("quay-start",    "Hố Đen Vũ Trụ (Hiếm)",   "Trôi dạt không gian! Bị hút trực tiếp quay về vạch Start.",                       99)
        };
    }

    [System.Serializable]
    public class TrapInfo
    {
        public string type;
        public string name;
        public string detail;
        public int value;

        public TrapInfo(string type, string name, string detail, int value)
        {
            this.type = type;
            this.name = name;
            this.detail = detail;
            this.value = value;
        }
    }
}
