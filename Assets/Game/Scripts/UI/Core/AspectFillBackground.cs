using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// Scales a background Image to always fill the screen without distortion,
    /// regardless of the device's aspect ratio (iPhone, iPad, etc.).
    ///
    /// Behaves like CSS "background-size: cover":
    /// the image uniformly scales until its shortest axis fills the screen,
    /// and any overflow on the longer axis is cropped off-screen.
    ///
    /// Usage:
    ///   1. Attach this component to your background Image GameObject.
    ///   2. Assign the sprite to the Image component on the same GameObject.
    ///   3. Leave "Preserve Aspect" UNCHECKED — this script overrides the size.
    ///   Note: Anchors are automatically set to center at runtime. No manual setup needed.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class AspectFillBackground : MonoBehaviour
    {
        private void Start()
        {
            // Ensure the Canvas Scaler has committed its layout before we read parent.rect.
            Canvas.ForceUpdateCanvases();
            Apply();
        }

        /// <summary>Call this at runtime if the screen resolution changes (e.g. split-screen).</summary>
        [ContextMenu("Apply")]
        public void Apply()
        {
            var image = GetComponent<Image>();

            if (image.sprite == null)
            {
                Debug.LogWarning("[AspectFillBackground] No sprite assigned to Image component!", this);
                return;
            }

            var rect = GetComponent<RectTransform>();
            var sprite = image.sprite;

            var parent = rect.parent as RectTransform;
            if (parent == null)
            {
                Debug.LogWarning("[AspectFillBackground] No parent RectTransform found!", this);
                return;
            }

            // Switch to center anchors so sizeDelta represents absolute size.
            // With stretch anchors, sizeDelta means "delta beyond the stretched parent" — not what we want.
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            // Canvas size in canvas units (already accounts for Canvas Scaler)
            float canvasW = parent.rect.width;
            float canvasH = parent.rect.height;
            float screenAspect = canvasW / canvasH;

            // Sprite size in canvas units (pixels / pixelsPerUnit)
            float spriteW = sprite.rect.width / sprite.pixelsPerUnit;
            float spriteH = sprite.rect.height / sprite.pixelsPerUnit;
            float spriteAspect = spriteW / spriteH;

            // Pick the axis that needs to fill completely; the other axis will overflow and crop
            float scale = screenAspect > spriteAspect
                ? canvasW / spriteW   // canvas wider than sprite → fill width
                : canvasH / spriteH;  // canvas taller than sprite → fill height

            rect.sizeDelta = new Vector2(spriteW * scale, spriteH * scale);
        }

#if UNITY_EDITOR
        // Allows previewing the result directly in the Editor without entering Play Mode
        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            Apply();
        }
#endif
    }
}
