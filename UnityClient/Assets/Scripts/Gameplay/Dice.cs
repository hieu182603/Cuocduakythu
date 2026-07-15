using System;
using TMPro;
using UnityEngine;
using DG.Tweening;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Gameplay
{
    public class Dice : MonoBehaviour
    {
        public TMP_Text valueText;
        public float shakeStrength = 15f;
        public float shakeDuration = Constants.DiceShakeDuration;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private void Start()
        {
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
            ResetVisual();
        }

        public void ResetVisual()
        {
            if (valueText != null) valueText.text = "?";
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
        }

        public void PlayShakeAnimation(Action onComplete)
        {
            // Shake position and rotate randomly using DOTween
            Sequence rollSequence = DOTween.Sequence();
            
            rollSequence.Append(transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true))
                        .Join(transform.DORotate(new Vector3(0, 0, UnityEngine.Random.Range(360f, 720f)), shakeDuration, RotateMode.FastBeyond360))
                        .OnComplete(() =>
                        {
                            onComplete?.Invoke();
                        });
        }

        public void ShowResult(int value)
        {
            if (valueText != null) valueText.text = value.ToString();
            // Subtle pop animation
            transform.localScale = Vector3.one * 1.2f;
            transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }
    }
}
