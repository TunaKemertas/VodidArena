using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivors.Weapons.RotatingSaw
{
    public class RotatingSawWeapon : MonoBehaviour
    {
        [Header("Orbit")]
        public float orbitRadius = 5f;
        public float orbitSpeedDegrees = 180f;

        [Header("Activation Cycle")]
        public float activeSeconds = 2.8f;
        public float inactiveSeconds = 1.2f;

        [Header("Damage")]
        public int damage = 8;

        [Header("Runtime")]
        [Range(1, 5)] public int level = 1;

        private readonly List<Transform> _blades = new List<Transform>();
        private float _angle;
        private float _cycleTimer;
        private bool _active;

        private void Start()
        {
            RebuildBlades();
            SetActiveState(true);
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
                return;

            TickCycle();
            TickOrbit();
        }

        public void SetLevel(int newLevel)
        {
            int clamped = Mathf.Clamp(newLevel, 1, 5);
            if (level == clamped) return;
            level = clamped;
            RebuildBlades();
        }

        private void TickCycle()
        {
            _cycleTimer -= Time.deltaTime;
            if (_cycleTimer > 0f) return;

            if (_active)
                SetActiveState(false);
            else
                SetActiveState(true);
        }

        private void SetActiveState(bool active)
        {
            _active = active;
            _cycleTimer = active ? Mathf.Max(0.1f, activeSeconds) : Mathf.Max(0.1f, inactiveSeconds);

            for (int i = 0; i < _blades.Count; i++)
            {
                SawBlade sb = _blades[i] != null ? _blades[i].GetComponent<SawBlade>() : null;
                if (sb != null)
                {
                    sb.damage = damage;
                    sb.SetActive(active);
                }
            }
        }

        private void TickOrbit()
        {
            if (_blades.Count == 0) return;

            _angle += orbitSpeedDegrees * Time.deltaTime;
            float step = 360f / _blades.Count;

            for (int i = 0; i < _blades.Count; i++)
            {
                Transform t = _blades[i];
                if (t == null) continue;

                float a = (_angle + i * step) * Mathf.Deg2Rad;
                Vector3 local = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * orbitRadius;
                t.localPosition = local;
                t.localRotation = Quaternion.Euler(0f, 0f, -_angle * 2f);
            }
        }

        private void RebuildBlades()
        {
            int want = Mathf.Clamp(level, 1, 5);

            // Add missing
            while (_blades.Count < want)
            {
                _blades.Add(CreateBlade(_blades.Count));
            }

            // Disable extras (shouldn't happen often, but keep safe)
            for (int i = 0; i < _blades.Count; i++)
            {
                if (_blades[i] == null) continue;
                _blades[i].gameObject.SetActive(i < want);
            }
        }

        private Transform CreateBlade(int index)
        {
            GameObject go = new GameObject($"SawBlade_{index + 1}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.right * orbitRadius;

            AutoSprite2D.AddTo(go, new Color(0.75f, 0.75f, 0.8f, 1f), sortingOrder: 18);
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.28f;

            SawBlade sb = go.AddComponent<SawBlade>();
            sb.damage = damage;
            sb.hitCooldown = 0.25f;
            sb.SetActive(true);

            return go.transform;
        }
    }
}

