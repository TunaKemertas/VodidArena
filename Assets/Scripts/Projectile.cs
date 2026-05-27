using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public enum TargetType
    {
        Enemy = 0,
        Player = 1
    }

    public float lifetime = 3f;

    private Rigidbody2D _rb;
    private Vector2 _dir;
    private float _speed;
    private int _damage;
    private TargetType _targetType;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float speed, int damage, TargetType targetType)
    {
        _dir = direction.normalized;
        _speed = speed;
        _damage = damage;
        _targetType = targetType;

        gameObject.SetActive(true);
        // Fast trigger bullets can tunnel through enemies without continuous collision.
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;
        if (_rb == null || _speed <= 0f) return;

        _rb.MovePosition(_rb.position + _dir * _speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_targetType == TargetType.Enemy)
        {
            EnemyAI enemy = other.GetComponent<EnemyAI>();
            if (enemy == null) return;
            enemy.TakeDamage(_damage);
        }
        else
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;
            player.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
}

