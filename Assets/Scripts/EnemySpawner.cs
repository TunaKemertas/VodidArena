using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Enemy Prefabs")]
    public EnemyAI meleeEnemyPrefab;
    public RangedEnemyAI rangedEnemyPrefab;

    [Header("Spawn Rules")]
    public float spawnRadius = 14f;
    public float minSpawnDistanceFromPlayer = 5f;

    [Header("Difficulty Scaling")]
    [Tooltip("Spawns per second at the start. Example 0.6 = one spawn every ~1.67s")]
    public float startSpawnsPerSecond = 0.6f;
    [Tooltip("How many spawns per second after 3 minutes (end of match).")]
    public float endSpawnsPerSecond = 2.0f;

    private bool _spawning = true;
    private float _spawnTimer;

    private void Start()
    {
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            player = pc != null ? pc.transform : null;
        }

        _spawnTimer = 0.5f;
    }

    private void Update()
    {
        if (!_spawning) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOverOrWon) return;
        if (player == null) return;
        if (meleeEnemyPrefab == null || rangedEnemyPrefab == null) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer > 0f) return;

        float t = 0f;
        if (GameManager.Instance != null)
            t = Mathf.Clamp01(GetElapsedNormalized());

        float spawnsPerSecond = Mathf.Lerp(startSpawnsPerSecond, endSpawnsPerSecond, t);
        float interval = 1f / Mathf.Max(0.05f, spawnsPerSecond);

        SpawnOne();
        _spawnTimer = interval;
    }

    private float GetElapsedNormalized()
    {
        // We don't expose elapsed directly, so we scale based on timer UI values:
        // We'll approximate using Time.timeSinceLevelLoad relative to GameManager duration.
        float duration = (GameManager.Instance != null) ? GameManager.Instance.gameDurationSeconds : 180f;
        return Mathf.Clamp01(Time.timeSinceLevelLoad / Mathf.Max(1f, duration));
    }

    private void SpawnOne()
    {
        Vector2 spawnPos = FindSpawnPosition();

        // 70% melee, 30% ranged (simple mix).
        bool spawnRanged = Random.value < 0.3f;
        if (spawnRanged)
        {
            RangedEnemyAI e = Instantiate(rangedEnemyPrefab, spawnPos, Quaternion.identity);
            e.gameObject.SetActive(true);
        }
        else
        {
            EnemyAI e = Instantiate(meleeEnemyPrefab, spawnPos, Quaternion.identity);
            e.gameObject.SetActive(true);
        }
    }

    private Vector2 FindSpawnPosition()
    {
        // Try a few random positions; if all fail, just use a point on the circle.
        for (int i = 0; i < 12; i++)
        {
            Vector2 candidate = (Vector2)player.position + Random.insideUnitCircle.normalized * Random.Range(minSpawnDistanceFromPlayer, spawnRadius);
            if (Vector2.Distance(candidate, player.position) >= minSpawnDistanceFromPlayer)
                return candidate;
        }

        return (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;
    }

    public void StopSpawning()
    {
        _spawning = false;
    }
}

