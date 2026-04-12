using System.Collections.Generic;
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
    /// </summary>
    public class LevelCompletedView : MonoBehaviour
    {
        [Header("UI Layers")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;

        [Header("Animated Elements")]
        [SerializeField] private RectTransform _titleTextBack;
        [SerializeField] private RectTransform _titleText;
        [SerializeField] private Transform _bigStar;

        [Header("Score Configuration")]
        [Tooltip("The massive score text sitting directly under the star")]
        [SerializeField] private TextMeshProUGUI _mainScoreText;
        [Tooltip("Target to count up to for the demo")]
        [SerializeField] private int _targetScore = 250;

        [Header("Tick-Up Counters")]
        [Tooltip("Drop your RewardView prefabs spanning across the screen here!")]
        [SerializeField] private List<RewardView> _rewards;

        [Header("Secret Sauce (VFX)")]
        [SerializeField] private GameObject _shineVFX;
        [SerializeField] private GameObject _constantShineVFX;
        [SerializeField] private GameObject _starVFX;

        [Header("Actions")]
        [Tooltip("The CanvasGroup holding your buttons so they cleanly fade in at the end")]
        [SerializeField] private CanvasGroup _buttonContainer;
        [SerializeField] private Button _homeButton;

        private Sequence _entrySequence;
        private Vector2 _titleTargetPos;

        private void Awake()
        {
            // Lock in where the designer placed the title in the editor, so we can bounce it safely
            if (_titleText != null) _titleTargetPos = _titleText.anchoredPosition;

            EventManager.OnLevelCompletedTriggered += SequenceDemoEntry;

            if (_homeButton != null)
                _homeButton.onClick.AddListener(Hide);

            // Hide the screen initially
            if (_buttonContainer != null)
            {
                _buttonContainer.alpha = 0f;
                _buttonContainer.interactable = false;
            }

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

        private void ToggleVFX(bool isActive)
        {
            if (isActive) Taptic.Success(); // Particle burst moment — double-pulse celebration!
            _starVFX.SetActive(isActive);
            _constantShineVFX.SetActive(isActive);
            _shineVFX.SetActive(isActive);
        }

        private void SequenceDemoEntry()
        {
            gameObject.SetActive(true);
            _entrySequence?.Kill();

            // 1. Reset all elements back to an invisible "waiting" state
            _mainCanvasGroup.alpha = 0f;
            _mainCanvasGroup.blocksRaycasts = true; // Enable clicking

            // Title Bounce Setup: Lock the text rigidly in place, and scale it to 0
            _titleText.anchoredPosition = _titleTargetPos;
            _titleTextBack.anchoredPosition = _titleTargetPos;
            _titleText.localScale = Vector3.zero;
            _titleTextBack.localScale = Vector3.zero;

            // Hide star until it's ready to slam
            _bigStar.localScale = Vector3.zero;

            if (_buttonContainer != null)
            {
                _buttonContainer.alpha = 0f;
                _buttonContainer.interactable = false; // Prevent clicking while invisible!
            }

            // Reset Main Score
            if (_mainScoreText != null)
            {
                _mainScoreText.text = "";
                _mainScoreText.alpha = 0f;
                _mainScoreText.transform.localScale = Vector3.one * 0.5f;
            }

            if (_rewards != null)
            {
                foreach (var reward in _rewards) reward.PrepareForEntry();
            }

            // 2. Build the master timeline
            _entrySequence = DOTween.Sequence()
                // First, calmly fade the dark blue background in
                .Append(_mainCanvasGroup.DOFade(1f, 0.4f));

            // Bouncy Scale effect for Title Data
            _entrySequence.Insert(0.4f, _titleText.DOScale(1f, 0.6f).SetEase(Ease.OutBounce, 1.7f));
            _entrySequence.Insert(0.4f, _titleTextBack.DOScale(1f, 0.6f).SetEase(Ease.OutBounce, 1.7f));

            // Build the Epic Star SLAM sequence
            Sequence starSlam = DOTween.Sequence();
            starSlam.AppendCallback(() =>
            {
                // Teleport the star to be massive and rotated right before the visible tween starts
                _bigStar.localScale = Vector3.one * 3.5f;
                _bigStar.localRotation = Quaternion.Euler(0, 0, 90f);
            });
            // Slam it down into the center while rotating back perfectly straight!
            starSlam.Append(_bigStar.DOScale(Vector3.one, 0.25f));
            starSlam.Join(_bigStar.DORotate(Vector3.zero, 0.25f));
            starSlam.AppendCallback(() => Taptic.Heavy()); // Physical impact moment!
            starSlam.Append(_bigStar.DOScale(Vector3.one * 0.6f, 0f));
            starSlam.InsertCallback(0.25f, () => _shineVFX.SetActive(true));
            starSlam.Append(_bigStar.DOScale(Vector3.one * 1.1f, 0.2f).SetEase(Ease.OutSine));
            starSlam.Append(_bigStar.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutCubic));
            starSlam.InsertCallback(3f, () => _shineVFX.SetActive(false));
            starSlam.InsertCallback(0.5f, () => ToggleVFX(true));


            _entrySequence
                // Insert the slam sequence right after the title starts floating
                .Insert(0.6f, starSlam);

            // Execute the Main Score Pop exactly after the star settles
            if (_mainScoreText != null)
            {
                _entrySequence.InsertCallback(0.7f, () => _mainScoreText.text = "0");
                _entrySequence.Insert(0.7f, _mainScoreText.DOFade(1f, 0.3f));
                _entrySequence.Insert(0.7f, _mainScoreText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack, 1.5f));
                _entrySequence.Insert(0.7f, DOVirtual.Int(0, _targetScore, 1.0f, v => _mainScoreText.text = v.ToString("N0")).SetEase(Ease.OutExpo));
            }

            // Pop the rewards from the sky one by one exactly after everything is settled
            // The Star impact + explosion ends around 0.85s
            float buttonFadeTime = 1.1f; // Fallback time just in case there are no rewards

            if (_rewards != null && _rewards.Count > 0)
            {
                float sequenceDelay = 1.1f;
                foreach (var reward in _rewards)
                {
                    RewardView localReward = reward;

                    if (localReward != null)
                        _entrySequence.Insert(sequenceDelay, localReward.PlayEntryAndCount(0.5f, 0.8f));

                    // Stagger the next reward's pop by 0.2s
                    sequenceDelay += 0.2f;
                }

                // Calculate the exact millisecond the FINAL reward hits the ground
                // The last reward started at (sequenceDelay - 0.2f) and takes 0.5s to drop
                buttonFadeTime = (sequenceDelay - 0.2f) + 0.5f;
            }

            // Finally, cleanly fade the interaction buttons in!
            if (_buttonContainer != null)
            {
                _entrySequence.Insert(buttonFadeTime, _buttonContainer.DOFade(1f, 0.4f));
                _entrySequence.InsertCallback(buttonFadeTime, () => _buttonContainer.interactable = true);
            }
        }

        // Made public so it can be manually wired up via the Unity OnClick() block in the Inspector if needed!
        public void Hide()
        {
            Debug.Log("[LevelCompleted] Hide() triggered!");
            _entrySequence?.Kill();

            ToggleVFX(false);

            // Prevent spam clicking while fading out
            if (_mainCanvasGroup != null)
                _mainCanvasGroup.blocksRaycasts = false;

            // Standard dissolve out
            _mainCanvasGroup.DOFade(0f, 0.3f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                gameObject.SetActive(false);
                EventManager.FireLevelCompletedClosed();
            });
        }
    }
}
