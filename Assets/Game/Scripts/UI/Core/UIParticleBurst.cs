using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace TripledotCase.UI.Core
{
    /// <summary>
    /// A lightweight UI Particle system providing creative VFX without the deep
    /// sorting-order headaches of mixing 3D Shuriken particles with Canvas UI.
    /// Perfect for the "Star Burst" effect when the Level Completed star pops in!
    /// </summary>
    public class UIParticleBurst : MonoBehaviour
    {
        [Header("VFX Configuration")]
        [Tooltip("The sparkle or tiny star sprite to shoot out.")]
        [SerializeField] private Sprite _particleSprite;
        
        [Tooltip("How many particles to spawn in a single burst.")]
        [SerializeField] private int _particleCount = 12;
        
        [Tooltip("How far outward they travel in pixels.")]
        [SerializeField] private float _burstRadius = 250f;
        
        [Tooltip("How long the burst lasts before vanishing.")]
        [SerializeField] private float _burstDuration = 0.75f;

        public void FireBurst()
        {
            if (_particleSprite == null) return;

            for (int i = 0; i < _particleCount; i++)
            {
                // Instantiate a temporary UI Image to act as a particle
                GameObject pObj = new GameObject("UI_SparkleParticle");
                pObj.transform.SetParent(transform, false);
                
                Image pImg = pObj.AddComponent<Image>();
                pImg.sprite = _particleSprite;
                pImg.SetNativeSize();
                
                // Add a little randomness to size so it looks organic
                pObj.transform.localScale = Vector3.one * Random.Range(0.6f, 1.2f);
                
                RectTransform rt = pObj.GetComponent<RectTransform>();
                rt.anchoredPosition = Vector2.zero; // Start exactly at the origin of this object

                // Calculate a random outward angle for a circular explosion
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                Vector2 targetPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(_burstRadius * 0.4f, _burstRadius);

                // Tween the particle outward, fading it, and shrinking it to dust!
                Sequence seq = DOTween.Sequence();
                seq.Join(rt.DOAnchorPos(targetPos, _burstDuration).SetEase(Ease.OutCirc));
                seq.Join(pImg.DOFade(0f, _burstDuration).SetEase(Ease.InQuad));
                seq.Join(rt.DOScale(0f, _burstDuration).SetEase(Ease.InBack));
                
                // Clean up the memory when it finishes
                seq.OnComplete(() => Destroy(pObj));
            }
        }
    }
}
