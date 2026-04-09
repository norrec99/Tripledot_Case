using System.Collections.Generic;
using UnityEngine;

namespace TripledotCase.UI.Core
{
    [System.Serializable]
    public struct TranslationEntry
    {
        [Tooltip("e.g. popup.settings.sound")]
        public string Key;
        
        [Tooltip("The actual translated word. e.g. Sonido (for Spanish)")]
        public string Value;
    }

    /// <summary>
    /// A data container for a single language translation set.
    /// Create via Right Click -> Tripledot -> Language Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewLanguageData", menuName = "Tripledot/Language Data")]
    public class LanguageData : ScriptableObject
    {
        public string LanguageID = "EN"; 
        
        [Tooltip("The flag icon representing this language.")]
        public Sprite LanguageIcon;
        
        [Header("Translations")]
        public List<TranslationEntry> Entries = new List<TranslationEntry>();
    }
}
