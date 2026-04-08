using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.HomeScreen.BottomBar
{
    /// <summary>
    /// Represents a single button in the BottomBar.
    ///
    /// Each button has three states driven by DOTween:
    ///   Idle    → flat in the bar, no card, no label
    ///   Active  → card rises above the bar, icon punches, label fades in
    ///   Locked  → flat, lock overlay visible, horizontal shake on tap
    ///
    /// This class owns only its own visual state.
    /// All selection logic lives in BottomBarView — this keeps
    /// the button reusable and independently testable.
    ///
    /// Hierarchy expected:
    ///   BottomBarButton  (Button + BottomBarButtonView)
    ///   └── CardRect     (RectTransform, pivot bottom-center so it grows upward)
    ///       ├── CardBG   (Image — the coloured rounded card)
    ///       ├── Icon     (Image — button icon)
    ///       └── LabelGroup (CanvasGroup)
    ///           └── Label   (TextMeshProUGUI)
    ///   └── LockOverlay  (GameObject — padlock icon, hidden for unlocked buttons)
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BottomBarButtonView : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────────────

        [Header("References")]
        [SerializeField] private RectTransform _cardRect;
        [SerializeField] private Image _cardBackground;
        [SerializeField] private Image _icon;
        [SerializeField] private CanvasGroup _labelGroup;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private GameObject _lockOverlay;

        [Header("Identity")]
        [SerializeField] private int _buttonIndex;
        [SerializeField] private string _buttonLabel = "Button";
        [SerializeField] private ButtonState _initialState = ButtonState.Idle;

        [Header("Card Sizes")]
        [Tooltip("Card size when the button is idle/inactive.")]
        [SerializeField] private Vector2 _idleCardSize = new Vector2(80f, 80f);
        [Tooltip("Card size when the button is active — grows upward thanks to bottom-center pivot.")]
        [SerializeField] private Vector2 _activeCardSize = new Vector2(110f, 130f);

        [Header("Card Colors")]
        [SerializeField] private Color _activeCardColor = new Color(0.34f, 0.75f, 0.97f, 1f); // #56C0F7
        [SerializeField] private Color _idleCardColor = new Color(0f, 0f, 0f, 0f); // transparent

        // ── Animation Timings ──────────────────────────────────────────────────────

        [Header("Durations (seconds)")]
        [SerializeField] private float _activateDuration = 0.30f;
        [SerializeField] private float _deactivateDuration = 0.20f;
        [SerializeField] private float _labelFadeDuration = 0.18f;
        [SerializeField] private float _shakeDuration = 0.30f;

        // ── State ──────────────────────────────────────────────────────────────────

        private ButtonState _currentState;
        private Sequence _currentSequence;
        private Button _button;

        // ── Public API ─────────────────────────────────────────────────────────────

        public ButtonState CurrentState => _currentState;
        public int ButtonIndex => _buttonIndex;
        public bool IsLocked => _currentState == ButtonState.Locked;
        public bool IsActive => _currentState == ButtonState.Active;

        /// <summary>
        /// Wired by BottomBarView. Invoked whenever this button is clicked,
        /// passing itself as the argument so BottomBarView can route the event.
        /// </summary>
        public System.Action<BottomBarButtonView> OnButtonClicked;

        // ── Lifecycle ──────────────────────────────────────────────────────────────

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);

            if (_label != null)
                _label.text = _buttonLabel;

            ApplyStateImmediate(_initialState);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
            KillTweens();
        }

        // ── Public Animation Methods ───────────────────────────────────────────────

        /// <summary>
        /// Animates the button from Idle → Active:
        /// card expands upward, background colours in, icon pops, label fades in.
        /// </summary>
        public void Activate()
        {
            if (_currentState == ButtonState.Locked) return;

            KillTweens();
            _currentState = ButtonState.Active;

            _currentSequence = DOTween.Sequence()
                .Join(_cardRect.DOSizeDelta(_activeCardSize, _activateDuration)
                               .SetEase(Ease.OutBack))
                .Join(_cardBackground.DOColor(_activeCardColor, _activateDuration * 0.85f)
                               .SetEase(Ease.OutQuad))
                .Join(_icon.transform.DOPunchScale(Vector3.one * 0.25f, _activateDuration, 6, 0.5f))
                .Join(_labelGroup.DOFade(1f, _labelFadeDuration)
                               .SetEase(Ease.OutQuad)
                               .SetDelay(_activateDuration * 0.25f));
        }

        /// <summary>
        /// Animates the button from Active → Idle:
        /// label fades out first, then card collapses and card colour clears.
        /// </summary>
        public void Deactivate()
        {
            if (_currentState != ButtonState.Active) return;

            KillTweens();
            _currentState = ButtonState.Idle;

            _currentSequence = DOTween.Sequence()
                .Join(_labelGroup.DOFade(0f, _labelFadeDuration)
                               .SetEase(Ease.InQuad))
                .Join(_cardRect.DOSizeDelta(_idleCardSize, _deactivateDuration)
                               .SetEase(Ease.InBack)
                               .SetDelay(_labelFadeDuration * 0.5f))
                .Join(_cardBackground.DOColor(_idleCardColor, _deactivateDuration)
                               .SetEase(Ease.InQuad));
        }

        /// <summary>
        /// Plays a short horizontal shake — tactile feedback for tapping a locked button.
        /// Does not change state.
        /// </summary>
        public void PlayLockedFeedback()
        {
            KillTweens();
            // Shake only on X axis; fadeOut:true makes it feel natural
            transform.DOShakePosition(_shakeDuration,
                                      strength: new Vector3(10f, 0f, 0f),
                                      vibrato: 20,
                                      randomness: 0f,
                                      snapping: false,
                                      fadeOut: true);
        }

        // ── Private ────────────────────────────────────────────────────────────────

        private void HandleClick() => OnButtonClicked?.Invoke(this);

        /// <summary>Snaps the button to the correct visual state with no animation.</summary>
        private void ApplyStateImmediate(ButtonState state)
        {
            _currentState = state;

            bool isActive = state == ButtonState.Active;
            bool isLocked = state == ButtonState.Locked;

            _cardRect.sizeDelta = isActive ? _activeCardSize : _idleCardSize;
            _cardBackground.color = isActive ? _activeCardColor : _idleCardColor;

            if (_labelGroup != null)
                _labelGroup.alpha = isActive ? 1f : 0f;

            if (_lockOverlay != null)
                _lockOverlay.SetActive(isLocked);
        }

        private void KillTweens()
        {
            _currentSequence?.Kill();
            DOTween.Kill(transform);
        }
    }

    // ── Enum ───────────────────────────────────────────────────────────────────────

    /// <summary>The three visual states a bottom bar button can be in.</summary>
    public enum ButtonState
    {
        Idle,    // Flat in the bar, not selected
        Active,  // Card elevated, content visible
        Locked   // Unavailable; shows padlock, shakes on tap
    }
}
