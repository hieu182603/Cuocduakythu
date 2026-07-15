using UnityEngine;
using DG.Tweening;
using CuocDuaKyThu.Data;
using CuocDuaKyThu.Managers;

namespace CuocDuaKyThu.Gameplay
{
    public class PlayerToken : MonoBehaviour
    {
        [Header("Visuals")]
        public SpriteRenderer characterSpriteRenderer;
        public SpriteRenderer badgeSpriteRenderer;
        
        [Header("Emotes")]
        public SpriteRenderer emoteBubble;
        public Sprite emoteHappy;
        public Sprite emoteFailed;
        public Sprite emoteAngry;
        public Sprite emoteConfident;
        public Sprite emoteSurprised;
        public Sprite emoteVictory;

        [Header("Parameters")]
        public float hopHeight = 0.5f;

        private PlayerState _state;
        private GameObject _vfxInstance;
        private Sequence _idleSequence;

        public PlayerState State => _state;

        public void Initialize(PlayerState player)
        {
            _state = player;
            name = $"PlayerToken_{player.id}_{player.name}";

            // Set colors and badges
            if (characterSpriteRenderer != null)
            {
                characterSpriteRenderer.color = player.character.mainColor;
                if (player.character.miniSprite != null)
                {
                    characterSpriteRenderer.sprite = player.character.miniSprite;
                }
            }

            if (badgeSpriteRenderer != null && player.character.badgeSprite != null)
            {
                badgeSpriteRenderer.sprite = player.character.badgeSprite;
            }

            // Spawn special elemental VFX if enabled in settings
            var settings = GameManager.Instance.saveManager.CurrentSettings;
            if (settings.vfxEnabled && player.character.vfxPrefab != null)
            {
                _vfxInstance = Instantiate(player.character.vfxPrefab, transform);
                _vfxInstance.transform.localPosition = Vector3.zero;
            }

            // Hide emote bubble initially
            if (emoteBubble != null) emoteBubble.gameObject.SetActive(false);

            // Start idle animation (subtle breathing nhấp nhô nhẹ)
            StartIdleAnimation();
        }

        private void StartIdleAnimation()
        {
            if (characterSpriteRenderer == null) return;

            _idleSequence = DOTween.Sequence();
            _idleSequence.Append(characterSpriteRenderer.transform.DOLocalMoveY(0.05f, 0.5f).SetEase(Ease.InOutQuad))
                         .Append(characterSpriteRenderer.transform.DOLocalMoveY(0f, 0.5f).SetEase(Ease.InOutQuad))
                         .SetLoops(-1);
        }

        public void HopTo(Vector3 targetPosition, float duration)
        {
            // Pause idle animation during movement
            _idleSequence?.Pause();

            if (duration <= 0f)
            {
                transform.position = targetPosition;
                _idleSequence?.Play();
                return;
            }

            // Jump to target in arc shape
            transform.DOJump(targetPosition, hopHeight, 1, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _idleSequence?.Play();
                });
        }

        public void ShowEmote(string emotion, float duration = 2.0f)
        {
            if (emoteBubble == null) return;

            Sprite selectedEmote = emotion.ToLower() switch
            {
                "happy" => emoteHappy,
                "failed" => emoteFailed,
                "angry" => emoteAngry,
                "confident" => emoteConfident,
                "surprised" => emoteSurprised,
                "victory" => emoteVictory,
                _ => null
            };

            if (selectedEmote != null)
            {
                emoteBubble.sprite = selectedEmote;
                emoteBubble.gameObject.SetActive(true);
                // Bubble micro-animation
                emoteBubble.transform.localScale = Vector3.zero;
                emoteBubble.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

                CancelInvoke(nameof(HideEmote));
                Invoke(nameof(HideEmote), duration);
            }
        }

        private void HideEmote()
        {
            if (emoteBubble != null)
            {
                emoteBubble.transform.DOScale(Vector3.zero, 0.15f).OnComplete(() =>
                {
                    emoteBubble.gameObject.SetActive(false);
                });
            }
        }

        private void OnDestroy()
        {
            _idleSequence?.Kill();
        }
    }
}
