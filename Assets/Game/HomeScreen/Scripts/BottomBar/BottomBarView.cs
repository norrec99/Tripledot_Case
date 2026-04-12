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
            // First, identify who the active button is
            foreach (var btn in _buttons)
            {
                if (btn.IsActive)
                {
                    _activeButton = btn;
                    break;
                }
            }
            
            // Instantly apply structural sizes (300/195) so the Layout engine has exactly what it needs
            DistributeWidths(snapImmediate: true);

            // Finally, place the indicator squarely underneath the natively finalized layout coordinate
            if (_activeButton != null)
                SnapIndicatorTo(_activeButton);
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
                DistributeWidths(); // Revert to uncompressed idle state natively
                return;
            }

            // Switch: deactivate previous, slide indicator, activate new
            _activeButton?.Deactivate();
            _activeButton = clicked;
            clicked.Activate();
            SlideIndicatorTo(clicked);
            DistributeWidths(); // Re-distribute compression based on newly activated target
            EventManager.FireContentActivated(clicked.ButtonIndex);
        }

        private void HandleLockedButtonClicked(BottomBarButtonView clicked)
        {
            clicked.PlayLockedFeedback();
        }

        // ── Private: Indicator & Width Animation ──────────────────────────────────

        /// <summary>
        /// Mathematically computes proportional space-stealing so that when one button Inflates by X,
        /// all adjacent siblings collectively deflate by (-X) to ensure the total parent wrapper width never shifts!
        /// </summary>
        private void DistributeWidths(bool snapImmediate = false)
        {
            if (_buttons == null || _buttons.Count == 0) return;

            float compressionPerInactive = 0f;

            // If a button is active, compute exactly how much space it's stealing from the layout
            if (_activeButton != null && _buttons.Count > 1)
            {
                float extraWidth = _activeButton.ActiveWidth - _activeButton.NativeWidth;
                compressionPerInactive = extraWidth / (_buttons.Count - 1);
            }

            foreach (var btn in _buttons)
            {
                float targetWidth = btn.NativeWidth;

                if (_activeButton != null)
                {
                    if (btn == _activeButton) targetWidth = btn.ActiveWidth;
                    else targetWidth = btn.NativeWidth - compressionPerInactive;
                }

                // Command the button to smoothly ease into its new compressed/inflated bounds!
                btn.AnimateWidthTo(targetWidth, snapImmediate ? 0f : -1f);
            }
        }

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

            Canvas.ForceUpdateCanvases();

            // Apply dynamic width (Target's massively inflated width), but keep the custom Designer height (e.g. 270)
            _activeIndicator.sizeDelta = new Vector2(target.ActiveWidth, _indicatorHeight);

            // Snap only the X axis to match the button; leave Y exactly where it was in the Inspector
            _activeIndicator.position = new Vector3(target.transform.position.x, _activeIndicator.position.y, _activeIndicator.position.z);

            _activeIndicator.localScale = Vector3.one;
            _activeIndicator.gameObject.SetActive(true);
        }

        /// <summary>Pops the indicator up from scale 0 while mathematically tracking the layout shift beneath it.</summary>
        private void PopInIndicatorOn(BottomBarButtonView target)
        {
            _indicatorSequence?.Kill();

            _activeIndicator.sizeDelta = new Vector2(target.ActiveWidth, _indicatorHeight);
            _activeIndicator.localScale = Vector3.zero;
            _activeIndicator.gameObject.SetActive(true);

            _indicatorSequence = DOTween.Sequence()
                // Pop the scale up to 1.0!
                .Join(_activeIndicator.DOScale(1f, _popInDuration).SetEase(Ease.OutBack))
                // Heat-seek the target while the unity layout grows natively beneath us in the exact same timeframe!
                .Join(DOVirtual.Float(0f, 1f, _popInDuration, v =>
                {
                    if (target != null)
                        _activeIndicator.position = new Vector3(target.transform.position.x, _activeIndicator.position.y, _activeIndicator.position.z);
                }));
        }

        /// <summary>Slides the indicator to the target button's position.
        /// If the indicator is hidden, it pops in at the target instead of sliding.</summary>
        private void SlideIndicatorTo(BottomBarButtonView target)
        {
            _indicatorSequence?.Kill();

            if (!_activeIndicator.gameObject.activeSelf)
            {
                // Not yet visible — track and pop rather than rigidly evaluating from off-screen
                PopInIndicatorOn(target);
                return;
            }

            // Capture our starting position for the slide interpolation
            Vector3 startPos = _activeIndicator.position;

            _indicatorSequence = DOTween.Sequence()
                // Heat-seeking logic: Instead of a static DOMove, we interpolate to the target's CURRENT position every single frame.
                .Join(DOVirtual.Float(0f, 1f, _slideDuration, v =>
                {
                    if (target != null)
                    {
                        float dynamicX = Mathf.LerpUnclamped(startPos.x, target.transform.position.x, v);
                        _activeIndicator.position = new Vector3(dynamicX, _activeIndicator.position.y, _activeIndicator.position.z);
                    }
                }).SetEase(_slideEase))
                .Join(_activeIndicator.DOSizeDelta(new Vector2(target.ActiveWidth, _indicatorHeight), _slideDuration)
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
