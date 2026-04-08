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

        // ── State ──────────────────────────────────────────────────────────────────

        private BottomBarButtonView _activeButton;
        private Sequence _indicatorSequence;

        // ── Lifecycle ──────────────────────────────────────────────────────────────

        private void Awake()
        {
            foreach (var btn in _buttons)
            {
                btn.OnButtonClicked += HandleButtonClicked;
                btn.OnLockedButtonClicked += HandleLockedButtonClicked;
            }

            // Start indicator hidden — Start() will position it if a button is initially active
            _activeIndicator.gameObject.SetActive(false);
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
        }

        // ── Private: Click Routing ─────────────────────────────────────────────────

        private void HandleButtonClicked(BottomBarButtonView clicked)
        {
            // Already active — do nothing
            if (clicked.IsActive) return;

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

        /// <summary>Instantly positions the indicator with no animation (used on scene load).</summary>
        private void SnapIndicatorTo(BottomBarButtonView target)
        {
            _indicatorSequence?.Kill();
            _activeIndicator.position = target.transform.position;
            _activeIndicator.localScale = Vector3.zero;
            _activeIndicator.gameObject.SetActive(true);
            _activeIndicator.DOScale(1f, _popInDuration).SetEase(Ease.OutBack);
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

            _indicatorSequence = DOTween.Sequence()
                .Join(_activeIndicator.DOMove(target.transform.position, _slideDuration)
                                      .SetEase(_slideEase));
        }
    }
}
