using UnityEngine;

namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// ScriptableObject defining the board layout.
    /// Create one instance with all 32 tiles configured.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardLayout", menuName = "CuocDuaKyThu/TileData")]
    public class TileData : ScriptableObject
    {
        public TileEntry[] tiles = new TileEntry[32];
    }

    [System.Serializable]
    public class TileEntry
    {
        public int row;
        public int col;
        public TileType type;
        public string displayName;
        public bool isCorner;
    }

    public enum TileType
    {
        Start,
        Question,
        Trap,
        Reward,
        Wheel
    }
}
