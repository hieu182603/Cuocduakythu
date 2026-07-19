namespace Backend.Models
{
    public class Player
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string HorseId { get; set; } = string.Empty;
        public int TileIndex { get; set; } = 0;
        public int WrongStreak { get; set; } = 0;
        public bool Shield { get; set; } = false;
        public int FreezeTimeMs { get; set; } = 0;
        public bool DoubleDice { get; set; } = false;
        public int DiceModifier { get; set; } = 0;
        public bool IsAutoRoll { get; set; } = false;
        public int LapCount { get; set; } = 0;
        public bool IsSpectator { get; set; } = false;
        public bool IsHost { get; set; } = false;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsRolling { get; set; } = false;
        [System.Text.Json.Serialization.JsonIgnore]
        public bool CanProcessNewTileLanding { get; set; } = false;
        public DateTime? FrozenUntilUtc { get; set; }
        public string? CurrentQuestionId { get; set; }
        public string? PendingTileEventType { get; set; }
        public int PendingTileIndex { get; set; }
        public PlayerPhase Phase { get; set; } = PlayerPhase.Ready;
        public DateTime NextRollAllowedUtc { get; set; } = DateTime.MinValue;

        public void ResetForRematch()
        {
            TileIndex = 0;
            WrongStreak = 0;
            Shield = false;
            FreezeTimeMs = 0;
            DoubleDice = false;
            DiceModifier = 0;
            IsAutoRoll = false;
            LapCount = 0;
            IsSpectator = false;
            IsRolling = false;
            CanProcessNewTileLanding = false;
            FrozenUntilUtc = null;
            CurrentQuestionId = null;
            PendingTileEventType = null;
            PendingTileIndex = 0;
            Phase = PlayerPhase.Ready;
            NextRollAllowedUtc = DateTime.MinValue;
        }

        public int GetRemainingFreezeTimeMs()
        {
            if (!FrozenUntilUtc.HasValue) return 0;
            var remaining = (int)Math.Ceiling((FrozenUntilUtc.Value - DateTime.UtcNow).TotalMilliseconds);
            if (remaining > 0) return remaining;
            FrozenUntilUtc = null;
            FreezeTimeMs = 0;
            return 0;
        }
    }
}
