using System.Collections.Generic;
using UnityEngine;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Controls reward events: random selection, effect application, UI popup.
    /// Replaces triggerRewardEvent() from app.js.
    /// </summary>
    public class RewardManager : MonoBehaviour
    {
        private System.Random _random = new();

        /// <summary>Trigger a reward event for the given player (offline mode).</summary>
        public void TriggerRewardEvent(PlayerState player)
        {
            int idx = _random.Next(RewardDefinition.Rewards.Length);
            var reward = RewardDefinition.Rewards[idx];

            bool isExtraTurn = ApplyRewardEffect(player, reward);

            // Update board visuals
            GameManager.Instance.boardManager.TeleportToken(player);

            GameManager.Instance.uiManager.LogMessage(
                $"[{player.name}] kích hoạt thưởng: {reward.name} - {reward.detail}",
                UIManager.LogType.Reward);

            // Show popup — on close, advance turn
            GameManager.Instance.uiManager.ShowRewardPopup(reward.name, reward.detail, () =>
            {
                if (isExtraTurn)
                {
                    // Don't advance — repeat this player's turn
                    GameManager.Instance.turnManager.SetActiveIndex(
                        GameManager.Instance.turnManager.ActivePlayerIndex);
                }
                else
                {
                    GameManager.Instance.turnManager.NextTurn();
                }
            });
        }

        /// <summary>Apply reward effect to player state. Returns true if extra turn was granted.</summary>
        private bool ApplyRewardEffect(PlayerState player, RewardInfo reward)
        {
            switch (reward.type)
            {
                case "tiến":
                    player.tileIndex = Mathf.Min(Constants.TotalTiles - 1, player.tileIndex + reward.value);
                    return false;
                case "lá-chắn":
                    player.shield = true;
                    return false;
                case "thêm-lượt":
                    player.isExtraTurn = true;
                    return true;
                case "double-dice":
                    player.doubleDice = true;
                    return false;
                case "troll":
                    player.tileIndex = Mathf.Max(0, player.tileIndex + reward.value); // value is -2
                    return false;
                case "troll-nothing":
                    // No effect
                    return false;
                default:
                    return false;
            }
        }
    }
}
