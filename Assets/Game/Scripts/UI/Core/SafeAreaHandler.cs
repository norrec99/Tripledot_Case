using UnityEngine;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// Adjusts the attached RectTransform's anchors to match the device's safe area,
    /// accounting for notches, status bars, and home indicators on iOS and Android.
    ///
    /// Attach this to the root "SafeAreaPanel" that lives directly inside your Canvas.
    /// All top-bar and bottom-bar content should be children of that panel — not of
    /// the Canvas itself — so insets are automatically enforced everywhere.
    ///
    /// Re-evaluates every frame so rotation and split-screen changes are handled
    /// without requiring a scene reload.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect          _lastSafeArea;
        private Vector2       _lastScreenSize;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        // Check every frame — cheap and covers all edge cases
        // (rotation, multi-window, emulator safe-area simulation, etc.)
        private void Update()
        {
            var currentSize = new Vector2(Screen.width, Screen.height);

            if (_lastSafeArea != Screen.safeArea || _lastScreenSize != currentSize)
                ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            var safeArea  = Screen.safeArea;
            var screenSize = new Vector2(Screen.width, Screen.height);

            _lastSafeArea   = safeArea;
            _lastScreenSize = screenSize;

            // Guard against zero-size screen (can happen during startup on some devices)
            if (screenSize.x == 0f || screenSize.y == 0f)
                return;

            // Convert pixel safe-area to normalized (0–1) anchor coordinates
            var anchorMin = new Vector2(safeArea.xMin / screenSize.x,
                                        safeArea.yMin / screenSize.y);
            var anchorMax = new Vector2(safeArea.xMax / screenSize.x,
                                        safeArea.yMax / screenSize.y);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // Zero out offsets so the panel exactly matches the anchor rectangle
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
    }
}
