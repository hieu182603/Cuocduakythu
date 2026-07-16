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
        public McqQuestion? CurrentQuestion { get; set; }
        public string? PendingTileEventType { get; set; }
        public int PendingTileIndex { get; set; }

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
