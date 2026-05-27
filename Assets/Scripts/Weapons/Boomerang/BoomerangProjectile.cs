using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivors.Weapons.Boomerang
{
    public class BoomerangProjectile : MonoBehaviour
    {
        public float travelSeconds = 0.7f;
        public float returnSeconds = 0.7f;
        public float curveAmount = 0.8f;
        public int damage = 10;

        private Transform _owner;
        private Vector2 _start;
        private Vector2 _end;
        private Vector2 _perp;
        private float _t;
        private readonly Dictionary<EnemyAI, float> _hitCd = new Dictionary<EnemyAI, float>();

        public void Initialize(Transform owner, Vector2 dir, float distance, int dmg, float curve)
        {
            _owner = owner;
            _start = owner != null ? (Vector2)owner.position : (Vector2)transform.position;
            _end = _start + dir.normalized * Mathf.Max(0.2f, distance);
            _perp = new Vector2(-dir.y, dir.x).normalized;
            damage = dmg;
            curveAmount = curve;

            transform.position = _start;
            _t = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
                return;

            // Hit cooldown bookkeeping
            if (_hitCd.Count > 0)
            {
                var keys = new List<EnemyAI>(_hitCd.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    EnemyAI e = keys[i];
                    if (e == null)
                    {
                        _hitCd.Remove(e);
                        continue;
                    }
                    _hitCd[e] -= Time.deltaTime;
                    if (_hitCd[e] <= 0f) _hitCd.Remove(e);
                }
            }

            float total = Mathf.Max(0.05f, travelSeconds + returnSeconds);
            _t += Time.deltaTime / total;

            Vector2 a, b;
            float phase;
            if (_t <= travelSeconds / total)
            {
                phase = _t / (travelSeconds / total);
                a = _start;
                b = _end;
            }
            else
            {
                phase = (_t - (travelSeconds / total)) / (returnSeconds / total);
                a = _end;
                b = _owner != null ? (Vector2)_owner.position : _start;
            }

            Vector2 pos = Vector2.Lerp(a, b, Mathf.Clamp01(phase));
            float wave = Mathf.Sin(Mathf.Clamp01(phase) * Mathf.PI);
            pos += _perp * (wave * curveAmount);
            transform.position = pos;

            // End when returned.
            if (_t >= 1f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnemyAI e = other.GetComponent<EnemyAI>();
            if (e == null) return;
            if (_hitCd.ContainsKey(e)) return;

            e.TakeDamage(damage);
            _hitCd[e] = 0.18f;
        }
    }
}

