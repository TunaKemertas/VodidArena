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

        public void SetActive(bool active)
        {
            _active = active;
            gameObject.SetActive(true);

            // Simple visible feedback
            transform.DOKill();
            if (active)
            {
                GetComponent<Collider2D>().enabled = true;
                transform.localScale = Vector3.one * 0.15f;
                transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack).SetUpdate(true);
            }
            else
            {
                GetComponent<Collider2D>().enabled = false;
                transform.DOScale(0.15f, 0.12f).SetEase(Ease.OutQuad).SetUpdate(true);
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

        private void OnTriggerStay2D(Collider2D other)
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

