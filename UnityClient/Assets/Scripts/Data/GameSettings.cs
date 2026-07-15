namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// Serializable game settings. Persisted via SaveManager (PlayerPrefs).
    /// </summary>
    [System.Serializable]
    public class GameSettings
    {
        public int musicVolume = 50;       // 0–100
        public int sfxVolume = 70;         // 0–100
        public SpeedSetting speed = SpeedSetting.Normal;
        public bool vfxEnabled = true;

        /// <summary>Last player names entered in lobby (for convenience).</summary>
        public string[] lastPlayerNames = new string[6];
    }

    public enum SpeedSetting
    {
        Normal,   // 400ms per hop
        Fast,     // 200ms per hop
        Instant   // 0ms per hop
    }
}
