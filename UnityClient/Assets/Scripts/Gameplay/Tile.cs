using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CuocDuaKyThu.Data;

namespace CuocDuaKyThu.Gameplay
{
    public class Tile : MonoBehaviour
    {
        [Header("UI & Visual Elements")]
        public Image backgroundImage;
        public Image iconImage;
        public TMP_Text tileNumberText;
        public GameObject highlightOverlay; // For active-landing status

        [Header("Character Stacking Offset")]
        public float stackOffsetX = 0.15f;
        public float stackOffsetY = 0.15f;

        private int _index;
        private TileEntry _entry;

        public int Index => _index;
        public TileEntry TileEntry => _entry;

        public void Initialize(int index, TileEntry entry)
        {
            _index = index;
            _entry = entry;

            if (tileNumberText != null)
            {
                tileNumberText.text = (index + 1).ToString();
            }

            if (highlightOverlay != null)
            {
                highlightOverlay.SetActive(false);
            }

            ApplyVisualThemes();
        }

        private void ApplyVisualThemes()
        {
            if (backgroundImage == null) return;

            // Apply GDD colors
            backgroundImage.color = _entry.type switch
            {
                TileType.Start => new Color(0.137f, 0.584f, 0.286f), // Green
                TileType.Question => new Color(0.490f, 0.224f, 0.733f), // Purple
                TileType.Trap => new Color(0.863f, 0.196f, 0.196f), // Red
                TileType.Reward => new Color(0.251f, 0.635f, 0.314f), // Lighter Green/Gold
                TileType.Wheel => new Color(0.129f, 0.447f, 0.706f), // Blue
                _ => Color.gray
            };
            
            // TODO: Load specific icons from Addressables (bomb, gift, wheel, etc.)
        }

        public void SetHighlight(bool active)
        {
            if (highlightOverlay != null)
            {
                highlightOverlay.SetActive(active);
            }
        }

        /// <summary>
        /// Calculate the position where a token should stand on this tile, 
        /// supporting offsets for multi-character stacking.
        /// </summary>
        public Vector3 GetStandPosition(int playerOffsetIndex)
        {
            Vector3 basePos = transform.position;
            // 2D isometric offset or simple grid offset
            return new Vector3(
                basePos.x + (playerOffsetIndex * stackOffsetX),
                basePos.y,
                basePos.z - (playerOffsetIndex * stackOffsetY)
            );
        }
    }
}
