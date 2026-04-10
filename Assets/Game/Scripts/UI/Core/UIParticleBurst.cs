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

        [Tooltip("How many particles to spawn entirely.")]
        [SerializeField] private int _particleCount = 15;

        [Tooltip("Radius of the circular area where particles will randomly appear.")]
        [SerializeField] private float _spawnRadius = 150f;

        [Tooltip("How far upwards they float before vanishing.")]
        [SerializeField] private float _floatDistance = 80f;

        [Tooltip("Time delay between each particle appearing.")]
        [SerializeField] private float _staggerDelay = 0.06f;

        [Tooltip("How long each particle lives before dying.")]
        [SerializeField] private float _particleLifetime = 1.0f;

        public void FireBurst()
        {
            if (_particleSprite == null) return;

            for (int i = 0; i < _particleCount; i++)
            {
                // Stagger their appearances + add a tiny bit of random noise
                float delay = (i * _staggerDelay) + Random.Range(0f, 0.05f);
                DOVirtual.DelayedCall(delay, SpawnSingleParticle);
            }
        }

        private void SpawnSingleParticle()
        {
            // Instantiate a temporary UI Image to act as a particle
            GameObject pObj = new GameObject("UI_SparkleParticle");
            pObj.transform.SetParent(transform, false);

            Image pImg = pObj.AddComponent<Image>();
            pImg.sprite = _particleSprite;
            pImg.SetNativeSize();

            // Add a little randomness to size so it looks organic
            pObj.transform.localScale = Vector3.one * Random.Range(0.3f, 0.4f);

            RectTransform rt = pObj.GetComponent<RectTransform>();

            // Spawn at a random position inside the circle
            Vector2 randomStart = Random.insideUnitCircle * _spawnRadius;
            rt.anchoredPosition = randomStart;

            // Start completely invisible
            pImg.color = new Color(1, 1, 1, 0);

            // Target float position directly upwards
            Vector2 targetPos = randomStart + new Vector2(0, _floatDistance);

            // Tween the particle opacity and position
            Sequence seq = DOTween.Sequence();

            // Pop in quickly
            seq.Append(pImg.DOFade(1f, 0.2f));

            // Float up smoothly over the entire lifetime
            seq.Join(rt.DOAnchorPos(targetPos, _particleLifetime).SetEase(Ease.OutSine));

            // Start fading out halfway through its life
            seq.Insert(_particleLifetime * 0.5f, pImg.DOFade(0f, _particleLifetime * 0.5f).SetEase(Ease.InQuad));

            // Clean up the memory when it finishes
            seq.OnComplete(() => Destroy(pObj));
        }
    }
}
