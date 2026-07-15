using System;
using UnityEngine;
using DG.Tweening;

namespace CuocDuaKyThu.Gameplay
{
    public class SpinWheel : MonoBehaviour
    {
        public RectTransform wheelRectTransform; // The wheel image that will rotate
        public float totalSectors = 12f;

        /// <summary>
        /// Spin the wheel to land on a specific sector index.
        /// </summary>
        public void Spin(int targetSectorIndex, float duration, Action onComplete)
        {
            if (wheelRectTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            // A sector's arc angle in degrees
            float sectorArc = 360f / totalSectors;

            // Offset rotation so the pointer points to the center of the sector.
            // Pointer is usually at the top (12 o'clock, 0 degrees offset relative to top).
            // Sector 0 is drawn from 0 to sectorArc. The center of sector 0 is at sectorArc/2.
            // When we rotate the wheel by R, a point at angle P lands under the top pointer if P + R = 90 or similar.
            // In C# GameHub, winningIndex matches final angle. Let's compute matching rotation.
            // Sector index goes counter-clockwise/clockwise. We just target the precise angle:
            // targetAngle = 360 * 5 (rotations) + (targetSectorIndex * sectorArc) + (sectorArc / 2)
            float targetAngle = (360f * 5f) + (targetSectorIndex * sectorArc) + (sectorArc / 2f);

            // Spin using DOTween with Ease.OutCubic
            wheelRectTransform.DORotate(new Vector3(0, 0, targetAngle), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    // Normalize rotation to keep values clean
                    float finalZ = targetAngle % 360f;
                    wheelRectTransform.localRotation = Quaternion.Euler(0, 0, finalZ);
                    onComplete?.Invoke();
                });
        }
    }
}
