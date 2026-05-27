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

    // Used by Slowing Field (and any future slow effects).
    private float _speedMultiplier = 1f;
    protected float EffectiveMoveSpeed => moveSpeed * _speedMultiplier;

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
        if (GameManager.Instance != null &&
            (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
            return;

        if (_player == null) return;

        Vector2 dir = ((Vector2)_player.position - _rb.position).normalized;
        _rb.MovePosition(_rb.position + dir * EffectiveMoveSpeed * Time.fixedDeltaTime);
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
        if (_hp <= 0) Die();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        // Keep it simple: single multiplier, clamped so enemies never fully freeze.
        _speedMultiplier = Mathf.Clamp(multiplier, 0.2f, 2.5f);
    }

    public void ClearSpeedMultiplier()
    {
        _speedMultiplier = 1f;
    }

    protected virtual void Die()
    {
        if (xpGemPrefab != null)
        {
            XpGem gem = Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
            gem.gameObject.SetActive(true);
            gem.xpAmount = xpValue;
        }

        TrySpawnExtraDrop();
        Destroy(gameObject);
    }

    private void TrySpawnExtraDrop()
    {
        Vector3 pos = transform.position;

        // Heart: rare
        if (Random.value < 0.06f)
        {
            SpawnHeart(pos);
            return;
        }

        // Magnet: even rarer than heart
        if (Random.value < 0.02f)
            SpawnMagnet(pos);
    }

    private void SpawnHeart(Vector3 pos)
    {
        GameObject go = new GameObject("HeartPickup");
        go.transform.position = pos;
        AutoSprite2D.AddTo(go, new Color(1f, 0.28f, 0.35f, 1f), sortingOrder: 12);
        go.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.24f;

        HeartPickup hp = go.AddComponent<HeartPickup>();
        hp.healPercent = 0.25f;
    }

    private void SpawnMagnet(Vector3 pos)
    {
        GameObject go = new GameObject("MagnetPickup");
        go.transform.position = pos;
        AutoSprite2D.AddTo(go, new Color(0.35f, 0.85f, 1f, 1f), sortingOrder: 12);
        go.transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.22f;

        go.AddComponent<MagnetPickup>();
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
