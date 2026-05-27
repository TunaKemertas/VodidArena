using UnityEngine;

namespace VoidSurvivors.Weapons.Boomerang
{
    public class BoomerangWeapon : MonoBehaviour
    {
        [Header("Tuning")]
        public float throwInterval = 1.35f;
        public float baseDistance = 5.5f;
        public float distancePerLevel = 0.6f;
        public int damage = 10;

        [Header("Runtime")]
        [Range(1, 5)] public int level = 1;

        private float _timer;

        private void Start()
        {
            _timer = Random.Range(0.1f, throwInterval);
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
                return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = throwInterval;

            Transform target = FindNearestEnemy();
            if (target == null) return;

            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            int count = Mathf.Clamp(level, 1, 5);
            float spread = count == 1 ? 0f : 18f;

            float distance = baseDistance + distancePerLevel * (level - 1);
            float curve = 0.55f + 0.12f * (level - 1);

            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                float angle = Mathf.Lerp(-spread, spread, t);
                Vector2 d = Rotate(dir, angle);
                SpawnBoomerang(d, distance, curve);
            }
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

        private void SpawnBoomerang(Vector2 dir, float distance, float curve)
        {
            GameObject go = new GameObject("Boomerang");
            go.transform.position = transform.position;
            AutoSprite2D.AddTo(go, new Color(0.3f, 1f, 0.75f, 1f), sortingOrder: 21);
            go.transform.localScale = new Vector3(0.36f, 0.14f, 1f);

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.14f;

            BoomerangProjectile p = go.AddComponent<BoomerangProjectile>();
            p.damage = damage + (level - 1) * 2;
            p.curveAmount = curve;
            p.travelSeconds = 0.65f;
            p.returnSeconds = 0.65f;
            p.Initialize(transform, dir, distance, p.damage, curve);
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float a = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(a);
            float s = Mathf.Sin(a);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
    }
}

