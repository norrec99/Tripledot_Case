using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// A premium utility component that provides tactile hover/press feedback for any UI element.
    /// </summary>
    public class AnimatedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Animation Settings")]
        [SerializeField] private float _pressScaleMultiplier = 0.9f;
        [SerializeField] private float _animationDuration = 0.1f;
        [SerializeField] private Ease _easeType = Ease.OutQuad;

        private Vector3 _originalScale;
        private Sequence _animSequence;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void OnDisable()
        {
            // Failsafe in case the popup is destroyed/hidden while the button is pressed
            _animSequence?.Kill();
            transform.localScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _animSequence?.Kill();
            _animSequence = DOTween.Sequence()
                .Append(transform.DOScale(_originalScale * _pressScaleMultiplier, _animationDuration)
                                 .SetEase(_easeType));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _animSequence?.Kill();
            _animSequence = DOTween.Sequence()
                .Append(transform.DOScale(_originalScale, _animationDuration)
                                 .SetEase(_easeType));
        }
    }
}
