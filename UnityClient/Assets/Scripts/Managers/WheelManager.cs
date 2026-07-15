using UnityEngine;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Controls spin wheel events: animation, sector calculation, effect application.
    /// Replaces triggerWheelEvent(), drawWheel(), spinWheel(), applyWheelResult().
    /// </summary>
    public class WheelManager : MonoBehaviour
    {
        [Header("References")]
        public Gameplay.SpinWheel spinWheelUI;  // The visual wheel component

        private System.Random _random = new();

        /// <summary>Trigger a wheel event for the given player (offline mode).</summary>
        public void TriggerWheelEvent(PlayerState player)
        {
            // Show wheel popup with spin button
            GameManager.Instance.uiManager.ShowWheelPopup(() =>
            {
                // On spin button pressed
                SpinForPlayer(player);
            });
        }

        private void SpinForPlayer(PlayerState player)
        {
            // Determine result sector
            int sectorIdx = _random.Next(WheelDefinition.Sectors.Length);
            var sector = WheelDefinition.Sectors[sectorIdx];

            // Animate the wheel spin, then apply result
            spinWheelUI.Spin(sectorIdx, Constants.WheelSpinDuration, () =>
            {
                ApplyWheelResult(player, sectorIdx);

                GameManager.Instance.boardManager.TeleportToken(player);

                GameManager.Instance.uiManager.LogMessage(
                    $"Vòng quay trúng vào: {sector.label} - {sector.description}");

                // Show result and close button
                GameManager.Instance.uiManager.ShowWheelResult(
                    sector.label, sector.description, sector.isReward,
                    () =>
                    {
                        if (player.isExtraTurn)
                        {
                            player.isExtraTurn = false;
                            GameManager.Instance.turnManager.SetActiveIndex(
                                GameManager.Instance.turnManager.ActivePlayerIndex);
                        }
                        else
                        {
                            GameManager.Instance.turnManager.NextTurn();
                        }
                    });
            });
        }

        private void ApplyWheelResult(PlayerState player, int sectorIdx)
        {
            switch (sectorIdx)
            {
                case 0:  player.tileIndex = Mathf.Max(0, player.tileIndex - 3); break;                     // Lùi 3
                case 1:  player.tileIndex = Mathf.Min(Constants.TotalTiles - 1, player.tileIndex + 3); break; // Tiến 3
                case 2:  player.skipTurn = true; break;                                                     // Mất lượt
                case 3:  player.isExtraTurn = true; break;                                                  // Thêm lượt
                case 4:  player.tileIndex = Mathf.Max(0, player.tileIndex - 2); break;                     // Lùi 2
                case 5:  player.shield = true; break;                                                       // Nhận Khiên
                case 6:  player.doubleDice = true; break;                                                   // x2
                case 7:  player.tileIndex = 0; break;                                                       // Quay về Start
                case 8:  player.tileIndex = Mathf.Min(Constants.TotalTiles - 1, player.tileIndex + 2); break; // Tiến 2
                case 9:  player.shield = false; break;                                                      // Mất Khiên
                case 10: player.tileIndex = Mathf.Min(Constants.TotalTiles - 1, player.tileIndex + 5); break; // Tiến 5
                case 11: player.tileIndex = Mathf.Max(0, player.tileIndex - 5); break;                     // Lùi 5
            }
        }
    }
}
