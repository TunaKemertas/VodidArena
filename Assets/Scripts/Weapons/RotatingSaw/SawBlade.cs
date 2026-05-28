using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace VoidSurvivors.Weapons.RotatingSaw
{
    public class SawBlade : MonoBehaviour
    {
        public int damage = 8;
        public float hitCooldown = 0.25f;

        private readonly Dictionary<EnemyAI, float> _cooldowns = new Dictionary<EnemyAI, float>();
        private bool _active;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        public void SetActive(bool active)
        {
            _active = active;
            gameObject.SetActive(true);
            if (_baseScale == Vector3.zero)
                _baseScale = transform.localScale;

            // Simple visible feedback
            transform.DOKill();
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (active)
            {
                GetComponent<Collider2D>().enabled = true;
                if (sr != null) sr.color = Color.white;
                transform.localScale = _baseScale * 0.75f;
                transform.DOScale(_baseScale, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
            }
            else
            {
                GetComponent<Collider2D>().enabled = false;
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.35f);
                transform.DOScale(_baseScale * 0.75f, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true);
            }
        }

        private void Update()
        {
            if (!_active) return;

            // Cooldown tracking per enemy so we don't melt everything instantly.
            if (_cooldowns.Count == 0) return;

            var keys = new List<EnemyAI>(_cooldowns.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                EnemyAI e = keys[i];
                if (e == null)
                {
                    _cooldowns.Remove(e);
                    continue;
                }

                _cooldowns[e] -= Time.deltaTime;
                if (_cooldowns[e] <= 0f)
                    _cooldowns.Remove(e);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void TryDamage(Collider2D other)
        {
            if (!_active) return;

            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy == null) return;

            if (_cooldowns.ContainsKey(enemy)) return;

            enemy.TakeDamage(damage);
            _cooldowns[enemy] = hitCooldown;
        }
    }
}

