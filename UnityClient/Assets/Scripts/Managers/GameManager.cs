using System.Collections.Generic;
using UnityEngine;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Central orchestrator. Singleton MonoBehaviour.
    /// Manages game mode, screen flow, and coordinates all other managers.
    /// No direct gameplay logic lives here — it dispatches to specialized managers.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Mode")]
        public bool isOnlineMode = false;

        [Header("Data Assets")]
        public CharacterData[] allCharacters;  // 10 characters, assigned in Inspector
        public TileData boardLayout;           // 32-tile layout ScriptableObject

        [Header("Manager References")]
        public TurnManager turnManager;
        public BoardManager boardManager;
        public DiceManager diceManager;
        public QuestionManager questionManager;
        public RewardManager rewardManager;
        public TrapManager trapManager;
        public WheelManager wheelManager;
        public PlayerManager playerManager;
        public AudioManager audioManager;
        public UIManager uiManager;
        public SaveManager saveManager;

        [Header("Network (Online Only)")]
        public Network.SignalRManager signalRManager;
        public Network.ApiClient apiClient;

        // ── Runtime State ──
        public GameScreen CurrentScreen { get; private set; } = GameScreen.Splash;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            saveManager.LoadSettings();
            ShowScreen(GameScreen.Splash);

            // Auto-transition from Splash to Menu
            Invoke(nameof(GoToMenu), Constants.SplashDuration);

            // TODO: Initialize SignalR connection for online mode
            // signalRManager.Initialize(Constants.DefaultServerUrl + Constants.HubEndpoint);
        }

        // ════════════════════════════════════════
        // SCREEN NAVIGATION
        // ════════════════════════════════════════

        public void ShowScreen(GameScreen screen)
        {
            CurrentScreen = screen;
            uiManager.ShowScreen(screen);
        }

        public void GoToMenu()
        {
            ShowScreen(GameScreen.Menu);
        }

        public void GoToLobby(bool online)
        {
            isOnlineMode = online;
            ShowScreen(GameScreen.Lobby);
        }

        // ════════════════════════════════════════
        // GAME LIFECYCLE
        // ════════════════════════════════════════

        /// <summary>Start an offline game. Called when lobby "Start" is pressed.</summary>
        public void StartOfflineGame(List<PlayerState> lobbyPlayers)
        {
            isOnlineMode = false;
            playerManager.InitializePlayers(lobbyPlayers);
            boardManager.BuildBoard(boardLayout);
            boardManager.PlaceAllTokens(playerManager.Players);

            // Load questions from Supabase via API
            questionManager.LoadQuestionsFromApi(() =>
            {
                turnManager.StartFirstTurn();
                ShowScreen(GameScreen.Gameplay);
                uiManager.LogMessage("Cuộc đua đã bắt đầu! Đang ở vạch xuất phát.", UIManager.LogType.Success);
            });
        }

        /// <summary>Called when a player reaches START after completing a lap.</summary>
        public void TriggerVictory(PlayerState winner)
        {
            uiManager.LogMessage(
                $"🎉 KÌ TÍCH! Tay đua [{winner.name}] chơi nhân vật [{winner.character.characterName}] đã chính thức vô địch!",
                UIManager.LogType.Success);

            uiManager.ShowVictoryScreen(winner, playerManager.GetLeaderboard());

            Invoke(nameof(ShowVictoryScreen), Constants.VictoryDelay);
        }

        private void ShowVictoryScreen()
        {
            ShowScreen(GameScreen.Victory);
        }

        /// <summary>Reset all runtime state for a new match.</summary>
        public void ResetGame()
        {
            playerManager.ClearPlayers();
            turnManager.Reset();
        }

        /// <summary>Quit current match and return to menu.</summary>
        public void QuitToMenu()
        {
            ResetGame();
            ShowScreen(GameScreen.Menu);
        }

        /// <summary>Return to lobby for a rematch.</summary>
        public void PlayAgain()
        {
            ResetGame();
            ShowScreen(GameScreen.Lobby);
        }
    }

    public enum GameScreen
    {
        Splash,
        Menu,
        Lobby,
        Gameplay,
        Victory
    }
}
