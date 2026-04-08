using TripledotCase.UI.Core;
using TripledotCase.UI.HomeScreen.BottomBar;
using UnityEngine;

namespace TripledotCase.UI.HomeScreen
{
    /// <summary>
    /// Root MonoBehaviour for the Home Screen scene.
    ///
    /// Responsibilities:
    ///   • Holds serialized references to all child views (TopBar, BottomBar).
    ///   • Subscribes to EventManager and acts as the central response point
    ///     for UI events (e.g. showing a content panel when a nav button is tapped).
    ///   • Seeds the UI with initial demo data on Awake.
    ///
    /// By keeping this class thin we make it easy to swap out or extend
    /// individual views without touching the HomeScreen entry point.
    /// </summary>
    public class HomeScreenView : MonoBehaviour
    {
        [Header("Child Views")]
        [SerializeField] private TopBarView _topBarView;
        [SerializeField] private BottomBarView _bottomBarView;

        // ── Lifecycle ──────────────────────────────────────────────────────────────

        private void Awake()
        {
            SubscribeToEvents();
            InitializeDemoValues();
        }


        private void OnDestroy()
        {
            // Always unsubscribe on teardown to prevent stale delegate references
            EventManager.ClearAllListeners();
        }

        // ── Initialization ─────────────────────────────────────────────────────────

        private void SubscribeToEvents()
        {
            EventManager.OnContentActivated += OnContentActivated;
            EventManager.OnBarClosed += OnBarClosed;
            EventManager.OnSettingsClicked += OnSettingsClicked;
        }

        private void InitializeDemoValues()
        {
            _topBarView.SetValues(coins: 2850, hearts: 5, stars: 165, heartsFull: true);
        }

        // ── Event Handlers ─────────────────────────────────────────────────────────

        private void OnContentActivated(int buttonIndex)
        {
            Debug.Log($"[HomeScreen] ContentActivated → Button [{buttonIndex}]");
            // TODO: Show content panel for buttonIndex
        }

        private void OnBarClosed()
        {
            Debug.Log("[HomeScreen] BarClosed → all buttons off");
            // TODO: Hide any active content panel
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[HomeScreen] SettingsClicked");
            // TODO: Open settings popup
        }
    }
}
