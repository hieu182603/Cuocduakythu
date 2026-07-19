using Backend.Models;

namespace Backend.Services
{
    /// <summary>
    /// Encapsulates all gameplay business logic.
    /// Extracted from GameHub to follow Single Responsibility Principle.
    /// </summary>
    public interface IGameService
    {
        // ── Dice ──
        (int rollVal1, int rollVal2, int totalMove) CalculateDiceRoll(Player player);

        // ── Movement ──
        void MovePlayer(Player player, int totalMove);

        // ── Tile Processing ──
        string GetTileType(int tileIndex);
        bool CheckVictory(Player player);

        // ── Questions ──
        McqQuestion? GetRandomQuestion();
        (bool isCorrect, string penaltyText) ProcessAnswer(Player player, McqQuestion question, int answerIndex);

        // ── Traps ──
        (string name, string detail, string movementDirection, int movementSteps) ApplyTrap(Player player);

        // ── Rewards ──
        (string name, string detail, bool isAutoRoll, string movementDirection, int movementSteps) ApplyReward(Player player);

        // ── Wheel ──
        (int sectorIndex, string label, string desc, bool isReward, string movementDirection, int movementSteps) SpinWheel(Player player);

        // ── Room Management ──
        string GenerateRoomCode();
    }
}
