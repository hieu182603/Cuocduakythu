using System;
using System.Collections.Generic;
using UnityEngine;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Managers;
using CuocDuaKyThu.Utilities;

// Note: Requires Microsoft.AspNetCore.SignalR.Client package imported into Unity project.
// In actual implementation, the package DLLs are placed in Assets/Plugins.

namespace CuocDuaKyThu.Network
{
    public class SignalRManager : MonoBehaviour
    {
        public static SignalRManager Instance { get; private set; }

        private string _connectionId;
        public string LocalConnectionId => _connectionId;

        // Placeholders/Events for UI
        public event Action<string, List<PlayerState>> OnRoomCreatedEvent;
        public event Action<List<PlayerState>> OnPlayerJoinedEvent;
        public event Action<List<PlayerState>, int> OnGameStartedEvent;
        public event Action<string, int, int> OnDiceRolledEvent;
        public event Action<int, int, bool> OnPlayerMovedEvent;
        public event Action<string, string, List<string>, int> OnTriggerQuestionEvent;
        public event Action<string, bool, int, int, string> OnAnswerOutcomeEvent;
        public event Action<string, string, string, int, bool> OnTriggerTrapEvent;
        public event Action<string, string, string, int, bool, bool> OnTriggerRewardEvent;
        public event Action<string> OnTriggerShieldBlockEvent;
        public event Action<string> OnTriggerWheelEvent;
        public event Action<string, int, string, string, bool, int, bool, bool> OnWheelSpunEvent;
        public event Action<int> OnNextTurnTriggeredEvent;
        public event Action<PlayerState> OnGameFinishedEvent;
        public event Action<string, List<PlayerState>> OnPlayerDisconnectedEvent;
        public event Action<string> OnErrorEvent;

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

        public void Initialize(string url)
        {
            // TODO: Establish HubConnection with automatic reconnect:
            // _connection = new HubConnectionBuilder().WithUrl(url).WithAutomaticReconnect().Build();
            
            // TODO: Wire up events using _connection.On(...) matching the client RPC endpoints:
            // _connection.On<string, List<PlayerState>>("RoomCreated", (code, list) => { ... });
            // _connection.On<List<PlayerState>>("PlayerJoined", (list) => { ... });
            // _connection.On<List<PlayerState>, int>("GameStarted", (list, idx) => { ... });
            // _connection.On<string, int, int>("DiceRolled", (name, roll, move) => { ... });
            // _connection.On<int, int, bool>("PlayerMoved", (id, idx, lap) => { ... });
            // _connection.On<string, string, List<string>, int>("TriggerQuestion", (name, text, answers, streak) => { ... });
            // _connection.On<string, bool, int, int, string>("AnswerOutcome", (name, correct, correctIdx, streak, penalty) => { ... });
            // _connection.On<string, string, string, int, bool>("TriggerTrap", (name, trap, detail, idx, skip) => { ... });
            // _connection.On<string, string, string, int, bool, bool>("TriggerReward", (name, rew, detail, idx, shield, doubleD) => { ... });
            // _connection.On<string>("TriggerShieldBlock", (name) => { ... });
            // _connection.On<string>("TriggerWheel", (name) => { ... });
            // _connection.On<string, int, string, string, bool, int, bool, bool>("WheelSpun", (name, sectorIdx, label, desc, rew, idx, skip, shield) => { ... });
            // _connection.On<int>("NextTurnTriggered", (idx) => { ... });
            // _connection.On<PlayerState>("GameFinished", (winner) => { ... });
            // _connection.On<string, List<PlayerState>>("PlayerDisconnected", (name, list) => { ... });
            // _connection.On<string>("Error", (msg) => { ... });
            
            // TODO: Start connection asynchronously:
            // _connection.StartAsync();
            
            Debug.Log("[SignalRManager] Initialized. SignalR client library integration pending package import.");
        }

        public void CreateRoom(string playerName, string horseId)
        {
            // TODO: _connection.InvokeAsync("CreateRoom", playerName, horseId);
        }

        public void JoinRoom(string roomCode, string playerName, string horseId)
        {
            // TODO: _connection.InvokeAsync("JoinRoom", roomCode, playerName, horseId);
        }

        public void StartGame(string roomCode)
        {
            // TODO: _connection.InvokeAsync("StartGame", roomCode);
        }

        public void RollDice(string roomCode)
        {
            // TODO: _connection.InvokeAsync("RollDice", roomCode);
        }

        public void SubmitAnswer(string roomCode, int answerIndex)
        {
            // TODO: _connection.InvokeAsync("SubmitAnswer", roomCode, answerIndex);
        }

        public void SpinWheel(string roomCode)
        {
            // TODO: _connection.InvokeAsync("SpinWheel", roomCode);
        }

        public void CloseModal(string roomCode)
        {
            // TODO: _connection.InvokeAsync("CloseModal", roomCode);
        }
    }
}
