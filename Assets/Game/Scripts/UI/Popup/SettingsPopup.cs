using TripledotCase.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.Popup
{
    /// <summary>
    /// Controls the specific logic inside the Settings Popup.
    /// Inherits universal bounce animations from PopupBase.
    /// </summary>
    public class SettingsPopup : PopupBase
    {
        [Header("Settings Rows")]
        [SerializeField] private SettingsRowView _soundRow;
        [SerializeField] private SettingsRowView _musicRow;
        [SerializeField] private SettingsRowView _vibrationRow;
        [SerializeField] private SettingsRowView _notificationsRow;
        [SerializeField] private SettingsRowView _languageRow;

        [Header("Settings Actions")]
        [SerializeField] private Button _privacyButton;
        [SerializeField] private Button _termsButton;
        [SerializeField] private Button _supportButton;

        [Header("Close Setup")]
        [SerializeField] private Button _closeButton;

        protected override void Awake()
        {
            base.Awake(); // Grabs CanvasGroup

            if (_closeButton != null)
                _closeButton.onClick.AddListener(RequestClose); // Calls the base RequestClose

            // Hook up logic listeners
            if (_soundRow != null && _soundRow.RowToggle != null) _soundRow.RowToggle.onValueChanged.AddListener(OnSoundToggled);
            if (_musicRow != null && _musicRow.RowToggle != null) _musicRow.RowToggle.onValueChanged.AddListener(OnMusicToggled);
            if (_vibrationRow != null && _vibrationRow.RowToggle != null) _vibrationRow.RowToggle.onValueChanged.AddListener(OnVibrationToggled);
            if (_notificationsRow != null && _notificationsRow.RowToggle != null) _notificationsRow.RowToggle.onValueChanged.AddListener(OnNotificationsToggled);
            if (_languageRow != null && _languageRow.ActionButton != null) _languageRow.ActionButton.onClick.AddListener(OnLanguageClicked);

            // Global Subscriptions
            LocalizationManager.OnLanguageChanged += SyncLanguageRowIcon;

            if (_privacyButton != null) _privacyButton.onClick.AddListener(OnPrivacyClicked);
            if (_termsButton != null) _termsButton.onClick.AddListener(OnTermsClicked);
            if (_supportButton != null) _supportButton.onClick.AddListener(OnSupportClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_closeButton != null) _closeButton.onClick.RemoveListener(RequestClose);

            if (_soundRow != null && _soundRow.RowToggle != null) _soundRow.RowToggle.onValueChanged.RemoveListener(OnSoundToggled);
            if (_musicRow != null && _musicRow.RowToggle != null) _musicRow.RowToggle.onValueChanged.RemoveListener(OnMusicToggled);
            if (_vibrationRow != null && _vibrationRow.RowToggle != null) _vibrationRow.RowToggle.onValueChanged.RemoveListener(OnVibrationToggled);
            if (_notificationsRow != null && _notificationsRow.RowToggle != null) _notificationsRow.RowToggle.onValueChanged.RemoveListener(OnNotificationsToggled);
            if (_languageRow != null && _languageRow.ActionButton != null) _languageRow.ActionButton.onClick.RemoveListener(OnLanguageClicked);

            LocalizationManager.OnLanguageChanged -= SyncLanguageRowIcon;

            if (_privacyButton != null) _privacyButton.onClick.RemoveListener(OnPrivacyClicked);
            if (_termsButton != null) _termsButton.onClick.RemoveListener(OnTermsClicked);
            if (_supportButton != null) _supportButton.onClick.RemoveListener(OnSupportClicked);
        }

        public override void Show(System.Action onShown = null)
        {
            // Sync UI to actual saved settings when requested to show
            SyncStateFromData();
            base.Show(onShown);
        }

        private void SyncStateFromData()
        {
            // In a real app, you would load from PlayerPrefs or a SaveData model here.
            Debug.Log("[SettingsPopup] Synced UI bounds to current player preferences.");
        }

        private void Start()
        {
            // By running this in Start(), we guarantee it executes AFTER SettingsRowView.Start()
            // has finished configuring its default serialized sprite!
            SyncLanguageRowIcon();
        }

        private void SyncLanguageRowIcon()
        {
            if (_languageRow != null && LocalizationManager.Instance != null && LocalizationManager.Instance.ActiveLanguage != null)
            {
                _languageRow.SetIcon(LocalizationManager.Instance.ActiveLanguage.LanguageIcon);
            }
        }

        // ── Interaction Handlers ─────────────────────────────────────────────

        private void OnSoundToggled(bool isOn) => Debug.Log($"[SettingsPopup] Sound toggled: {isOn}");
        private void OnMusicToggled(bool isOn) => Debug.Log($"[SettingsPopup] Music toggled: {isOn}");
        private void OnVibrationToggled(bool isOn) => Debug.Log($"[SettingsPopup] Vibration toggled: {isOn}");
        private void OnNotificationsToggled(bool isOn) => Debug.Log($"[SettingsPopup] Notifications toggled: {isOn}");

        private void OnLanguageClicked()
        {
            Debug.Log("[SettingsPopup] Language row clicked. Swapping active language...");
            LocalizationManager.Instance?.CycleNextLanguage();
        }

        private void OnPrivacyClicked() => Debug.Log("[SettingsPopup] Privacy Policy opened.");
        private void OnTermsClicked() => Debug.Log("[SettingsPopup] Terms & Conditions opened.");
        private void OnSupportClicked() => Debug.Log("[SettingsPopup] Support center opened.");
    }
}
