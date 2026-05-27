using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Targeting")]
    public float targetSearchRadius = 30f;

    [Header("Weapon Stats")]
    public int damage = 10;
    [Tooltip("Shots per second. Example: 0.75 ≈ one shot every 1.33s")]
    public float fireRate = 0.75f;
    public float projectileSpeed = 12f;

    [Header("Prefabs")]
    public Projectile projectilePrefab;
    public Transform firePoint;

    private float _cooldownTimer;

    private void Update()
    {
        if (GameManager.Instance != null &&
            (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
            return;

        if (projectilePrefab == null || firePoint == null) return;

        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer > 0f) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Shoot(target.position);
        _cooldownTimer = 1f / Mathf.Max(0.01f, fireRate);
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
            if (d > targetSearchRadius) continue;

            if (d < bestDist)
            {
                bestDist = d;
                best = e.transform;
            }
        }

        return best;
    }

    private void Shoot(Vector3 targetPos)
    {
        Vector2 dir = (targetPos - firePoint.position).normalized;
        Projectile p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        p.gameObject.SetActive(true);
        p.Initialize(dir, projectileSpeed, damage, Projectile.TargetType.Enemy);
    }

    public void ApplyLevelUpBonus()
    {
        damage += 2;
        fireRate += 0.12f;
    }
}
