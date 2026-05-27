using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 20;
    public float moveSpeed = 2.8f;
    public int contactDamage = 10;
    public float contactDamageCooldown = 0.5f;

    [Header("Drops")]
    public XpGem xpGemPrefab;
    public int xpValue = 5;

    protected int _hp;
    protected Rigidbody2D _rb;
    protected Transform _player;
    private float _contactCooldownTimer;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _hp = maxHP;
    }

    protected virtual void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        _player = pc != null ? pc.transform : null;
    }

    protected virtual void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;
        if (_player == null) return;

        Vector2 dir = ((Vector2)_player.position - _rb.position).normalized;
        Vector2 targetPos = _rb.position + dir * moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(targetPos);
    }

    protected virtual void Update()
    {
        if (_contactCooldownTimer > 0f)
            _contactCooldownTimer -= Time.deltaTime;
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        _hp = Mathf.Max(0, _hp - amount);

        if (_hp <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (xpGemPrefab != null)
        {
            XpGem gem = Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
            gem.gameObject.SetActive(true);
            gem.xpAmount = xpValue;
        }
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (_contactCooldownTimer > 0f) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        player.TakeDamage(contactDamage);
        _contactCooldownTimer = contactDamageCooldown;
    }
}

