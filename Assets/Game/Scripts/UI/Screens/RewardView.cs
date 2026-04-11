using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.Screens
{
    /// <summary>
    /// Displays a single reward and owns its own count-up animation sequence.
    /// Uses OnValidate to seamlessly populate in the editor via ScriptableObjects.
    /// </summary>
    public class RewardView : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private RewardData _data;

        [Header("UI Links")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;

        [Header("Animation")]
        [Tooltip("Leave empty to animate this root object, or drag a child RectTransform here if this script sits on a Layout Wrapper!")]
        [SerializeField] private RectTransform _animatorRect;

        private void OnValidate()
        {
            if (_data == null) return;

            if (_iconImage != null && _data.Icon != null)
                _iconImage.sprite = _data.Icon;

#if UNITY_EDITOR
            // Give instant layout feedback in the editor, but don't do it at runtime 
            // because we want the value to start at 0 and animate up!
            if (!Application.isPlaying && _amountText != null)
                _amountText.text = _data.Amount.ToString("N0");
#endif
        }

        private RectTransform _targetRect;
        private CanvasGroup _canvasGroup;

        /// <summary>
        /// Shrinks the reward, makes it transparent, and hides the text completely ready for the intro!
        /// </summary>
        public void PrepareForEntry()
        {
            _targetRect = _animatorRect != null ? _animatorRect : GetComponent<RectTransform>();
            
            // Failsafe: Ensures we can cleanly fade the element without caring about what type of Images/Texts it uses
            _canvasGroup = _targetRect.GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = _targetRect.gameObject.AddComponent<CanvasGroup>();
            
            // Start tiny and invisible!
            _targetRect.localScale = Vector3.one * 0.5f;
            _canvasGroup.alpha = 0f;

            // Hide text completely
            if (_amountText != null) _amountText.text = "";
        }

        /// <summary>
        /// Fades and scales the reward in, then pops the text onto the screen and counts it up.
        /// </summary>
        public Sequence PlayEntryAndCount(float entryDuration, float countDuration)
        {
            Sequence seq = DOTween.Sequence();

            if (_targetRect != null && _canvasGroup != null)
            {
                // Smoothly fade in while softly popping the scale to 1.0!
                seq.Append(_targetRect.DOScale(Vector3.one, entryDuration).SetEase(Ease.OutBack, 1.2f));
                seq.Join(_canvasGroup.DOFade(1f, entryDuration));
            }

            if (_data != null && _amountText != null)
            {
                // Instantly appear as "0" exactly when the fade finishes, then start counting
                seq.AppendCallback(() => _amountText.text = "0"); 
                seq.Append(DOVirtual.Int(0, _data.Amount, countDuration, v => _amountText.text = v.ToString("N0")).SetEase(Ease.OutExpo));
            }

            return seq;
        }
    }
}
