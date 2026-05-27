using UnityEngine;

public class RangedEnemyAI : EnemyAI
{
    [Header("Ranged Behavior")]
    public float desiredDistance = 6f;
    public float stopDistance = 5f;
    public float retreatSpeedMultiplier = 1.1f;

    [Header("Ranged Attack")]
    public Projectile projectilePrefab;
    public float projectileSpeed = 7f;
    public int projectileDamage = 8;
    public float shootInterval = 1.5f;

    private float _shootTimer;

    protected override void Start()
    {
        base.Start();
        _shootTimer = Random.Range(0.2f, shootInterval); // desync shots a bit
    }

    protected override void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;
        if (_player == null) return;

        Vector2 toPlayer = (Vector2)_player.position - _rb.position;
        float dist = toPlayer.magnitude;

        // Keep distance: move closer if too far, retreat if too close, otherwise hover.
        if (dist > desiredDistance)
        {
            Vector2 dir = toPlayer.normalized;
            _rb.MovePosition(_rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
        else if (dist < stopDistance)
        {
            Vector2 dir = (-toPlayer).normalized;
            _rb.MovePosition(_rb.position + dir * (moveSpeed * retreatSpeedMultiplier) * Time.fixedDeltaTime);
        }
        // else: do nothing (stay in place)
    }

    protected override void Update()
    {
        base.Update();
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;
        if (_player == null) return;
        if (projectilePrefab == null) return;

        _shootTimer -= Time.deltaTime;
        if (_shootTimer > 0f) return;

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        Projectile p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        p.gameObject.SetActive(true);
        p.Initialize(dir, projectileSpeed, projectileDamage, Projectile.TargetType.Player);

        _shootTimer = shootInterval;
    }
}

