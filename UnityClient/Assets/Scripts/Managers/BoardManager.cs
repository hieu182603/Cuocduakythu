using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Gameplay;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Managers
{
    /// <summary>
    /// Controls board rendering, tile management, and player movement animation.
    /// Replaces renderBoard(), updatePlayerPositionsOnBoard(), movePlayerSequentially(), activateTile().
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("Board Setup")]
        public Transform boardParent;         // Parent transform for tile GameObjects
        public GameObject tilePrefab;          // Prefab for a single tile
        public GameObject playerTokenPrefab;   // Prefab for a horse token

        [Header("Grid Spacing")]
        public float cellWidth = 1.0f;
        public float cellHeight = 1.0f;

        // ── Runtime ──
        private Tile[] _tiles;
        private readonly Dictionary<int, PlayerToken> _playerTokens = new();

        public void BuildBoard(TileData layout)
        {
            // Clear existing tiles
            if (_tiles != null)
            {
                foreach (var tile in _tiles)
                    if (tile != null) Destroy(tile.gameObject);
            }

            _tiles = new Tile[layout.tiles.Length];

            for (int i = 0; i < layout.tiles.Length; i++)
            {
                var entry = layout.tiles[i];
                Vector3 position = new Vector3(entry.col * cellWidth, 0, -entry.row * cellHeight);

                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, boardParent);
                tileObj.name = $"Tile_{i}_{entry.type}";

                Tile tileComponent = tileObj.GetComponent<Tile>();
                tileComponent.Initialize(i, entry);
                _tiles[i] = tileComponent;
            }
        }

        public void PlaceAllTokens(List<PlayerState> players)
        {
            // Clear existing tokens
            foreach (var kvp in _playerTokens)
                if (kvp.Value != null) Destroy(kvp.Value.gameObject);
            _playerTokens.Clear();

            // Create tokens at their tile positions
            foreach (var player in players)
            {
                CreateToken(player);
            }
        }

        private void CreateToken(PlayerState player)
        {
            if (_tiles == null || player.tileIndex >= _tiles.Length) return;

            Vector3 tilePos = _tiles[player.tileIndex].GetStandPosition(_playerTokens.Count);
            GameObject tokenObj = Instantiate(playerTokenPrefab, tilePos, Quaternion.identity, boardParent);

            PlayerToken token = tokenObj.GetComponent<PlayerToken>();
            token.Initialize(player);
            _playerTokens[player.id] = token;
        }

        /// <summary>Highlight the active tile (where the current player stands).</summary>
        public void HighlightTile(int tileIndex)
        {
            if (_tiles == null) return;
            for (int i = 0; i < _tiles.Length; i++)
            {
                _tiles[i].SetHighlight(i == tileIndex);
            }
        }

        /// <summary>Get the TileType at a given index.</summary>
        public TileType GetTileType(int tileIndex)
        {
            if (_tiles == null || tileIndex < 0 || tileIndex >= _tiles.Length)
                return TileType.Start;
            return _tiles[tileIndex].TileEntry.type;
        }

        // ════════════════════════════════════════
        // SEQUENTIAL MOVEMENT
        // ════════════════════════════════════════

        /// <summary>
        /// Move a player token step-by-step along the board.
        /// Uses coroutine for smooth sequential hopping with DOTween.
        /// Calls onComplete when finished landing on the final tile.
        /// </summary>
        public void MovePlayerSequentially(PlayerState player, int steps, System.Action onComplete)
        {
            StartCoroutine(MoveSequence(player, steps, onComplete));
        }

        private IEnumerator MoveSequence(PlayerState player, int steps, System.Action onComplete)
        {
            if (!_playerTokens.TryGetValue(player.id, out var token))
            {
                onComplete?.Invoke();
                yield break;
            }

            float stepDelay = GameManager.Instance.saveManager.CurrentSettings.speed switch
            {
                SpeedSetting.Normal => Constants.HopDurationNormal,
                SpeedSetting.Fast => Constants.HopDurationFast,
                SpeedSetting.Instant => 0f,
                _ => Constants.HopDurationNormal
            };

            for (int i = 0; i < steps; i++)
            {
                // Advance position
                player.tileIndex = (player.tileIndex + 1) % Constants.TotalTiles;

                // Check lap completion
                if (player.tileIndex == 0)
                    player.lapCompleted = true;

                // Highlight tile
                HighlightTile(player.tileIndex);

                // Animate hop to new tile position
                Vector3 targetPos = _tiles[player.tileIndex].GetStandPosition(0);
                token.HopTo(targetPos, stepDelay);

                if (stepDelay > 0)
                    yield return new WaitForSeconds(stepDelay);
            }

            // Update all token positions (handles stacking)
            RefreshTokenPositions();

            onComplete?.Invoke();
        }

        /// <summary>Reposition all tokens on their current tiles (handles multi-character stacking).</summary>
        public void RefreshTokenPositions()
        {
            if (_tiles == null) return;

            // Group players by tile
            var tileGroups = new Dictionary<int, List<int>>();
            foreach (var kvp in _playerTokens)
            {
                var player = GameManager.Instance.playerManager.GetPlayer(kvp.Key);
                if (player == null) continue;

                if (!tileGroups.ContainsKey(player.tileIndex))
                    tileGroups[player.tileIndex] = new List<int>();
                tileGroups[player.tileIndex].Add(kvp.Key);
            }

            foreach (var group in tileGroups)
            {
                int tileIdx = group.Key;
                var playerIds = group.Value;

                for (int i = 0; i < playerIds.Count; i++)
                {
                    if (_playerTokens.TryGetValue(playerIds[i], out var token))
                    {
                        Vector3 pos = _tiles[tileIdx].GetStandPosition(i);
                        token.transform.position = pos;
                    }
                }
            }
        }

        /// <summary>Instantly teleport a player token to a specific tile (for trap effects).</summary>
        public void TeleportToken(PlayerState player)
        {
            if (_playerTokens.TryGetValue(player.id, out var token) && _tiles != null)
            {
                Vector3 pos = _tiles[player.tileIndex].GetStandPosition(0);
                token.transform.position = pos;
            }
            RefreshTokenPositions();
        }
    }
}
