using TMPro;
using TripledotCase.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.Popup
{
    /// <summary>
    /// Optional view class to cleanly manage a single Settings Row prefab.
    /// This makes it easy to assign different icons, labels, and turn the right-side
    /// UI elements (toggle vs chevron button) on or off per row.
    /// </summary>
    public class SettingsRowView : MonoBehaviour
    {
        [Header("Data Profile")]
        [SerializeField] private SettingsRowData _data;

        [Header("Left Side")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _label;

        [Header("Right Side (Interactive)")]
        [Tooltip("Assign the on/off switch here if this row uses a toggle (Music, Sound, etc).")]
        [SerializeField] private Toggle _toggle;

        [Tooltip("Assign the chevron/arrow button here if this row goes to another menu (Language).")]
        [SerializeField] private Button _actionButton;

        [Header("Decorations")]
        [Tooltip("Assign the thin bottom divider line GameObject here.")]
        [SerializeField] private GameObject _divider;

        public Toggle RowToggle => _toggle;
        public Button ActionButton => _actionButton;

        private bool _isIconOverridden = false;

        /// <summary>
        /// Unity calls OnValidate automatically in the Editor whenever a value changes.
        /// The moment you drag a ScriptableObject into the "_data" slot, your UI updates instantly!
        /// </summary>
        private void OnValidate()
        {
            if (_data == null) return;

            if (_iconImage != null && _data.Icon != null && !_isIconOverridden)
                _iconImage.sprite = _data.Icon;

            if (_label != null)
            {
                // Put placeholders in editor, wait for runtime to inject into localized text natively
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    _label.text = $"[{_data.LocalizationKey}]";
#endif
            }

            if (_toggle != null)
                _toggle.gameObject.SetActive(_data.UseToggle);

            if (_actionButton != null)
                _actionButton.gameObject.SetActive(_data.UseChevron);

            if (_divider != null)
                _divider.SetActive(_data.ShowDivider);
        }

        private void Start()
        {
            // Sync the localization natively on boot
            if (_label != null && _data != null)
            {
                var locText = _label.GetComponent<LocalizedText>();
                if (locText != null)
                {
                    locText.SetKey(_data.LocalizationKey);
                }
            }

            // Ensure data is synced when the game physically starts
            OnValidate();
        }

        /// <summary>Allows external scripts to override the icon (e.g. dynamically changing flags).</summary>
        public void SetIcon(Sprite newIcon)
        {
            if (_iconImage != null && newIcon != null)
            {
                _iconImage.sprite = newIcon;
                _isIconOverridden = true;
            }
        }
    }
}
