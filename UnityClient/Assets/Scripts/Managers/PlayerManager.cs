using System.Collections.Generic;
using UnityEngine;
using CuocDuaKyThu.Data;

namespace CuocDuaKyThu.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        private readonly List<PlayerState> _players = new();

        public List<PlayerState> Players => _players;
        public int PlayerCount => _players.Count;

        public void InitializePlayers(List<PlayerState> lobbyPlayers)
        {
            _players.Clear();
            foreach (var p in lobbyPlayers)
            {
                p.ResetForNewGame();
                _players.Add(p);
            }
        }

        public PlayerState GetPlayer(int index)
        {
            if (index < 0 || index >= _players.Count) return null;
            return _players[index];
        }

        public PlayerState GetPlayerById(int id)
        {
            return _players.Find(p => p.id == id);
        }

        public void ClearPlayers()
        {
            _players.Clear();
        }

        public List<PlayerState> GetLeaderboard()
        {
            var sorted = new List<PlayerState>(_players);
            // Sort by tileIndex descending
            sorted.Sort((a, b) => b.tileIndex.CompareTo(a.tileIndex));
            return sorted;
        }
    }
}
