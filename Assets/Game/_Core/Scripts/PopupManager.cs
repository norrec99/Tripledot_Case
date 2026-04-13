using System.Collections;
using DG.Tweening;
using TripledotCase.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.Popup
{
    /// <summary>
    /// Central manager for all popups.
    /// Isolates layout (dimming, depth sorting) from the popups themselves.
    /// Responds to global events to instantiate requested screen overlays.
    ///
    /// Blur Setup:
    ///   1. Add a RawImage with UIBlurMat assigned.
    ///   2. Drag it into the _blurOverlay slot.
    ///   We capture a screenshot, blit it into a MIPMAPPED RenderTexture, then call
    ///   GenerateMips(). The blur shader samples a low mip level (LOD) via tex2Dlod —
    ///   fractional LOD values trilinearly blend between mips for perfectly smooth blur.
    /// </summary>
    public class PopupManager : MonoBehaviour
    {
        [Header("Visual Interpolation")]
        [Tooltip("The fullscreen overlay that dims and blurs the game behind the popup.")]
        [SerializeField] private CanvasGroup _dimmedBackground;
        [Range(0f, 1f)][SerializeField] private float _maxDimAlpha = 0.6f;
        [SerializeField] private float _backgroundFadeDuration = 0.4f;

        [Header("Blur Overlay")]
        [Tooltip("A full-screen RawImage inside DimmedBackground with UIBlurMat assigned.")]
        [SerializeField] private RawImage _blurOverlay;

        [Header("Popup Spawning")]
        [Tooltip("Parent transform where newly instantiated popups will live.")]
        [SerializeField] private Transform _popupContainer;

        [Header("Registered Popups")]
        [SerializeField] private SettingsPopup _settingsPopupPrefab;

        private PopupBase _activePopupInstance;
        private Texture2D _screenshotTexture;
        private RenderTexture _mippedRT;      // Mipmapped RT — blur strength controlled by LOD level in shader
        private CanvasGroup _blurCanvasGroup;

        private void Awake()
        {
            EventManager.OnSettingsClicked += HandleSettingsClicked;
            EventManager.OnPopupClosed += HandlePopupClosed;
            EventManager.OnBlurToggled += HandleBlurToggled;

            _dimmedBackground.gameObject.SetActive(false);
            _dimmedBackground.alpha = 0f;

            if (_blurOverlay != null)
            {
                _blurCanvasGroup = _blurOverlay.GetComponent<CanvasGroup>()
                                ?? _blurOverlay.gameObject.AddComponent<CanvasGroup>();
                _blurCanvasGroup.alpha = 0f;
                _blurOverlay.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            EventManager.OnSettingsClicked -= HandleSettingsClicked;
            EventManager.OnPopupClosed -= HandlePopupClosed;
            EventManager.OnBlurToggled -= HandleBlurToggled;
            Cleanup();
        }

        // ── Signal Handlers ────────────────────────────────────────────────────────

        private void HandleSettingsClicked()
        {
            if (_activePopupInstance != null) return;
            StartCoroutine(CaptureAndShowPopup());
        }

        private void HandlePopupClosed()
        {
            HideDimmedBackground();

            if (_activePopupInstance != null)
            {
                Destroy(_activePopupInstance.gameObject, 0.5f);
                _activePopupInstance = null;
            }
        }

        private void HandleBlurToggled()
        {
            if (_blurOverlay == null || _blurCanvasGroup == null) return;

            // Toggle: if currently visible fade out, if hidden fade in
            float targetAlpha = _blurCanvasGroup.alpha > 0.5f ? 0f : 1f;
            _blurCanvasGroup.DOFade(targetAlpha, _backgroundFadeDuration).SetEase(Ease.OutQuad);
        }

        // ── Mipmap Blur Capture ────────────────────────────────────────────────────

        private IEnumerator CaptureAndShowPopup()
        {
            yield return new WaitForEndOfFrame();

            Cleanup();

            // 1. Capture the full composited frame
            _screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();

            // 2. Create a mipmapped RenderTexture at the same resolution.
            //    autoGenerateMips = false so we control exactly when mips are built.
            //    FilterMode.Trilinear makes fractional LOD values blend smoothly
            //    between adjacent mip levels (this is what eliminates pixelation!).
            _mippedRT = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32)
            {
                useMipMap = true,
                autoGenerateMips = false,
                filterMode = FilterMode.Trilinear,
            };
            _mippedRT.Create();

            // 3. Copy the screenshot into mip 0 of the RT
            Graphics.Blit(_screenshotTexture, _mippedRT);

            // 4. GPU builds all mip levels (each level = bilinear avg of the level above)
            _mippedRT.GenerateMips();

            // Immediately free the source Texture2D — it has served its purpose.
            // This drops peak GPU memory from ~22 MB → ~12.8 MB while the popup is open.
            Destroy(_screenshotTexture);
            _screenshotTexture = null;

            // 5. Hand the mipmapped RT to the blur shader.
            //    The shader's _MipLevel property picks which LOD to sample.
            //    Fractional values (e.g. 3.5) trilinearly blend between mip 3 and 4.
            if (_blurOverlay != null)
            {
                _blurOverlay.texture = _mippedRT;
                _blurCanvasGroup.alpha = 0f;
            }

            ShowDimmedBackground();

            _activePopupInstance = Instantiate(_settingsPopupPrefab, _popupContainer);
            _activePopupInstance.Show(onShown: null);
        }

        // ── Background Control ─────────────────────────────────────────────────────

        private void ShowDimmedBackground()
        {
            _blurOverlay.gameObject.SetActive(true);
            _blurCanvasGroup.DOFade(1f, _backgroundFadeDuration / 2).SetEase(Ease.OutQuad);
            _dimmedBackground.gameObject.SetActive(true);
            _dimmedBackground.DOFade(_maxDimAlpha, _backgroundFadeDuration).SetEase(Ease.OutQuad);
        }

        private void HideDimmedBackground()
        {
            // Fade both layers out in parallel — same duration so they disappear together
            _dimmedBackground.DOFade(0f, _backgroundFadeDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => _dimmedBackground.gameObject.SetActive(false));

            _blurCanvasGroup.DOFade(0f, _backgroundFadeDuration / 2).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _blurOverlay.gameObject.SetActive(false);
                    Cleanup();
                });
        }

        private void Cleanup()
        {
            if (_screenshotTexture != null) { Destroy(_screenshotTexture); _screenshotTexture = null; }
            if (_mippedRT != null) { _mippedRT.Release(); _mippedRT = null; }
        }
    }
}
