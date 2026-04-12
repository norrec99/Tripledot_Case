using UnityEngine;

namespace TripledotCase.UI.Screens
{
    /// <summary>
    /// Configuration for a specific reward awarded during Level Complete.
    /// Right-click -> Tripledot -> Reward Data
    /// </summary>
    [CreateAssetMenu(fileName = "NewRewardData", menuName = "Tripledot/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Tooltip("The icon representing the currency/reward")]
        public Sprite Icon;
        
        [Tooltip("The target amount to count up to")]
        public int Amount;
    }
}
