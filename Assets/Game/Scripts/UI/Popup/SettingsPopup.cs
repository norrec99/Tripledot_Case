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
        [Header("Settings Toggles")]
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _vibrationToggle;
        [SerializeField] private Toggle _notificationsToggle;

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
            if (_soundToggle != null)       _soundToggle.onValueChanged.AddListener(OnSoundToggled);
            if (_musicToggle != null)       _musicToggle.onValueChanged.AddListener(OnMusicToggled);
            if (_vibrationToggle != null)   _vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
            if (_notificationsToggle != null) _notificationsToggle.onValueChanged.AddListener(OnNotificationsToggled);
            
            if (_privacyButton != null)     _privacyButton.onClick.AddListener(OnPrivacyClicked);
            if (_termsButton != null)       _termsButton.onClick.AddListener(OnTermsClicked);
            if (_supportButton != null)     _supportButton.onClick.AddListener(OnSupportClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_closeButton != null)       _closeButton.onClick.RemoveListener(RequestClose);

            if (_soundToggle != null)       _soundToggle.onValueChanged.RemoveListener(OnSoundToggled);
            if (_musicToggle != null)       _musicToggle.onValueChanged.RemoveListener(OnMusicToggled);
            if (_vibrationToggle != null)   _vibrationToggle.onValueChanged.RemoveListener(OnVibrationToggled);
            if (_notificationsToggle != null) _notificationsToggle.onValueChanged.RemoveListener(OnNotificationsToggled);
            
            if (_privacyButton != null)     _privacyButton.onClick.RemoveListener(OnPrivacyClicked);
            if (_termsButton != null)       _termsButton.onClick.RemoveListener(OnTermsClicked);
            if (_supportButton != null)     _supportButton.onClick.RemoveListener(OnSupportClicked);
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
            // _soundToggle.SetIsOnWithoutNotify(SaveData.SoundEnabled);
            Debug.Log("[SettingsPopup] Synced UI bounds to current player preferences.");
        }

        // ── Interaction Handlers ─────────────────────────────────────────────

        private void OnSoundToggled(bool isOn) => Debug.Log($"[SettingsPopup] Sound toggled: {isOn}");
        private void OnMusicToggled(bool isOn) => Debug.Log($"[SettingsPopup] Music toggled: {isOn}");
        private void OnVibrationToggled(bool isOn) => Debug.Log($"[SettingsPopup] Vibration toggled: {isOn}");
        private void OnNotificationsToggled(bool isOn) => Debug.Log($"[SettingsPopup] Notifications toggled: {isOn}");
        
        private void OnPrivacyClicked() => Debug.Log("[SettingsPopup] Privacy Policy opened.");
        private void OnTermsClicked() => Debug.Log("[SettingsPopup] Terms & Conditions opened.");
        private void OnSupportClicked() => Debug.Log("[SettingsPopup] Support center opened.");
    }
}
