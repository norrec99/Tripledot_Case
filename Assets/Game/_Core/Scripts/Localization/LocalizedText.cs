using TMPro;
using UnityEngine;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// Stub class for future localization integration.
    /// Satisfies Requirement #3 (Text & Localization Preparedness).
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [Header("Localization Setup")]
        [Tooltip("The unique key used to fetch the translation from the localization database.")]
        [SerializeField] private string _localizationKey;

        private TextMeshProUGUI _textComponent;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>Allows external scripts to dynamically feed new localization keys.</summary>
        public void SetKey(string newKey)
        {
            _localizationKey = newKey;
            
            // Only attempt to fetch if the game is actually running and manager exists
            if (Application.isPlaying && LocalizationManager.Instance != null)
            {
                RefreshText();
            }
            else if (_textComponent != null)
            {
               _textComponent.text = $"[{_localizationKey}]"; // Editor placeholder
            }
        }

        private void Start()
        {
            if (LocalizationManager.Instance != null)
            {
                // Subscribe so it updates dynamically if the user swaps language mid-game!
                LocalizationManager.OnLanguageChanged += RefreshText;
                
                // Fetch the initial translation immediately on load
                RefreshText(); 
            }
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLanguageChanged -= RefreshText;
        }

        private void RefreshText()
        {
            if (string.IsNullOrEmpty(_localizationKey)) return;
            UpdateTranslation(LocalizationManager.Instance.GetTranslation(_localizationKey));
        }

        /// <summary>
        /// Called by central localization manager when the language changes.
        /// </summary>
        public void UpdateTranslation(string translatedText)
        {
            if (_textComponent != null)
            {
                _textComponent.text = translatedText;
            }
        }
    }
}
