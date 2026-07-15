using UnityEngine;

namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// Defines a single sector on the spin wheel.
    /// 12 sectors total matching the GDD spec.
    /// </summary>
    [System.Serializable]
    public class WheelSector
    {
        public string label;
        public string description;
        public bool isReward;
        public Color sectorColor;

        public WheelSector(string label, string desc, bool isReward, Color color)
        {
            this.label = label;
            this.description = desc;
            this.isReward = isReward;
            this.sectorColor = color;
        }
    }

    /// <summary>Static definition of all 12 wheel sectors.</summary>
    public static class WheelDefinition
    {
        private static readonly Color Red    = new Color(0.937f, 0.267f, 0.267f);
        private static readonly Color Green  = new Color(0.063f, 0.725f, 0.506f);
        private static readonly Color Blue   = new Color(0.231f, 0.510f, 0.988f);
        private static readonly Color Yellow = new Color(0.961f, 0.620f, 0.043f);

        public static readonly WheelSector[] Sectors =
        {
            new WheelSector("Lùi 3 ô",       "Lùi ngay 3 ô trên bảng.",                          false, Red),
            new WheelSector("Tiến 3 ô",      "Tiến lên 3 ô trên bảng.",                           true,  Green),
            new WheelSector("Mất lượt",      "Mất lượt chơi tiếp theo.",                           false, Blue),
            new WheelSector("Thêm lượt",     "Được xoay xúc xắc thêm lượt nữa!",                 true,  Yellow),
            new WheelSector("Lùi 2 ô",       "Lùi lại 2 ô.",                                      false, Red),
            new WheelSector("Nhận Khiên",    "Nhận 1 lá chắn bảo hộ.",                             true,  Green),
            new WheelSector("Xúc xắc x2",   "Lượt tới khoảng cách xúc xắc nhân đôi.",            true,  Blue),
            new WheelSector("Quay về Start", "Ôi xui xẻo! Quay về vạch xuất phát.",               false, Yellow),
            new WheelSector("Tiến 2 ô",      "Tiến lên 2 ô.",                                      true,  Red),
            new WheelSector("Mất Khiên",    "Bị tịch thu lá chắn đang có (nếu có).",              false, Green),
            new WheelSector("Tiến 5 ô",      "Bứt tốc cực mạnh! Tiến lên 5 ô.",                   true,  Blue),
            new WheelSector("Lùi 5 ô",       "Trượt dài ngã ngựa! Lùi lại 5 ô.",                  false, Yellow)
        };
    }
}
