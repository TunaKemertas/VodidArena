using UnityEngine;

namespace VoidSurvivors.Weapons.RocketLauncher
{
    public class RocketLauncherWeapon : MonoBehaviour
    {
        [Header("Tuning")]
        [Tooltip("Shots per second (slower than main gun).")]
        public float fireRate = 0.35f;
        public float projectileSpeed = 6f;
        public int damage = 18;
        public float explosionRadius = 1.2f;

        [Header("Runtime")]
        [Range(1, 5)] public int level = 1;

        private float _cooldown;
        private Transform _firePoint;

        private void Awake()
        {
            // Reuse player's FirePoint if present; otherwise shoot from player center.
            Transform fp = transform.Find("FirePoint");
            _firePoint = fp != null ? fp : transform;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
                return;

            _cooldown -= Time.deltaTime;
            if (_cooldown > 0f) return;

            Transform target = FindNearestEnemy();
            if (target == null) return;

            FireAt(target.position);
            _cooldown = 1f / Mathf.Max(0.05f, fireRate);
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Clamp(newLevel, 1, 5);
        }

        private Transform FindNearestEnemy()
        {
            EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            if (enemies == null || enemies.Length == 0) return null;

            Transform best = null;
            float bestDist = float.PositiveInfinity;
            Vector3 pos = transform.position;
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyAI e = enemies[i];
                if (e == null || !e.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(pos, e.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = e.transform;
                }
            }
            return best;
        }

        private void FireAt(Vector3 targetPos)
        {
            int rockets = Mathf.Clamp(level, 1, 4);
            float spreadDeg = rockets <= 1 ? 0f : 12f;

            Vector2 baseDir = ((Vector2)targetPos - (Vector2)_firePoint.position).normalized;
            float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;

            float radius = explosionRadius + (level >= 5 ? 0.6f : 0f);
            int dmg = damage + (level >= 5 ? 10 : 0);

            for (int i = 0; i < rockets; i++)
            {
                float t = rockets == 1 ? 0.5f : i / (float)(rockets - 1);
                float angle = baseAngle + Mathf.Lerp(-spreadDeg, spreadDeg, t);
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                SpawnRocket(_firePoint.position, dir, dmg, radius);
            }
        }

        private void SpawnRocket(Vector3 pos, Vector2 dir, int dmg, float radius)
        {
            GameObject go = new GameObject("Rocket");
            go.transform.position = pos;
            AutoSprite2D.AddTo(go, new Color(1f, 0.85f, 0.3f, 1f), sortingOrder: 22);
            go.transform.localScale = new Vector3(0.45f, 0.2f, 1f);

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.bodyType = RigidbodyType2D.Kinematic;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.12f;

            RocketProjectile rp = go.AddComponent<RocketProjectile>();
            rp.lifetime = 3.2f;
            rp.Initialize(dir, projectileSpeed, dmg, radius);
        }
    }
}

