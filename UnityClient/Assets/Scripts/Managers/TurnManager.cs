using UnityEngine;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Controls turn rotation, skip-turn logic, and extra-turn handling.
    /// Replaces setupTurn() and nextTurn() from app.js.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        private PlayerManager _playerManager;
        private UIManager _uiManager;
        private DiceManager _diceManager;

        public int ActivePlayerIndex { get; private set; } = 0;
        public PlayerState ActivePlayer => _playerManager.GetPlayer(ActivePlayerIndex);

        private void Awake()
        {
            _playerManager = GameManager.Instance.playerManager;
            _uiManager = GameManager.Instance.uiManager;
            _diceManager = GameManager.Instance.diceManager;
        }

        public void StartFirstTurn()
        {
            ActivePlayerIndex = 0;
            SetupTurn();
        }

        public void NextTurn()
        {
            ActivePlayerIndex = (ActivePlayerIndex + 1) % _playerManager.PlayerCount;
            SetupTurn();
        }

        public void Reset()
        {
            ActivePlayerIndex = 0;
        }

        /// <summary>
        /// Configures the current turn: updates UI, checks skip-turn, enables/disables dice.
        /// </summary>
        private void SetupTurn()
        {
            var player = ActivePlayer;
            if (player == null) return;

            // Update scoreboard highlighting
            _uiManager.UpdateScoreboard(_playerManager.GetLeaderboard(), ActivePlayerIndex);

            // Highlight active tile
            GameManager.Instance.boardManager.HighlightTile(player.tileIndex);

            // Update turn info panel
            _uiManager.UpdateTurnInfo(player);

            // Handle skip turn
            if (player.skipTurn)
            {
                player.skipTurn = false;
                _uiManager.LogMessage($"Tay đua [{player.name}] bị mất lượt trong vòng này.", UIManager.LogType.Trap);
                _uiManager.SetDiceInteractable(false);
                _uiManager.SetTurnStatusText("Mất lượt! Lượt chuyển sang người kế tiếp.");

                if (!GameManager.Instance.isOnlineMode)
                {
                    Invoke(nameof(NextTurn), Constants.SkipTurnDelay);
                }
                return;
            }

            // Normal turn
            bool isMyTurn = !GameManager.Instance.isOnlineMode; // Offline: always your turn
            // TODO: Online mode — check if this player's connectionId matches local player
            // if (GameManager.Instance.isOnlineMode) isMyTurn = (player.connectionId == SignalRManager.LocalConnectionId);

            _uiManager.SetDiceInteractable(isMyTurn);
            _uiManager.SetTurnStatusText(isMyTurn
                ? $"Đến lượt bạn! Hãy tung xúc xắc để di chuyển [{player.name}]!"
                : $"Đang đợi lượt di chuyển của [{player.name}]...");

            _diceManager.ResetDiceVisual();
        }

        /// <summary>Force set the active player index (used by SignalR sync).</summary>
        public void SetActiveIndex(int index)
        {
            ActivePlayerIndex = index;
            SetupTurn();
        }
    }
}
