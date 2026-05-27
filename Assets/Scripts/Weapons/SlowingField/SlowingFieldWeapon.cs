using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivors.Weapons.SlowingField
{
    public class SlowingFieldWeapon : MonoBehaviour
    {
        [Header("Base Tuning")]
        public float baseRadius = 2.3f;
        public float radiusPerLevel = 0.55f;
        public float baseSpeedMultiplier = 0.8f;
        public float multiplierPerLevel = 0.05f;

        [Header("Runtime")]
        [Range(1, 5)] public int level = 1;

        private CircleCollider2D _col;
        private readonly HashSet<EnemyAI> _inside = new HashSet<EnemyAI>();
        private float _currentMultiplier = 0.8f;

        private void Awake()
        {
            // Visual ring
            GameObject ring = new GameObject("SlowingField_Ring");
            ring.transform.SetParent(transform, false);
            AutoSprite2D.AddTo(ring, new Color(0.35f, 0.75f, 1f, 0.22f), sortingOrder: 6);
            ring.transform.localScale = Vector3.one * 4f;

            _col = gameObject.AddComponent<CircleCollider2D>();
            _col.isTrigger = true;

            ApplyLevelStats();
            UpdateRingScale(ring.transform);
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Clamp(newLevel, 1, 5);
            ApplyLevelStats();

            Transform ring = transform.Find("SlowingField_Ring");
            if (ring != null) UpdateRingScale(ring);

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

        private void UpdateRingScale(Transform ring)
        {
            // Sprite is a 1x1 quad; scale roughly to collider diameter.
            float diameter = _col.radius * 2f;
            ring.localScale = new Vector3(diameter, diameter, 1f);
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

