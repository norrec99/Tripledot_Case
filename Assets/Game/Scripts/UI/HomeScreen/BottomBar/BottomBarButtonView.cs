using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.HomeScreen.BottomBar
{
    /// <summary>
    /// Visual state for a single navigation area button.
    ///
    /// Owns only its own icon and label animations:
    ///   Activate   → icon moves up, icon scales ×1.2, label fades in
    ///   Deactivate → reverses all of the above
    ///   Locked     → horizontal shake on tap
    ///
    /// The shared active-state card (background image) is owned and
    /// animated by BottomBarView — this class has no knowledge of it.
    ///
    /// Expected NavArea child hierarchy:
    ///   ├── LockedButton  (Button — visible only when Locked)
    ///   ├── NavButton     (Button — visible when Idle or Active)
    ///   ├── Icon          (Image)
    ///   └── Label         (TMP + CanvasGroup — fades in/out)
    /// </summary>
    public class BottomBarButtonView : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────────────

        [Header("Buttons")]
        [SerializeField] private Button _navButton;
        [SerializeField] private Button _lockedButton;

        [Header("Visuals")]
        [SerializeField] private RectTransform _iconRect;
        [SerializeField] private Image _icon;
        [SerializeField] private CanvasGroup _labelGroup;
        [SerializeField] private TextMeshProUGUI _label;

        [Header("Identity")]
        [SerializeField] private int _buttonIndex;
        [Tooltip("The localization key used for this button's label.")]
        [SerializeField] private string _localizationKey = "bottombar.button";
        [SerializeField] private ButtonState _initialState = ButtonState.Idle;

        [Header("Animation")]
        [SerializeField] private float _iconMoveY = 54f;
        [SerializeField] private float _iconActiveScale = 1.2f;
        [SerializeField] private float _activateDuration = 0.30f;
        [SerializeField] private float _deactivateDuration = 0.22f;
        [SerializeField] private float _shakeDuration = 0.30f;

        // ── State ──────────────────────────────────────────────────────────────────

        private ButtonState _currentState;
        private Sequence _sequence;
        private Vector2 _iconIdlePos;

        // ── Public API ─────────────────────────────────────────────────────────────

        public ButtonState CurrentState => _currentState;
        public int ButtonIndex => _buttonIndex;
        public bool IsLocked => _currentState == ButtonState.Locked;
        public bool IsActive => _currentState == ButtonState.Active;

        public System.Action<BottomBarButtonView> OnButtonClicked;
        public System.Action<BottomBarButtonView> OnLockedButtonClicked;

        // ── Lifecycle ──────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_label != null)
            {
                var locText = _label.GetComponent<TripledotCase.UI.Core.LocalizedText>();
                if (locText != null) locText.SetKey(_localizationKey);
                else _label.text = _localizationKey; // Fallback placeholder
            }
            _iconIdlePos = _iconRect.anchoredPosition;

            _navButton?.onClick.AddListener(HandleNavClick);
            _lockedButton?.onClick.AddListener(HandleLockedClick);

            ApplyStateImmediate(_initialState);
        }

        private void OnDestroy()
        {
            _navButton?.onClick.RemoveListener(HandleNavClick);
            _lockedButton?.onClick.RemoveListener(HandleLockedClick);
            KillTweens();
        }

        // ── Public Animation API ───────────────────────────────────────────────────

        public void Activate()
        {
            if (_currentState == ButtonState.Locked) return;
            KillTweens();
            _currentState = ButtonState.Active;

            _sequence = DOTween.Sequence()
                .Join(_iconRect.DOAnchorPos(_iconIdlePos + new Vector2(0f, _iconMoveY), _activateDuration)
                               .SetEase(Ease.OutCubic))
                .Join(_icon.transform.DOScale(_iconActiveScale, _activateDuration)
                               .SetEase(Ease.OutBack))
                .Join(_labelGroup.DOFade(1f, _activateDuration * 0.7f)
                               .SetEase(Ease.OutQuad)
                               .SetDelay(_activateDuration * 0.2f));
        }

        public void Deactivate()
        {
            if (_currentState != ButtonState.Active) return;
            KillTweens();
            _currentState = ButtonState.Idle;

            _sequence = DOTween.Sequence()
                .Join(_labelGroup.DOFade(0f, _deactivateDuration * 0.6f)
                               .SetEase(Ease.InQuad))
                .Join(_iconRect.DOAnchorPos(_iconIdlePos, _deactivateDuration)
                               .SetEase(Ease.InCubic))
                .Join(_icon.transform.DOScale(1f, _deactivateDuration)
                               .SetEase(Ease.InBack));
        }

        public void PlayLockedFeedback()
        {
            KillTweens();
            transform.DOShakePosition(_shakeDuration,
                                      new Vector3(10f, 0f, 0f),
                                      vibrato: 20, randomness: 0f,
                                      snapping: false, fadeOut: true);
        }

        // ── Private ────────────────────────────────────────────────────────────────

        private void HandleNavClick() => OnButtonClicked?.Invoke(this);
        private void HandleLockedClick() => OnLockedButtonClicked?.Invoke(this);

        private void ApplyStateImmediate(ButtonState state)
        {
            _currentState = state;
            bool isLocked = state == ButtonState.Locked;
            bool isActive = state == ButtonState.Active;

            _navButton?.gameObject.SetActive(!isLocked);
            _lockedButton?.gameObject.SetActive(isLocked);

            if (_labelGroup != null) _labelGroup.alpha = isActive ? 1f : 0f;

            if (_iconRect != null)
                _iconRect.anchoredPosition = isActive
                    ? _iconIdlePos + new Vector2(0f, _iconMoveY)
                    : _iconIdlePos;

            if (_icon != null)
                _icon.transform.localScale = isActive
                    ? Vector3.one * _iconActiveScale
                    : Vector3.one;
        }

        private void KillTweens()
        {
            _sequence?.Kill();
            DOTween.Kill(transform);
        }
    }

    public enum ButtonState { Idle, Active, Locked }
}
