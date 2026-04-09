using DG.Tweening;
using TMPro;
using TripledotCase.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.Screens
{
    /// <summary>
    /// Orchestrates the highly-polished Level Completed entry sequence, featuring
    /// elastic drops, snappy scale bounces, and numbers that dramatically tick up.
    /// Satisfies Task 3: "focusing on impressive animation and creative flair".
    /// </summary>
    public class LevelCompletedView : MonoBehaviour
    {
        [Header("UI Layers")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;

        [Header("Animated Elements")]
        [SerializeField] private RectTransform _titleText;
        [SerializeField] private Transform _bigStar;
        [SerializeField] private CanvasGroup _lightRaysGroup;

        [Header("Tick-Up Counters")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _crownsText;

        [Header("Secret Sauce (VFX)")]
        [SerializeField] private UIParticleBurst _starBurstVFX;

        [Header("Actions")]
        [SerializeField] private Button _homeButton;

        private Sequence _entrySequence;
        private Vector2 _titleTargetPos;

        // Hardcoding targets for the demo layout as requested
        private int _targetScore = 250;
        private int _targetCoins = 100;
        private int _targetCrowns = 8;

        private void Awake()
        {
            // Lock in where the designer placed the title in the editor, so we can bounce it safely
            if (_titleText != null) _titleTargetPos = _titleText.anchoredPosition;

            EventManager.OnLevelCompletedTriggered += SequenceDemoEntry;

            if (_homeButton != null)
                _homeButton.onClick.AddListener(Hide);

            // Hide the screen initially
            _mainCanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            EventManager.OnLevelCompletedTriggered -= SequenceDemoEntry;
            if (_homeButton != null) _homeButton.onClick.RemoveListener(Hide);

            _entrySequence?.Kill();
        }

        // ── Animation Choreography ───────────────────────────────────────────────────

        private void SequenceDemoEntry()
        {
            gameObject.SetActive(true);
            _entrySequence?.Kill();

            // 1. Reset all elements back to an invisible "waiting" state
            _mainCanvasGroup.alpha = 0f;
            _titleText.anchoredPosition = _titleTargetPos + new Vector2(0, 500f); // 500px above screen
            _bigStar.localScale = Vector3.zero;
            _lightRaysGroup.alpha = 0f;
            _scoreText.text = "0";
            _coinsText.text = "0";
            _crownsText.text = "0";

            // 2. Build the master timeline
            _entrySequence = DOTween.Sequence()
                // First, calmly fade the dark blue background in
                .Append(_mainCanvasGroup.DOFade(1f, 0.4f))

                // CRASH the title down from the top with an extreme elastic rubber-band effect
                .Append(_titleText.DOAnchorPos(_titleTargetPos, 0.8f).SetEase(Ease.OutElastic))

                // SNAP the big star into existence using an overshoot curve
                .Insert(0.5f, _bigStar.DOScale(1f, 0.6f).SetEase(Ease.OutBack, 1.5f))

                // EXACTLY as the star reaches max size, fire the 2D star explosion VFX
                .InsertCallback(0.7f, () => { if (_starBurstVFX != null) _starBurstVFX.FireBurst(); })

                // Slowly fade in the infinitely-spinning light rays sitting behind the star
                .Insert(0.6f, _lightRaysGroup.DOFade(1f, 1f))

                // Tick the reward numbers up violently to make them feel heavy and earned
                .Insert(0.7f, DOVirtual.Int(0, _targetScore, 1.2f, v => _scoreText.text = v.ToString("N0")).SetEase(Ease.OutExpo))
                .Insert(0.9f, DOVirtual.Int(0, _targetCoins, 1.0f, v => _coinsText.text = v.ToString("N0")).SetEase(Ease.OutExpo))
                .Insert(1.1f, DOVirtual.Int(0, _targetCrowns, 0.8f, v => _crownsText.text = v.ToString("N0")).SetEase(Ease.OutExpo));
        }

        private void Hide()
        {
            _entrySequence?.Kill();

            // Standard dissolve out
            _mainCanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                gameObject.SetActive(false);
                EventManager.FireLevelCompletedClosed();
            });
        }
    }
}
