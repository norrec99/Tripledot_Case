using DG.Tweening;
using TripledotCase.UI.Core;
using UnityEngine;

namespace TripledotCase.UI.Popup
{
    /// <summary>
    /// Central manager for all popups. 
    /// Isolates layout (dimming, depth sorting) from the popups themselves.
    /// Responds to global events to instantiate requested screen overlays.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        [Header("Visual Interpolation")]
        [Tooltip("The fullscreen black canvas heavily faded to darken the game behind the popup.")]
        [SerializeField] private CanvasGroup _dimmedBackground;
        [Range(0f, 1f)] [SerializeField] private float _maxDimAlpha = 0.6f;
        [SerializeField] private float _backgroundFadeDuration = 0.4f;

        [Header("Popup Spawning")]
        [Tooltip("Parent transform where newly instantiated popups will live (ensures proper UI depth).")]
        [SerializeField] private Transform _popupContainer;

        [Header("Registered Popups")]
        [SerializeField] private SettingsPopup _settingsPopupPrefab;

        // Tracks the actively open popup to prevent double-spawning
        private PopupBase _activePopupInstance;

        private void Awake()
        {
            // Subscribe to cross-system signals to stay perfectly decoupled
            EventManager.OnSettingsClicked += HandleSettingsClicked;
            EventManager.OnPopupClosed     += HandlePopupClosed;

            // Ensure background is fully hidden on boot
            _dimmedBackground.gameObject.SetActive(false);
            _dimmedBackground.alpha = 0f;
        }

        private void OnDestroy()
        {
            EventManager.OnSettingsClicked -= HandleSettingsClicked;
            EventManager.OnPopupClosed     -= HandlePopupClosed;
        }

        // ── Signal Handlers ────────────────────────────────────────────────────────

        private void HandleSettingsClicked()
        {
            if (_activePopupInstance != null) return; // Prevent spam-opening

            ShowDimmedBackground();

            // Instantiate dynamically to keep memory clean, rather than hiding permanently
            _activePopupInstance = Instantiate(_settingsPopupPrefab, _popupContainer);
            _activePopupInstance.Show(onShown: null);
        }

        private void HandlePopupClosed()
        {
            HideDimmedBackground();

            // The popup handles its own bounce-out animation.
            // We give it 0.5s to finish its DOTween sequence before purging it from memory.
            if (_activePopupInstance != null)
            {
                Destroy(_activePopupInstance.gameObject, 0.5f);
                _activePopupInstance = null;
            }
        }

        // ── Background Control ─────────────────────────────────────────────────────

        private void ShowDimmedBackground()
        {
            _dimmedBackground.gameObject.SetActive(true);
            _dimmedBackground.DOFade(_maxDimAlpha, _backgroundFadeDuration).SetEase(Ease.OutQuad);
        }

        private void HideDimmedBackground()
        {
            _dimmedBackground.DOFade(0f, _backgroundFadeDuration).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                _dimmedBackground.gameObject.SetActive(false);
            });
        }
    }
}
