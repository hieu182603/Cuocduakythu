namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// Runtime player state. Mutable during gameplay.
    /// One instance per player in the match.
    /// </summary>
    [System.Serializable]
    public class PlayerState
    {
        public int id;
        public string name;
        public string connectionId;     // SignalR connection ID (online mode)
        public CharacterData character;
        public string horseId;          // Character ID string for network sync

        // Board position
        public int tileIndex = 0;
        public bool lapCompleted = false;

        // Status effects
        public int wrongStreak = 0;
        public bool shield = false;
        public bool skipTurn = false;
        public bool doubleDice = false;
        public int diceModifier = 0;    // Number of turns with reduced dice
        public bool isExtraTurn = false;

        public PlayerState() { }

        public PlayerState(int id, string name, CharacterData character)
        {
            this.id = id;
            this.name = name;
            this.character = character;
            this.horseId = character != null ? character.characterId : "";
        }

        /// <summary>Reset all state for a new game while keeping identity.</summary>
        public void ResetForNewGame()
        {
            tileIndex = 0;
            lapCompleted = false;
            wrongStreak = 0;
            shield = false;
            skipTurn = false;
            doubleDice = false;
            diceModifier = 0;
            isExtraTurn = false;
        }
    }
}
