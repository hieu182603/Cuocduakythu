using System.Collections.Generic;
using UnityEngine;

namespace CuocDuaKyThu.Gameplay
{
    public class Board : MonoBehaviour
    {
        private readonly List<Tile> _tiles = new();

        public void AddTile(Tile tile)
        {
            _tiles.Add(tile);
        }

        public Tile GetTile(int index)
        {
            if (index < 0 || index >= _tiles.Count) return null;
            return _tiles[index];
        }

        public void Clear()
        {
            _tiles.Clear();
        }
    }
}
