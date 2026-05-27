using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace VoidSurvivors.Weapons.FireShoes
{
    public class FireTrail : MonoBehaviour
    {
        public float lifetime = 1.4f;
        public int dps = 10;

        private readonly Dictionary<EnemyAI, float> _tick = new Dictionary<EnemyAI, float>();
        private CanvasGroup _cg;

        private void Awake()
        {
            _cg = gameObject.AddComponent<CanvasGroup>();
            _cg.alpha = 1f;
        }

        private void Start()
        {
            // Fade out near the end.
            float fadeStart = Mathf.Max(0.05f, lifetime - 0.35f);
            _cg.DOFade(0f, lifetime - fadeStart).SetDelay(fadeStart).SetEase(Ease.OutQuad).SetUpdate(true)
                .OnComplete(() => Destroy(gameObject));
            Destroy(gameObject, lifetime + 0.1f);
        }

        private void Update()
        {
            if (_tick.Count == 0) return;
            var keys = new List<EnemyAI>(_tick.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                EnemyAI e = keys[i];
                if (e == null)
                {
                    _tick.Remove(e);
                    continue;
                }

                _tick[e] -= Time.deltaTime;
                if (_tick[e] <= 0f) _tick.Remove(e);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            EnemyAI e = other.GetComponent<EnemyAI>();
            if (e == null) return;
            if (_tick.ContainsKey(e)) return;

            // Apply damage in small ticks (4 per second).
            int tickDmg = Mathf.CeilToInt(dps * 0.25f);
            e.TakeDamage(tickDmg);
            _tick[e] = 0.25f;
        }
    }
}

