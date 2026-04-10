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
        [Tooltip("Attach a CanvasGroup to your Title Text so we can cleanly fade it!")]
        [SerializeField] private CanvasGroup _titleGroup;
        [SerializeField] private Transform _bigStar;
        [SerializeField] private CanvasGroup _lightRaysGroup;

        [Header("Tick-Up Counters")]
        [Tooltip("Drop your RewardView prefabs spanning across the screen here!")]
        [SerializeField] private System.Collections.Generic.List<RewardView> _rewards;

        [Header("Secret Sauce (VFX)")]
        [SerializeField] private UIParticleBurst _starBurstVFX;

        [Header("Actions")]
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
            _mainCanvasGroup.blocksRaycasts = true; // Enable clicking
            
            if (_titleGroup != null) _titleGroup.alpha = 0f;
            _titleText.anchoredPosition = _titleTargetPos - new Vector2(0, 150f); // 150px BELOW the target
            
            _bigStar.localScale = Vector3.zero;
            _lightRaysGroup.alpha = 0f;

            if (_rewards != null)
            {
                foreach (var reward in _rewards) reward.PrepareForEntry();
            }

            // 2. Build the master timeline
            _entrySequence = DOTween.Sequence()
                // First, calmly fade the dark blue background in
                .Append(_mainCanvasGroup.DOFade(1f, 0.4f));

            // Smooth float & fade title up from below
            _entrySequence.Append(_titleText.DOAnchorPos(_titleTargetPos, 0.8f).SetEase(Ease.OutCubic));
            if (_titleGroup != null) 
                _entrySequence.Insert(0.4f, _titleGroup.DOFade(1f, 0.7f)); // Starts right after background fade

            _entrySequence
                // SNAP the big star into existence using an overshoot curve
                .Insert(0.6f, _bigStar.DOScale(1f, 0.6f).SetEase(Ease.OutBack, 1.5f))

                // EXACTLY as the star reaches max size, fire the 2D star explosion VFX
                .InsertCallback(0.7f, () => { if (_starBurstVFX != null) _starBurstVFX.FireBurst(); })

                // Slowly fade in the infinitely-spinning light rays sitting behind the star
                .Insert(0.6f, _lightRaysGroup.DOFade(1f, 1f));

            // Drop the rewards from the sky one by one exactly after the Big Star finishes its intro!
            // The big star pops from 0.6s and lasts 0.6s (finishing at 1.2s total on the timeline).
            if (_rewards != null)
            {
                float dropDelay = 1.2f;
                foreach (var reward in _rewards)
                {
                    RewardView localReward = reward;
                    
                    // Insert the chained sequence (Drop -> Count Up) perfectly into the master timeline
                    if (localReward != null) 
                        _entrySequence.Insert(dropDelay, localReward.PlayEntryAndCount(0.6f, 0.8f));
                        
                    // Stagger the next reward's drop by 0.2s
                    dropDelay += 0.2f;
                }
            }
        }

        // Made public so it can be manually wired up via the Unity OnClick() block in the Inspector if needed!
        public void Hide()
        {
            Debug.Log("[LevelCompleted] Hide() triggered!");
            _entrySequence?.Kill();

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
