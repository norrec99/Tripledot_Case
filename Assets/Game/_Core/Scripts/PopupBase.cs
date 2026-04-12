using System;
using DG.Tweening;
using UnityEngine;
using TripledotCase.UI.Core;

namespace TripledotCase.UI.Popup
{
    /// <summary>
    /// Foundational class for all popups.
    /// Handles universal show/hide bounce animations.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour
    {
        [Header("Base Animation")]
        [SerializeField] protected float _animationDuration = 0.35f;

        [Tooltip("The central box that actually bounces in (not the dark background area).")]
        [SerializeField] protected Transform _popupPanel;

        private CanvasGroup _canvasGroup;
        private Sequence _animSequence;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show(Action onShown = null)
        {
            _animSequence?.Kill();

            gameObject.SetActive(true);
            _canvasGroup.alpha = 0f;
            _popupPanel.localScale = Vector3.one * 0.9f;

            _animSequence = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(1f, _animationDuration))
                .Join(_popupPanel.DOScale(1f, _animationDuration).SetEase(Ease.OutBack))
                .OnComplete(() => onShown?.Invoke());
        }

        public virtual void Hide(Action onHidden = null)
        {
            _animSequence?.Kill();

            _animSequence = DOTween.Sequence()
                .Join(_canvasGroup.DOFade(0f, _animationDuration))
                .Join(_popupPanel.DOScale(0.8f, _animationDuration).SetEase(Ease.InBack)) // Shrink slightly to look good
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onHidden?.Invoke();
                });
        }

        /// <summary>
        /// Wired to the 'X' button or generic close buttons in the UI prefab.
        /// </summary>
        public void RequestClose()
        {
            // Triggers PopupManager to fade out the dark background simultaneously
            EventManager.FirePopupClosed();
            Hide();
        }

        protected virtual void OnDestroy()
        {
            _animSequence?.Kill();
        }
    }
}
