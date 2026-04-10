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

        private RectTransform _rect;
        private Vector2 _targetPos;
        private bool _isPosCached = false;

        /// <summary>
        /// Pushes the reward way above the screen and zeros the counter, ready for the drop sequence!
        /// </summary>
        public void PrepareForEntry()
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            
            // Only cache the position ONCE! If we cache it midway through an animation, it corrupts the baseline!
            if (!_isPosCached) 
            {
                _targetPos = _rect.anchoredPosition;
                _isPosCached = true;
            }
            
            // Move it 800px up into the sky
            _rect.anchoredPosition = _targetPos + new Vector2(0, 800f);

            if (_amountText != null) _amountText.text = "0";
        }

        /// <summary>
        /// Chains a physical sky-drop and perfectly times the DOCounter sequence to start exactly when the drop finishes!
        /// </summary>
        public Sequence PlayEntryAndCount(float dropDuration, float countDuration)
        {
            Sequence seq = DOTween.Sequence();

            if (_rect != null)
            {
                // Drop from the sky with a heavy, satisfying bounce
                seq.Append(_rect.DOAnchorPos(_targetPos, dropDuration).SetEase(Ease.OutBounce));
            }

            if (_data != null && _amountText != null)
            {
                // Tick the numbers up immediately after the bounce finishes
                seq.AppendCallback(() => _amountText.text = "0"); // safeguard
                seq.Append(DOVirtual.Int(0, _data.Amount, countDuration, v => _amountText.text = v.ToString("N0")).SetEase(Ease.OutExpo));
            }

            return seq;
        }
    }
}
