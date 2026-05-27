using UnityEngine;

namespace VoidSurvivors.Weapons.FireShoes
{
    public class FireShoesWeapon : MonoBehaviour
    {
        [Header("Trail Spawn")]
        public float spawnInterval = 0.14f;

        [Header("Trail Stats")]
        public float baseLifetime = 1.2f;
        public float lifetimePerLevel = 0.25f;
        public int baseDps = 10;
        public int dpsPerLevel = 4;

        [Header("Runtime")]
        [Range(1, 5)] public int level = 1;

        private Vector2 _lastPos;
        private float _timer;

        private void Start()
        {
            _lastPos = transform.position;
            _timer = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
                return;

            Vector2 pos = transform.position;
            _lastPos = pos;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;
            _timer = spawnInterval;

            SpawnTrail(pos);
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Clamp(newLevel, 1, 5);
        }

        private void SpawnTrail(Vector2 pos)
        {
            GameObject go = new GameObject("FireTrail");
            go.transform.position = pos;
            AutoSprite2D.AddTo(go, new Color(1f, 0.35f, 0.1f, 0.7f), sortingOrder: 7);
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;

            FireTrail t = go.AddComponent<FireTrail>();
            t.lifetime = baseLifetime + lifetimePerLevel * (level - 1);
            t.dps = baseDps + dpsPerLevel * (level - 1);
        }
    }
}

