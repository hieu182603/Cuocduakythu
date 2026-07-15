namespace CuocDuaKyThu.Utilities
{
    /// <summary>Game-wide constants.</summary>
    public static class Constants
    {
        // Board
        public const int TotalTiles = 32;
        public const int BoardRows = 7;
        public const int BoardCols = 11;

        // Players
        public const int MinPlayers = 2;
        public const int MaxPlayersOffline = 6;
        public const int MaxPlayersOnline = 50;
        public const int TotalCharacters = 10;

        // Dice
        public const int DiceMin = 1;
        public const int DiceMax = 6;
        public const int ReducedDiceMax = 3;

        // Timing (seconds)
        public const float SplashDuration = 2.2f;
        public const float HopDurationNormal = 0.4f;
        public const float HopDurationFast = 0.2f;
        public const float DiceShakeDuration = 1.2f;
        public const float WheelSpinDuration = 4.0f;
        public const float SkipTurnDelay = 2.0f;
        public const float AnswerShowDelay = 1.5f;
        public const float WrongAnswerShowDelay = 2.5f;
        public const float VictoryDelay = 1.5f;

        // Penalties
        public const int WrongStreakBonusSeconds = 5;
        public const int BaseWaitPenaltySeconds = 10;
        public const float RareStartTrapChance = 0.15f;

        // Network
        public const string DefaultServerUrl = "http://localhost:5089";
        public const string HubEndpoint = "/gameHub";
        public const string ApiQuestionsEndpoint = "/api/questions";
        public const string ApiRoomsEndpoint = "/api/rooms";
        public const int RoomCodeLength = 5;
    }
}
