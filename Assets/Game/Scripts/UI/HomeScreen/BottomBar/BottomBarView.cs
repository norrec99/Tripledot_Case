using System.Collections.Generic;
using DG.Tweening;
using TripledotCase.UI.Core;
using UnityEngine;

namespace TripledotCase.UI.HomeScreen.BottomBar
{
    /// <summary>
    /// Orchestrates the five BottomBarButtonViews and owns the bar's
    /// appear / disappear animations.
    ///
    /// Selection contract:
    ///   • Tapping an AVAILABLE (idle) button  → deactivates any current active button
    ///                                           → activates the tapped button
    ///                                           → fires EventManager.FireContentActivated(index)
    ///   • Tapping the ACTIVE button again     → deactivates it
    ///                                           → fires EventManager.FireBarClosed()
    ///   • Tapping a LOCKED button             → delegates to PlayLockedFeedback(), no state change
    ///
    /// Call Appear() / Disappear() externally (e.g. from HomeScreenView.Start)
    /// to slide the bar in or out of view.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class BottomBarView : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────────────

        [Header("Buttons")]
        [Tooltip("Assign all five BottomBarButtonView components in order (left to right).")]
        [SerializeField] private List<BottomBarButtonView> _buttons;

        [Header("Appear / Disappear")]
        [Tooltip("Y offset applied when the bar is hidden (negative = below screen).")]
        [SerializeField] private float _hiddenOffsetY = -220f;
        [SerializeField] private float _appearDuration = 0.40f;
        [SerializeField] private float _disappearDuration = 0.35f;

        // ── Private ────────────────────────────────────────────────────────────────

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private BottomBarButtonView _activeButton;
        private float _shownPositionY;
        private Sequence _barSequence;

        // ── Lifecycle ──────────────────────────────────────────────────────────────

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            // Record the designed resting position before any animation shifts it
            _shownPositionY = _rectTransform.anchoredPosition.y;

            foreach (var button in _buttons)
                button.OnButtonClicked += HandleButtonClicked;

            // Start hidden so Appear() can animate it in
            SetBarPositionImmediate(hidden: true);
        }

        private void OnDestroy()
        {
            foreach (var button in _buttons)
                button.OnButtonClicked -= HandleButtonClicked;

            _barSequence?.Kill();
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Slides the bar up from below the screen into its resting position.
        /// Typically called once from HomeScreenView.Start().
        /// </summary>
        public void Appear()
        {
            _barSequence?.Kill();

            float hiddenY = _shownPositionY + _hiddenOffsetY;
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, hiddenY);
            _canvasGroup.alpha = 0f;

            _barSequence = DOTween.Sequence()
                .Join(_rectTransform.DOAnchorPosY(_shownPositionY, _appearDuration)
                                    .SetEase(Ease.OutBack))
                .Join(_canvasGroup.DOFade(1f, _appearDuration * 0.75f)
                                  .SetEase(Ease.OutQuad));
        }

        /// <summary>
        /// Slides the bar back down below the screen and fades it out.
        /// </summary>
        public void Disappear()
        {
            _barSequence?.Kill();

            float hiddenY = _shownPositionY + _hiddenOffsetY;

            _barSequence = DOTween.Sequence()
                .Join(_rectTransform.DOAnchorPosY(hiddenY, _disappearDuration)
                                    .SetEase(Ease.InBack))
                .Join(_canvasGroup.DOFade(0f, _disappearDuration * 0.7f)
                                  .SetEase(Ease.InQuad));
        }

        // ── Private ────────────────────────────────────────────────────────────────

        private void HandleButtonClicked(BottomBarButtonView clicked)
        {
            // Locked button → tactile feedback only, no state change
            if (clicked.IsLocked)
            {
                clicked.PlayLockedFeedback();
                return;
            }

            // Active button tapped again → toggle off
            if (clicked.IsActive)
            {
                clicked.Deactivate();
                _activeButton = null;
                EventManager.FireBarClosed();
                return;
            }

            // Idle button tapped → switch active button
            // Deactivate the previous active button (if any) in parallel
            _activeButton?.Deactivate();
            _activeButton = clicked;
            clicked.Activate();
            EventManager.FireContentActivated(clicked.ButtonIndex);
        }

        private void SetBarPositionImmediate(bool hidden)
        {
            float y = hidden ? _shownPositionY + _hiddenOffsetY : _shownPositionY;
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, y);
            _canvasGroup.alpha = hidden ? 0f : 1f;
        }
    }
}
