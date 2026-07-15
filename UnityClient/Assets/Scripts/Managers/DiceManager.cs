using UnityEngine;
using DG.Tweening;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Controls dice rolling: animation, value calculation, modifier application.
    /// Replaces handleRollDice() from app.js.
    /// </summary>
    public class DiceManager : MonoBehaviour
    {
        [Header("References")]
        public Gameplay.Dice diceObject;  // The visual dice in the scene

        private System.Random _random = new();

        /// <summary>
        /// Roll the dice for the active player. Handles modifiers and double-dice.
        /// </summary>
        public void Roll(PlayerState player, System.Action<int> onResult)
        {
            if (GameManager.Instance.isOnlineMode)
            {
                // TODO: Online mode — invoke SignalR RollDice instead
                // GameManager.Instance.signalRManager.RollDice(roomCode);
                return;
            }

            // Disable dice button during roll
            GameManager.Instance.uiManager.SetDiceInteractable(false);

            // Play shake animation
            diceObject.PlayShakeAnimation(() =>
            {
                // Calculate roll value
                int rollVal = _random.Next(Constants.DiceMin, Constants.DiceMax + 1);

                // Apply dice modifier (reduced max)
                if (player.diceModifier > 0)
                {
                    rollVal = Mathf.Min(Constants.ReducedDiceMax, rollVal);
                    player.diceModifier--;
                }

                int totalMove = rollVal;

                // Apply double dice
                if (player.doubleDice)
                {
                    totalMove = rollVal * 2;
                    player.doubleDice = false;
                    GameManager.Instance.uiManager.SetDiceModifierText($"Double Dice: {rollVal} x 2 = {totalMove}");
                }
                else
                {
                    GameManager.Instance.uiManager.SetDiceModifierText($"Động cơ tung ra: {rollVal} ô");
                }

                // Update dice visual
                diceObject.ShowResult(totalMove);

                GameManager.Instance.uiManager.LogMessage(
                    $"[{player.name}] xúc xắc được {totalMove} ô.");

                onResult?.Invoke(totalMove);
            });
        }

        /// <summary>Reset dice visual to "?" state.</summary>
        public void ResetDiceVisual()
        {
            diceObject.ResetVisual();
            GameManager.Instance.uiManager.SetDiceModifierText("");
        }

        /// <summary>Display a dice result without rolling (used by SignalR sync).</summary>
        public void DisplayResult(int totalMove, string description)
        {
            diceObject.PlayShakeAnimation(() =>
            {
                diceObject.ShowResult(totalMove);
                GameManager.Instance.uiManager.SetDiceModifierText(description);
            });
        }
    }
}
