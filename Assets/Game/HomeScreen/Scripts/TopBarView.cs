using TMPro;
using TripledotCase.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.HomeScreen
{
    /// <summary>
    /// Manages the top currency bar UI:
    ///
    ///   [ 🪙 2,850 ]   [ ❤️ 5  Full ]   [ ⭐ 165 ]   [ ⚙️ ]
    ///
    /// Each currency widget is a white pill-shaped group containing:
    ///   • A circular icon image
    ///   • A bold numeric value label
    ///   • (Coins only) a "+" add-currency button
    ///   • (Hearts only) a status text label (e.g. "Full")
    ///
    /// Call SetValues() to update the displayed figures from any data source.
    /// The Settings button dispatches via EventManager so no direct coupling
    /// to higher-level systems is needed.
    /// </summary>
    public class TopBarView : MonoBehaviour
    {
        // ── Coins Widget ───────────────────────────────────────────────────────────

        [Header("Coins Widget")]
        [SerializeField] private TextMeshProUGUI _coinsText;

        /// <summary>
        /// The "+" button next to the coin count.
        /// Wired in Inspector; notifies settings or a store popup in future.
        /// </summary>
        [SerializeField] private Button _coinsAddButton;

        // ── Hearts Widget ──────────────────────────────────────────────────────────

        [Header("Hearts Widget")]
        [SerializeField] private TextMeshProUGUI _heartsCountText;

        /// <summary>
        /// Shows "Full" when the heart meter is at capacity, otherwise empty.
        /// </summary>
        [SerializeField] private TextMeshProUGUI _heartsStatusText;

        // ── Stars Widget ───────────────────────────────────────────────────────────

        [Header("Stars Widget")]
        [SerializeField] private TextMeshProUGUI _starsText;

        // ── Settings Button ────────────────────────────────────────────────────────

        [Header("Settings")]
        [SerializeField] private Button _settingsButton;

        // ── Lifecycle ──────────────────────────────────────────────────────────────

        private void Awake()
        {
            _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_coinsAddButton != null)
                _coinsAddButton.onClick.AddListener(OnCoinsAddClicked);
        }

        private void OnDestroy()
        {
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (_coinsAddButton != null)
                _coinsAddButton.onClick.RemoveListener(OnCoinsAddClicked);
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Updates all currency display widgets.
        /// </summary>
        /// <param name="coins">Coin count to display.</param>
        /// <param name="hearts">Heart count to display.</param>
        /// <param name="stars">Star count to display.</param>
        /// <param name="heartsFull">When true, shows "Full" status next to hearts.</param>
        public void SetValues(int coins, int hearts, int stars, bool heartsFull)
        {
            // "N0" formats with thousands separator: 2850 → "2,850"
            _coinsText.text = coins.ToString("N0");
            _heartsCountText.text = hearts.ToString();
            _starsText.text = stars.ToString("N0");

            _heartsStatusText.gameObject.SetActive(heartsFull);
        }

        // ── Private Handlers ───────────────────────────────────────────────────────

        private void OnSettingsClicked() => EventManager.FireSettingsClicked();

        private void OnCoinsAddClicked()
        {
            // Placeholder — in a real game this would open the store or a purchase flow
            Debug.Log("[TopBarView] Add Coins tapped");
        }
    }
}
