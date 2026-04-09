using UnityEngine;

namespace TripledotCase.UI.Popup
{
    /// <summary>
    /// Data container to easily configure Settings Rows.
    /// Right-click in the Project Window -> Create -> Tripledot -> Settings Row Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewSettingsRowData", menuName = "Tripledot/Settings Row Data")]
    public class SettingsRowData : ScriptableObject
    {
        public Sprite Icon;
        [Tooltip("e.g. popup.settings.row.sound")]
        public string LocalizationKey;
        public bool UseToggle = true;
        public bool UseChevron = false;
        public bool ShowDivider = true;
    }
}
