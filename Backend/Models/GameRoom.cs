namespace Backend.Models
{
    /// <summary>
    /// In-memory representation of an active game room.
    /// Holds all runtime state for a single match.
    /// </summary>
    public class GameRoom
    {
        public string RoomCode { get; set; } = string.Empty;
        public int NextPlayerId { get; set; } = 0;
        public List<Player> Players { get; set; } = new();
        public int ActivePlayerIndex { get; set; } = 0;
        public bool IsStarted { get; set; } = false;
        public int GameDurationMinutes { get; set; } = 30;
        public System.DateTime? GameStartTime { get; set; }

        /// <summary>Cached questions loaded from Supabase at game start.</summary>
        public List<McqQuestion> CachedQuestions { get; set; } = new();

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Threading.Timer? GameTimer { get; set; }
    }
}
