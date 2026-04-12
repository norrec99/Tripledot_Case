using UnityEngine;

namespace TripledotCase.Core
{
    /// <summary>
    /// Bootstraps global application settings at startup.
    /// Attach this to a persistent GameObject in your first scene (e.g. GameManager).
    /// </summary>
    public class AppSettings : MonoBehaviour
    {
        [SerializeField] private int _targetFrameRate = 60;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0; // Disable VSync
            Application.targetFrameRate = _targetFrameRate;
        }
    }
}
