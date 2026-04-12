using TripledotCase.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.DebugTools
{
    /// <summary>
    /// A temporary utility to allow playing with the Level Completed screen
    /// before backend game logic is fully implemented.
    /// Attach this to a temporary "Win" button on the Home Screen!
    /// </summary>
    public class DebugTrigger : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(FireTrigger);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(FireTrigger);
        }

        private void FireTrigger()
        {
            Debug.Log("[DebugTrigger] Simulating level completion...");
            EventManager.FireLevelCompletedTriggered();
        }
    }
}
