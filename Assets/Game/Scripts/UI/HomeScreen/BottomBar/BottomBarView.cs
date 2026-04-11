using System.Collections.Generic;
using DG.Tweening;
using TripledotCase.UI.Core;
using UnityEngine;

namespace TripledotCase.UI.HomeScreen.BottomBar
{
    /// <summary>
    /// Orchestrates all navigation area buttons and owns the single shared
    /// active-state indicator (the coloured card that slides between buttons).
    ///
    /// Indicator behaviour:
    ///   First activation  → snaps to button position, pops in with punch scale
    ///   Switching buttons → slides from current to new position (DOMove)
    ///   Deactivation      → shrinks and disables
    ///
    /// Each BottomBarButtonView handles only its own icon and label animations.
    /// </summary>
    public class BottomBarView : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────────────

        [Header("Buttons (left → right)")]
        [SerializeField] private List<BottomBarButtonView> _buttons;

        [Header("Active Indicator")]
        [Tooltip("The single shared card image that slides between active buttons.")]
        [SerializeField] private RectTransform _activeIndicator;

        [Header("Indicator Animation")]
        [SerializeField] private float _slideDuration = 0.35f;
        [SerializeField] private Ease _slideEase = Ease.OutCubic;
        [SerializeField] private float _popInDuration = 0.30f;
        [SerializeField] private float _hideOutDuration = 0.20f;

        [Header("Bar Appear / Disappear")]
        [Tooltip("Y offset when the bar is out of view (negative = below screen).")]
        [SerializeField] private float _hiddenOffsetY = -260f;
        [SerializeField] private float _appearDuration = 0.50f;
        [SerializeField] private float _disappearDuration = 0.35f;

        // ── State ──────────────────────────────────────────────────────────────────

        private BottomBarButtonView _activeButton;
        private Sequence _indicatorSequence;
        private Sequence _barSequence;
        private RectTransform _rectTransform;
        private float _shownPositionY;
        private float _indicatorHeight;

        // ── Lifecycle ──────────────────────────────────────────────────────────────

        private void Awake()
        {
            foreach (var btn in _buttons)
            {
                btn.OnButtonClicked += HandleButtonClicked;
                btn.OnLockedButtonClicked += HandleLockedButtonClicked;
            }

            // Capture the height from the Inspector (e.g., 270) before we manipulate the indicator
            _indicatorHeight = _activeIndicator.rect.height;

            // Start indicator hidden — Start() will position it if a button is initially active
            _activeIndicator.gameObject.SetActive(false);

            _rectTransform = GetComponent<RectTransform>();
            _shownPositionY = _rectTransform.anchoredPosition.y;
            SetBarImmediate(hidden: false);
        }

        // Start() is guaranteed to run after ALL Awake() calls — safe to read initial button states
        private void Start()
        {
            foreach (var btn in _buttons)
            {
                if (btn.IsActive)
                {
                    _activeButton = btn;
                    SnapIndicatorTo(btn);
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var btn in _buttons)
            {
                btn.OnButtonClicked -= HandleButtonClicked;
                btn.OnLockedButtonClicked -= HandleLockedButtonClicked;
            }
            _indicatorSequence?.Kill();
            _barSequence?.Kill();
        }



        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Slides the bar up from below into its designed position.</summary>
        [ContextMenu("Test Appear")]
        public void Appear()
        {
            _barSequence?.Kill();

            _rectTransform.anchoredPosition = new Vector2(
                _rectTransform.anchoredPosition.x,
                _shownPositionY + _hiddenOffsetY);

            _barSequence = DOTween.Sequence()
                .Join(_rectTransform.DOAnchorPosY(_shownPositionY, _appearDuration)
                                    .SetEase(Ease.OutBack));
        }

        /// <summary>Slides the bar back below the screen.</summary>
        [ContextMenu("Test Disappear")]
        public void Disappear()
        {
            _barSequence?.Kill();

            _barSequence = DOTween.Sequence()
                .Join(_rectTransform.DOAnchorPosY(_shownPositionY + _hiddenOffsetY, _disappearDuration)
                                    .SetEase(Ease.InBack));

            _barSequence.OnComplete(() => EventManager.FireBarClosed());
        }

        // ── Private: Click Routing ─────────────────────────────────────────────────

        private void HandleButtonClicked(BottomBarButtonView clicked)
        {
            // Already active — toggle it OFF!
            if (clicked.IsActive) 
            {
                clicked.Deactivate();
                _activeButton = null;
                HideIndicator();
                return;
            }

            // Switch: deactivate previous, slide indicator, activate new
            _activeButton?.Deactivate();
            _activeButton = clicked;
            clicked.Activate();
            SlideIndicatorTo(clicked);
            EventManager.FireContentActivated(clicked.ButtonIndex);
        }

        private void HandleLockedButtonClicked(BottomBarButtonView clicked)
        {
            clicked.PlayLockedFeedback();
        }

        // ── Private: Indicator Animation ──────────────────────────────────────────
        
        /// <summary>Shrinks and hides the indicator entirely (used when a button is toggled off).</summary>
        private void HideIndicator()
        {
            _indicatorSequence?.Kill();
            _indicatorSequence = DOTween.Sequence()
                .Append(_activeIndicator.DOScale(0f, _hideOutDuration).SetEase(Ease.InBack))
                .OnComplete(() => _activeIndicator.gameObject.SetActive(false));
        }

        /// <summary>Instantly positions the indicator with no animation (used on scene load).</summary>
        private void SnapIndicatorTo(BottomBarButtonView target)
        {
            _indicatorSequence?.Kill();

            // Force layout groups to calculate their sizes so we can read the correct width
            Canvas.ForceUpdateCanvases();
            var targetRect = target.GetComponent<RectTransform>();

            // Apply dynamic width, but keep the custom Designer height (e.g. 270)
            _activeIndicator.sizeDelta = new Vector2(targetRect.rect.width, _indicatorHeight);

            // Snap only the X axis to match the button; leave Y exactly where it was in the Inspector
            _activeIndicator.position = new Vector3(target.transform.position.x, _activeIndicator.position.y, _activeIndicator.position.z);

            _activeIndicator.localScale = Vector3.one;
            _activeIndicator.gameObject.SetActive(true);
        }

        /// <summary>Slides the indicator to the target button's position.
        /// If the indicator is hidden, it pops in at the target instead of sliding.</summary>
        private void SlideIndicatorTo(BottomBarButtonView target)
        {
            _indicatorSequence?.Kill();

            if (!_activeIndicator.gameObject.activeSelf)
            {
                // Not yet visible — snap and pop rather than slide from off-screen
                SnapIndicatorTo(target);
                return;
            }

            var targetRect = target.GetComponent<RectTransform>();

            _indicatorSequence = DOTween.Sequence()
                .Join(_activeIndicator.DOMoveX(target.transform.position.x, _slideDuration)
                                      .SetEase(_slideEase))
                .Join(_activeIndicator.DOSizeDelta(new Vector2(targetRect.rect.width, _indicatorHeight), _slideDuration)
                                      .SetEase(_slideEase))
                .Join(_activeIndicator.DOScale(1f, _slideDuration) // Failsafe recovery if interrupted while hiding!
                                      .SetEase(_slideEase));
        }

        private void SetBarImmediate(bool hidden)
        {
            float y = hidden ? _shownPositionY + _hiddenOffsetY : _shownPositionY;
            _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, y);
        }
    }
}
