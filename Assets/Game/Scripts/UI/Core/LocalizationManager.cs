using System;
using System.Collections.Generic;
using UnityEngine;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// A lightweight implementation of a real Localization system.
    /// Attach this to your core Canvas or an Empty "GameManager" object in the scene.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        /// <summary>Fired when the language changes so all UI can refresh instantly.</summary>
        public static event Action OnLanguageChanged;

        [Header("Current Configuration")]
        [Tooltip("Drop a LanguageData ScriptableObject here (e.g. 'English') to boot the game with it.")]
        [SerializeField] private LanguageData _activeLanguage;

        [Header("Available Languages")]
        [Tooltip("List all available LanguageData objects here (e.g. English, Turkish) so the game can cycle through them.")]
        [SerializeField] private List<LanguageData> _availableLanguages = new List<LanguageData>();

        public LanguageData ActiveLanguage => _activeLanguage;

        // Extremely fast lookup dictionary for the active language
        private Dictionary<string, string> _translations = new Dictionary<string, string>();

        private void Awake()
        {
            // Standard Singleton pattern protecting core managers
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadLanguage(_activeLanguage);
        }

        /// <summary>
        /// Swaps the entire game's language and tells all text to update.
        /// Call this when the player selects a new language in the settings!
        /// </summary>
        public void LoadLanguage(LanguageData languageData)
        {
            if (languageData == null) return;

            _activeLanguage = languageData;
            _translations.Clear();

            // Populate the fast lookup dictionary
            foreach (var entry in languageData.Entries)
            {
                if (!_translations.ContainsKey(entry.Key))
                {
                    _translations.Add(entry.Key, entry.Value);
                }
            }

            Debug.Log($"[Localization] Loaded language: {_activeLanguage.LanguageID} with {_translations.Count} entries.");
            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Instantly swaps to the next language in the available list.
        /// </summary>
        public void CycleNextLanguage()
        {
            if (_availableLanguages == null || _availableLanguages.Count <= 1) 
                return;

            int currentIndex = _availableLanguages.IndexOf(_activeLanguage);
            int nextIndex = (currentIndex + 1) % _availableLanguages.Count;
            
            LoadLanguage(_availableLanguages[nextIndex]);
        }

        /// <summary>
        /// Returns the translated string for a key. If not found, returns the raw key to make missing translations obvious.
        /// </summary>
        public string GetTranslation(string key)
        {
            if (string.IsNullOrEmpty(key)) return "";

            if (_translations.TryGetValue(key, out string translatedValue))
            {
                return translatedValue;
            }
            
            return $"[{key}]"; // Fallback identifier
        }
    }
}
