using System;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// Centralized static event bus for all Home Screen UI events.
    ///
    /// Usage:
    ///   Subscribe  → EventManager.OnContentActivated += MyHandler;
    ///   Dispatch   → EventManager.FireContentActivated(index);
    ///   Cleanup    → EventManager.ClearAllListeners(); (call on scene unload)
    ///
    /// Keeping events here decouples views from one another — no direct
    /// cross-component references are required.
    /// </summary>
    public static class EventManager
    {
        // ── Bottom Bar ────────────────────────────────────────────────────────────

        /// <summary>
        /// Fired when a bottom bar button is toggled ON.
        /// Parameter: zero-based button index.
        /// </summary>
        public static event Action<int> OnContentActivated;

        /// <summary>
        /// Fired when all bottom bar buttons are toggled OFF
        /// (user tapped the active button a second time).
        /// </summary>
        public static event Action OnBarClosed;

        // ── Top Bar ───────────────────────────────────────────────────────────────

        /// <summary>Fired when the Settings gear button is tapped.</summary>
        public static event Action OnSettingsClicked;

        // ── Dispatchers ───────────────────────────────────────────────────────────

        /// <param name="buttonIndex">Zero-based index of the activated button.</param>
        public static void FireContentActivated(int buttonIndex) =>
            OnContentActivated?.Invoke(buttonIndex);

        public static void FireBarClosed() =>
            OnBarClosed?.Invoke();

        public static void FireSettingsClicked() =>
            OnSettingsClicked?.Invoke();

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Clears all subscribed listeners.
        /// Call this when the HomeScreen scene is unloaded to prevent stale references.
        /// </summary>
        public static void ClearAllListeners()
        {
            OnContentActivated = null;
            OnBarClosed        = null;
            OnSettingsClicked  = null;
        }
    }
}
