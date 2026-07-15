using UnityEngine;

namespace CuocDuaKyThu.Data
{
    /// <summary>
    /// ScriptableObject defining a single horse character.
    /// Create 10 instances in Unity via Assets → Create → CuocDuaKyThu → CharacterData.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "CuocDuaKyThu/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterId;     // "01"–"10"
        public string characterName;   // "Lộc Phát", "Bạch Mã", etc.

        [Header("Visuals")]
        public Color mainColor = Color.white;
        public string badgeEmoji;      // "⚡", "❄️", "🔥", etc.
        public Sprite artworkSprite;   // Full artwork for Character Select screen
        public Sprite miniSprite;      // Mini version for board token
        public Sprite badgeSprite;     // Chest badge icon

        [Header("VFX")]
        public VfxType vfxType = VfxType.None;
        public GameObject vfxPrefab;   // Particle system prefab for elemental effect

        [Header("Animation")]
        public RuntimeAnimatorController animatorController; // Shared or per-character
    }

    public enum VfxType
    {
        None,
        Lightning,  // Lộc Phát
        Water,      // Bạch Mã
        Fire,       // Hỏa Long
        Ice,        // Băng Phong
        Sakura      // Sakura
    }
}
