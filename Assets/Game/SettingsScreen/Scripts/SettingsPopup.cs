using System;
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
        private const string PrefVibration = "setting_vibration";
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
            if (_soundRow != null) _soundRow.OnSwitchToggled += OnSoundToggled;
            if (_musicRow != null) _musicRow.OnSwitchToggled += OnMusicToggled;
            if (_vibrationRow != null) _vibrationRow.OnSwitchToggled += OnVibrationToggled;
            if (_notificationsRow != null) _notificationsRow.OnSwitchToggled += OnNotificationsToggled;
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

            if (_soundRow != null) _soundRow.OnSwitchToggled -= OnSoundToggled;
            if (_musicRow != null) _musicRow.OnSwitchToggled -= OnMusicToggled;
            if (_vibrationRow != null) _vibrationRow.OnSwitchToggled -= OnVibrationToggled;
            if (_notificationsRow != null) _notificationsRow.OnSwitchToggled -= OnNotificationsToggled;
            if (_languageRow != null && _languageRow.ActionButton != null) _languageRow.ActionButton.onClick.RemoveListener(OnLanguageClicked);

            LocalizationManager.OnLanguageChanged -= SyncLanguageRowIcon;

            if (_privacyButton != null) _privacyButton.onClick.RemoveListener(OnPrivacyClicked);
            if (_termsButton != null) _termsButton.onClick.RemoveListener(OnTermsClicked);
            if (_supportButton != null) _supportButton.onClick.RemoveListener(OnSupportClicked);
        }

        public override void Show(Action onShown = null)
        {
            // Sync UI to actual saved settings when requested to show
            SyncStateFromData();
            base.Show(onShown);
        }

        private void SyncStateFromData()
        {
            // Load persisted vibration state and sync both the Taptic engine and the switch UI
            bool vibrationOn = PlayerPrefs.GetInt(PrefVibration, 1) == 1;
            Taptic.tapticOn = vibrationOn;
            _vibrationRow?.SetSwitchState(vibrationOn, animate: false);
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
        private void OnVibrationToggled(bool isOn)
        {
            Taptic.tapticOn = isOn;
            PlayerPrefs.SetInt(PrefVibration, isOn ? 1 : 0);
            PlayerPrefs.Save();
        }
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
