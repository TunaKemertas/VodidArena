using UnityEngine;

namespace VoidSurvivors.Weapons.RocketLauncher
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class RocketProjectile : MonoBehaviour
    {
        public float lifetime = 3f;

        private Rigidbody2D _rb;
        private Vector2 _dir;
        private float _speed;
        private int _damage;
        private float _explosionRadius;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Vector2 direction, float speed, int damage, float explosionRadius)
        {
            _dir = direction.normalized;
            _speed = speed;
            _damage = damage;
            _explosionRadius = explosionRadius;

            gameObject.SetActive(true);
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            if (GameManager.Instance != null &&
                (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
                return;

            _rb.MovePosition(_rb.position + _dir * _speed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy == null) return;

            Explode();
        }

        private void Explode()
        {
            Vector2 pos = transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, Mathf.Max(0.01f, _explosionRadius));
            if (hits != null)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    EnemyAI e = hits[i] != null ? hits[i].GetComponent<EnemyAI>() : null;
                    if (e != null)
                        e.TakeDamage(_damage);
                }
            }

            RocketVfx.SpawnExplosion(pos, _explosionRadius);
            Destroy(gameObject);
        }
    }
}

