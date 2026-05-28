using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivors.Weapons.SlowingField
{
    public class SlowingFieldWeapon : MonoBehaviour
    {
        [Header("Base Tuning")]
        public float baseRadius = 1.15f;
        public float radiusPerLevel = 0.275f;
        public float baseSpeedMultiplier = 0.8f;
        public float multiplierPerLevel = 0.05f;

        [Header("Runtime")]
        [Range(1, 5)] public int level = 1;

        private CircleCollider2D _col;
        private Transform _ring;
        private readonly HashSet<EnemyAI> _inside = new HashSet<EnemyAI>();
        private float _currentMultiplier = 0.8f;

        private void Awake()
        {
            // Child aura: keeps the large trigger OFF the player object, so enemy projectiles cannot hit it as if it was the player.
            GameObject aura = new GameObject("SlowingField_Aura");
            aura.transform.SetParent(transform, false);
            aura.transform.localPosition = Vector3.zero;

            _col = aura.AddComponent<CircleCollider2D>();
            _col.isTrigger = true;

            AutoSprite2D.AddTo(aura, new Color(0.35f, 0.75f, 1f, 0.06f), sortingOrder: 6);
            _ring = aura.transform;

            ApplyLevelStats();
            UpdateRingScale();
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Clamp(newLevel, 1, 5);
            ApplyLevelStats();

            UpdateRingScale();

            // Re-apply slow to all inside (multiplier changes with level)
            foreach (EnemyAI e in _inside)
            {
                if (e != null) e.SetSpeedMultiplier(_currentMultiplier);
            }
        }

        private void ApplyLevelStats()
        {
            float radius = baseRadius + radiusPerLevel * (level - 1);
            _col.radius = radius;

            _currentMultiplier = Mathf.Clamp(baseSpeedMultiplier - multiplierPerLevel * (level - 1), 0.55f, 0.95f);
        }

        private void UpdateRingScale()
        {
            if (_ring == null) return;
            // Keep visual smaller/subtler than the gameplay collider so it does not tint the whole arena.
            float diameter = _col.radius * 1.25f;
            _ring.localScale = new Vector3(diameter, diameter, 1f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnemyAI e = other.GetComponent<EnemyAI>();
            if (e == null) return;
            _inside.Add(e);
            e.SetSpeedMultiplier(_currentMultiplier);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            EnemyAI e = other.GetComponent<EnemyAI>();
            if (e == null) return;
            _inside.Remove(e);
            e.ClearSpeedMultiplier();
        }
    }
}

