using System.Collections.Generic;
using UnityEngine;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Controls trap events: shield check, random selection, effect application.
    /// Replaces triggerTrapEvent() from app.js.
    /// </summary>
    public class TrapManager : MonoBehaviour
    {
        private System.Random _random = new();

        /// <summary>Trigger a trap event for the given player (offline mode).</summary>
        public void TriggerTrapEvent(PlayerState player)
        {
            // Shield blocks trap
            if (player.shield)
            {
                player.shield = false;
                GameManager.Instance.uiManager.LogMessage(
                    $"Lá Chắn của [{player.name}] đã kích hoạt thành công, giúp chặn đứng bẫy hiểm họa!",
                    UIManager.LogType.Reward);

                GameManager.Instance.uiManager.ShowRewardPopup(
                    "KÍCH HOẠT LÁ CHẮN!",
                    "Bẫy hiểm họa đã bị hóa giải hoàn toàn bằng khiên phòng ngự!",
                    () => GameManager.Instance.turnManager.NextTurn());
                return;
            }

            // Select random trap
            int trapIdx = _random.Next(TrapDefinition.Traps.Length);

            // Reduce "quay-start" (index 5) chance
            if (trapIdx == 5 && _random.NextDouble() > Constants.RareStartTrapChance)
            {
                trapIdx = 0; // fallback to simple "lùi 2"
            }

            var trap = TrapDefinition.Traps[trapIdx];
            ApplyTrapEffect(player, trapIdx);

            GameManager.Instance.boardManager.TeleportToken(player);

            GameManager.Instance.uiManager.LogMessage(
                $"[{player.name}] kích hoạt bẫy: {trap.name} - {trap.detail}",
                UIManager.LogType.Trap);

            GameManager.Instance.uiManager.ShowTrapPopup(trap.name, trap.detail, () =>
            {
                GameManager.Instance.turnManager.NextTurn();
            });
        }

        private void ApplyTrapEffect(PlayerState player, int trapIdx)
        {
            var players = GameManager.Instance.playerManager.Players;

            switch (trapIdx)
            {
                case 0: // Lùi 2
                    player.tileIndex = Mathf.Max(0, player.tileIndex - 2);
                    break;
                case 1: // Lùi 5
                    player.tileIndex = Mathf.Max(0, player.tileIndex - 5);
                    break;
                case 2: // Mất lượt
                    player.skipTurn = true;
                    break;
                case 3: // Dice max 3 for 2 turns
                    player.diceModifier = 2;
                    break;
                case 4: // Swap positions
                    var candidates = players.FindAll(p => p.id != player.id);
                    if (candidates.Count > 0)
                    {
                        var target = candidates[_random.Next(candidates.Count)];
                        (player.tileIndex, target.tileIndex) = (target.tileIndex, player.tileIndex);

                        GameManager.Instance.uiManager.LogMessage(
                            $"[{player.name}] đã tráo đổi vị trí với [{target.name}].",
                            UIManager.LogType.Trap);
                    }
                    break;
                case 5: // Return to Start
                    player.tileIndex = 0;
                    break;
            }
        }
    }
}
