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

        private void Start()
        {
            // Placeholder: In a real system, you'd register this text to a Localization Manager
            // e.g., LocalizationManager.Register(this);
            // And fetch the initial translation
            
            if (!string.IsNullOrEmpty(_localizationKey))
            {
                // Force an update to show it's active in dev
                Debug.Log($"[LocalizedText] Fetching translation for key: {_localizationKey}");
            }
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
