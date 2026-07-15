using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Screens")]
        public GameObject splashPanel;
        public GameObject menuPanel;
        public GameObject lobbyPanel;
        public GameObject gameplayPanel;
        public GameObject victoryPanel;

        [Header("Common Modals")]
        public GameObject settingsModal;
        public GameObject questionPoolModal;

        [Header("Gameplay Popups")]
        public GameObject questionModal;
        public GameObject trapModal;
        public GameObject rewardModal;
        public GameObject wheelModal;

        [Header("Lobby Controls")]
        public TMP_Text playerCountText;
        public Transform lobbyPlayersContainer;
        public GameObject lobbyPlayerCardPrefab;
        public GameObject startButton;
        public TMP_Text lobbyWaitingMsg;
        public TMP_Text lobbyRoomCodeText;

        [Header("Gameplay UI elements")]
        public TMP_Text activePlayerNameText;
        public TMP_Text activePlayerCharDescText;
        public TMP_Text diceValueText;
        public TMP_Text diceModifiersDescText;
        public TMP_Text turnStatusText;
        public Button rollDiceButton;
        public Transform scoreboardContainer;
        public GameObject scoreboardItemPrefab;
        public Transform gameLogsContainer;
        public GameObject gameLogItemPrefab;
        public Transform activePlayerEffectsContainer;
        public GameObject effectBadgePrefab;

        [Header("Question Popup elements")]
        public TMP_Text questionText;
        public TMP_Text wrongStreakWarningText;
        public Button[] answerButtons; // Normally 4 buttons

        [Header("Trap Popup elements")]
        public TMP_Text trapEffectNameText;
        public TMP_Text trapEffectDetailText;
        public Button closeTrapButton;

        [Header("Reward Popup elements")]
        public TMP_Text rewardEffectNameText;
        public TMP_Text rewardEffectDetailText;
        public Button closeRewardButton;

        [Header("Wheel Popup elements")]
        public TMP_Text wheelResultText;
        public Button spinWheelButton;
        public Button closeWheelButton;

        [Header("Victory Screen elements")]
        public TMP_Text winnerNameText;
        public TMP_Text winnerCharText;
        public Image winnerAvatarImage;
        public Transform victoryLeaderboardContainer;
        public GameObject victoryLeaderboardRowPrefab;

        // Callback delegates for popups
        private Action<int> _onAnswerSelected;
        private Action _onTrapClosed;
        private Action _onRewardClosed;
        private Action _onWheelSpun;
        private Action _onWheelClosed;

        public enum LogType
        {
            Info,
            Question,
            Reward,
            Trap,
            Success
        }

        private void Start()
        {
            // Bind buttons
            closeTrapButton.onClick.AddListener(() => {
                trapModal.SetActive(false);
                _onTrapClosed?.Invoke();
            });

            closeRewardButton.onClick.AddListener(() => {
                rewardModal.SetActive(false);
                _onRewardClosed?.Invoke();
            });

            spinWheelButton.onClick.AddListener(() => {
                spinWheelButton.interactable = false;
                _onWheelSpun?.Invoke();
            });

            closeWheelButton.onClick.AddListener(() => {
                wheelModal.SetActive(false);
                _onWheelClosed?.Invoke();
            });

            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i;
                answerButtons[i].onClick.AddListener(() => {
                    SetAnswerButtonsInteractable(false);
                    _onAnswerSelected?.Invoke(index);
                });
            }
        }

        public void ShowScreen(GameScreen screen)
        {
            splashPanel.SetActive(screen == GameScreen.Splash);
            menuPanel.SetActive(screen == GameScreen.Menu);
            lobbyPanel.SetActive(screen == GameScreen.Lobby);
            gameplayPanel.SetActive(screen == GameScreen.Gameplay);
            victoryPanel.SetActive(screen == GameScreen.Victory);
        }

        // ════════════════════════════════════════
        // LOBBY UI
        // ════════════════════════════════════════

        public void UpdateLobby(List<PlayerState> lobbyPlayers, string roomCode = "", bool isOnline = false)
        {
            playerCountText.text = lobbyPlayers.Count.ToString();
            lobbyRoomCodeText.text = roomCode;

            // Clear old cards
            foreach (Transform child in lobbyPlayersContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                var player = lobbyPlayers[i];
                GameObject card = Instantiate(lobbyPlayerCardPrefab, lobbyPlayersContainer);
                // Setup card UI (name input, horse pick button, etc.)
                // TODO: Wire up input field and button events
            }

            if (isOnline)
            {
                startButton.SetActive(false); // Only host starts, handled by SignalR lobby logic
                lobbyWaitingMsg.gameObject.SetActive(true);
            }
            else
            {
                startButton.SetActive(true);
                lobbyWaitingMsg.gameObject.SetActive(false);
            }
        }

        // ════════════════════════════════════════
        // GAMEPLAY UI
        // ════════════════════════════════════════

        public void UpdateTurnInfo(PlayerState player)
        {
            activePlayerNameText.text = player.name;
            activePlayerNameText.color = player.character.mainColor;
            activePlayerCharDescText.text = $"{player.character.characterName} ({player.character.badgeEmoji})";

            // Clear effects
            foreach (Transform child in activePlayerEffectsContainer)
            {
                Destroy(child.gameObject);
            }

            if (player.shield) CreateEffectBadge("Lá chắn", Color.cyan);
            if (player.skipTurn) CreateEffectBadge("Bị Choáng/Mất lượt", Color.red);
            if (player.doubleDice) CreateEffectBadge("Double Dice", Color.yellow);
            if (player.diceModifier > 0) CreateEffectBadge($"Xúc xắc tối đa 3 ({player.diceModifier} lượt)", Color.red);
        }

        private void CreateEffectBadge(string text, Color color)
        {
            GameObject badge = Instantiate(effectBadgePrefab, activePlayerEffectsContainer);
            var badgeText = badge.GetComponentInChildren<TMP_Text>();
            badgeText.text = text;
            badge.GetComponent<Image>().color = color;
        }

        public void SetDiceInteractable(bool active)
        {
            rollDiceButton.interactable = active;
        }

        public void SetTurnStatusText(string text)
        {
            turnStatusText.text = text;
        }

        public void SetDiceModifierText(string text)
        {
            diceModifiersDescText.text = text;
        }

        public void UpdateScoreboard(List<PlayerState> sortedPlayers, int activePlayerId)
        {
            foreach (Transform child in scoreboardContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                var player = sortedPlayers[i];
                GameObject row = Instantiate(scoreboardItemPrefab, scoreboardContainer);
                
                // Highlight active player row
                var bgImage = row.GetComponent<Image>();
                if (player.id == activePlayerId && bgImage != null)
                {
                    bgImage.color = new Color(1f, 1f, 1f, 0.15f);
                }

                // Fill details
                var nameText = row.transform.Find("PlayerName").GetComponent<TMP_Text>();
                nameText.text = $"{player.name}";
                if (player.shield) nameText.text += " 🛡️";
                if (player.skipTurn) nameText.text += " 🚫";

                row.transform.Find("RankText").GetComponent<TMP_Text>().text = $"Hạng {i + 1}";
                row.transform.Find("PositionText").GetComponent<TMP_Text>().text = $"Ô {player.tileIndex + 1}";
                row.transform.Find("CharAvatar").GetComponent<Image>().color = player.character.mainColor;
            }
        }

        public void LogMessage(string text, LogType type = LogType.Info)
        {
            GameObject item = Instantiate(gameLogItemPrefab, gameLogsContainer);
            var logText = item.GetComponent<TMP_Text>();
            
            string timeStr = DateTime.Now.ToString("HH:mm:ss");
            string colorHex = type switch
            {
                LogType.Question => "#eab308", // Yellow
                LogType.Reward => "#22c55e",   // Green
                LogType.Trap => "#ef4444",     // Red
                LogType.Success => "#3b82f6",  // Blue
                _ => "#94a3b8"                 // Gray
            };

            logText.text = $"<color=#64748b>[{timeStr}]</color> <color={colorHex}>{text}</color>";
            Canvas.ForceUpdateCanvases();
            // Scroll to bottom
            var scrollRect = gameLogsContainer.parent.GetComponent<ScrollRect>();
            if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;
        }

        // ════════════════════════════════════════
        // QUESTION POPUP
        // ════════════════════════════════════════

        public void ShowQuestionPopup(QuestionDTO question, int wrongStreak, Action<int> onAnswerCallback)
        {
            _onAnswerSelected = onAnswerCallback;
            questionText.text = question.questionText;

            if (wrongStreak > 0)
            {
                wrongStreakWarningText.text = $"⚠️ CHUỖI SAI: {wrongStreak} lần! (Phạt cộng thêm {wrongStreak * 5} giây chờ)";
                wrongStreakWarningText.gameObject.SetActive(true);
            }
            else
            {
                wrongStreakWarningText.gameObject.SetActive(false);
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < question.options.Count)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerButtons[i].GetComponentInChildren<TMP_Text>().text = $"{question.options[i].optionLetter}. {question.options[i].optionText}";
                    answerButtons[i].image.color = Color.white; // Reset color
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            SetAnswerButtonsInteractable(true);
            questionModal.SetActive(true);
        }

        public void HideQuestionPopup()
        {
            questionModal.SetActive(false);
        }

        private void SetAnswerButtonsInteractable(bool interactable)
        {
            foreach (var btn in answerButtons)
            {
                btn.interactable = interactable;
            }
        }

        // ════════════════════════════════════════
        // TRAP POPUP
        // ════════════════════════════════════════

        public void ShowTrapPopup(string name, string detail, Action onCloseCallback)
        {
            _onTrapClosed = onCloseCallback;
            trapEffectNameText.text = name;
            trapEffectDetailText.text = detail;
            closeTrapButton.gameObject.SetActive(true);
            trapModal.SetActive(true);
        }

        // ════════════════════════════════════════
        // REWARD POPUP
        // ════════════════════════════════════════

        public void ShowRewardPopup(string name, string detail, Action onCloseCallback)
        {
            _onRewardClosed = onCloseCallback;
            rewardEffectNameText.text = name;
            rewardEffectDetailText.text = detail;
            closeRewardButton.gameObject.SetActive(true);
            rewardModal.SetActive(true);
        }

        // ════════════════════════════════════════
        // WHEEL POPUP
        // ════════════════════════════════════════

        public void ShowWheelPopup(Action onSpinCallback)
        {
            _onWheelSpun = onSpinCallback;
            wheelResultText.text = "Hãy ấn nút Xoay để xem thử!";
            spinWheelButton.gameObject.SetActive(true);
            spinWheelButton.interactable = true;
            closeWheelButton.gameObject.SetActive(false);
            wheelModal.SetActive(true);
        }

        public void ShowWheelResult(string label, string desc, bool isReward, Action onCloseCallback)
        {
            _onWheelClosed = onCloseCallback;
            string color = isReward ? "green" : "red";
            wheelResultText.text = $"<color={color}>{label}</color>\n<size=80%>{desc}</size>";
            spinWheelButton.gameObject.SetActive(false);
            closeWheelButton.gameObject.SetActive(true);
        }

        // ════════════════════════════════════════
        // VICTORY SCREEN
        // ════════════════════════════════════════

        public void ShowVictoryScreen(PlayerState winner, List<PlayerState> sortedPlayers)
        {
            winnerNameText.text = winner.name;
            winnerCharText.text = $"{winner.character.characterName} ({winner.character.badgeEmoji})";
            if (winnerAvatarImage != null)
            {
                winnerAvatarImage.color = winner.character.mainColor;
                winnerAvatarImage.sprite = winner.character.artworkSprite;
            }

            foreach (Transform child in victoryLeaderboardContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                var player = sortedPlayers[i];
                GameObject row = Instantiate(victoryLeaderboardRowPrefab, victoryLeaderboardContainer);
                
                row.transform.Find("RankText").GetComponent<TMP_Text>().text = (i + 1).ToString();
                row.transform.Find("PlayerName").GetComponent<TMP_Text>().text = player.name;
                row.transform.Find("CharText").GetComponent<TMP_Text>().text = player.character.characterName;
                row.transform.Find("PositionText").GetComponent<TMP_Text>().text = $"Ô {player.tileIndex + 1}";
            }
        }
    }
}
