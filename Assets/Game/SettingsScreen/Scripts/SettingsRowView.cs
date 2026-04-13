using DG.Tweening;
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
        [Tooltip("Assign the root GameObject of the switch here if this row uses a custom switch.")]
        [SerializeField] private GameObject _switchRoot;
        [Tooltip("A single giant invisible button that covers the entire switch background!")]
        [SerializeField] private Button _switchButton;
        [SerializeField] private RectTransform _switchHandle;
        [Tooltip("The Image component that swaps sprites.")]
        [SerializeField] private Image _switchDynamicImage;
        [SerializeField] private float _handleOnX = 22f;
        [SerializeField] private float _handleOffX = -22f;
        [SerializeField] private float _switchDuration = 0.2f;

        [Tooltip("Assign the chevron/arrow button here if this row goes to another menu (Language).")]
        [SerializeField] private Button _actionButton;

        [Header("Decorations")]
        [Tooltip("Assign the thin bottom divider line GameObject here.")]
        [SerializeField] private GameObject _divider;

        public System.Action<bool> OnSwitchToggled;
        private bool _isSwitchOn = true;

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

            if (_switchRoot != null)
            {
                _switchRoot.SetActive(_data.UseToggle);
                if (_switchDynamicImage != null && _data.UseToggle && _data.ToggleOnIcon != null && _data.ToggleOffIcon != null)
                {
                    _switchDynamicImage.sprite = _isSwitchOn ? _data.ToggleOnIcon : _data.ToggleOffIcon;
                }
            }

            if (_actionButton != null)
                _actionButton.gameObject.SetActive(_data.UseChevron);

            if (_divider != null)
                _divider.SetActive(_data.ShowDivider);
        }

        private void Awake()
        {
            // A single button just inverts whatever the current state is!
            if (_switchButton != null) _switchButton.onClick.AddListener(() => SetSwitchState(!_isSwitchOn, true));
        }

        private void OnDestroy()
        {
            if (_switchButton != null) _switchButton.onClick.RemoveAllListeners();
        }

        public void SetSwitchState(bool isOn, bool animate = false)
        {
            _isSwitchOn = isOn;
            float targetX = isOn ? _handleOnX : _handleOffX;

            if (_switchHandle != null)
            {
                _switchHandle.DOKill();
                if (animate)
                    _switchHandle.DOAnchorPosX(targetX, _switchDuration).SetEase(Ease.OutBack, 2.1f);
                else
                    _switchHandle.anchoredPosition = new Vector2(targetX, _switchHandle.anchoredPosition.y);
            }

            if (_switchDynamicImage != null && _data != null && _data.ToggleOnIcon != null && _data.ToggleOffIcon != null)
            {
                _switchDynamicImage.sprite = isOn ? _data.ToggleOnIcon : _data.ToggleOffIcon;
            }

            if (animate)
            {
                Taptic.Medium();
                OnSwitchToggled?.Invoke(isOn);
            }
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
