namespace Backend.Models
{
    /// <summary>
    /// In-memory representation of an active game room.
    /// Holds all runtime state for a single match.
    /// </summary>
    public class GameRoom
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public object SyncRoot { get; } = new();

        public string RoomCode { get; set; } = string.Empty;
        public int NextPlayerId { get; set; } = 0;
        public List<Player> Players { get; set; } = new();
        public int ActivePlayerIndex { get; set; } = 0;
        public bool IsStarted { get; set; } = false;
        public bool IsStarting { get; set; } = false;
        public bool IsFinished { get; set; } = false;
        public int GameDurationMinutes { get; set; } = 10;
        public System.DateTime? GameStartTime { get; set; }

        /// <summary>Cached questions loaded from Supabase at game start.</summary>
        public List<McqQuestion> CachedQuestions { get; set; } = new();

        [System.Text.Json.Serialization.JsonIgnore]
        public Task? QuestionLoadTask { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public TaskCompletionSource<bool> FirstQuestionBatchReady { get; set; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        [System.Text.Json.Serialization.JsonIgnore]
        public CancellationTokenSource QuestionLoadCancellation { get; set; } = new();
        public bool QuestionsLoadCompleted { get; set; }
        public string? QuestionLoadError { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public System.Threading.Timer? GameTimer { get; set; }

        public List<Player> GetPlayersSnapshot()
        {
            lock (SyncRoot)
            {
                return Players.ToList();
            }
        }
    }
}
