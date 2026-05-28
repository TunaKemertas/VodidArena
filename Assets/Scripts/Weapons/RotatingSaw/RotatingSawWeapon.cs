using System.Collections.Generic;
using UnityEngine;

namespace VoidSurvivors.Weapons.RotatingSaw
{
    public class RotatingSawWeapon : MonoBehaviour
    {
        [Header("Orbit")]
        public float orbitRadius = 3.2f;
        public float orbitSpeedDegrees = 180f;

        [Header("Activation Cycle")]
        public float activeSeconds = 2.8f;
        public float inactiveSeconds = 1.2f;

        [Header("Damage")]
        public int damage = 8;

        [Header("Visual")]
        public float bladeScale = 7f;

        [Header("Runtime")]
        [Range(1, 5)] public int level = 1;

        private const string SawBladePrefabPath = "Weapons/SawBladeShuriken";
        private readonly List<Transform> _blades = new List<Transform>();
        private GameObject _bladePrefab;
        private float _angle;
        private float _cycleTimer;
        private bool _active;

        private void Start()
        {
            _bladePrefab = Resources.Load<GameObject>(SawBladePrefabPath);
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

        private void OnDestroy()
        {
            for (int i = 0; i < _blades.Count; i++)
            {
                if (_blades[i] != null)
                    Destroy(_blades[i].gameObject);
            }
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
                Vector3 offset = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * orbitRadius;
                // World-space orbit: do not inherit the player's scale, otherwise blades fly off-screen.
                t.position = transform.position + offset;
                t.rotation = Quaternion.Euler(0f, 0f, -_angle * 2f);
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
            GameObject go = _bladePrefab != null
                ? Instantiate(_bladePrefab)
                : new GameObject($"SawBlade_{index + 1}");

            go.name = $"SawBlade_{index + 1}";
            go.transform.SetParent(null, true);
            go.transform.position = transform.position + Vector3.right * orbitRadius;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * bladeScale;

            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.sortingOrder = 18;
                sr.color = Color.white;
            }
            else
                AutoSprite2D.AddTo(go, new Color(0.75f, 0.75f, 0.8f, 1f), sortingOrder: 18);

            CircleCollider2D col = go.GetComponent<CircleCollider2D>();
            if (col == null) col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.08f;

            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb == null) rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            SawBlade sb = go.GetComponent<SawBlade>();
            if (sb == null) sb = go.AddComponent<SawBlade>();
            sb.damage = damage;
            sb.hitCooldown = 0.25f;
            sb.SetActive(true);

            return go.transform;
        }
    }
}

