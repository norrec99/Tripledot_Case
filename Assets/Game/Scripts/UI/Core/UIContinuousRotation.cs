using DG.Tweening;
using UnityEngine;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// A lightweight VFX utility that infinitely rotates a UI image.
    /// Perfect for the "light rays" burst background behind the big yellow reward star.
    /// </summary>
    public class UIContinuousRotation : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Seconds to complete one full 360-degree rotation.")]
        [SerializeField] private float _durationPerRotation = 10f;
        
        [Tooltip("If true, rotates counter-clockwise.")]
        [SerializeField] private bool _reverseDirection = false;

        private Tween _rotateTween;

        private void OnEnable()
        {
            float targetZ = _reverseDirection ? 360f : -360f;

            // Start from current rotation so it seamlessly resumes if toggled quickly
            Vector3 currentEuler = transform.localEulerAngles;

            _rotateTween = transform.DORotate(new Vector3(0, 0, currentEuler.z + targetZ), _durationPerRotation, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental); // Infinite looping
        }

        private void OnDisable()
        {
            _rotateTween?.Kill();
        }
    }
}
