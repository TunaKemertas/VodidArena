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
    public float startSpawnsPerSecond = 0.6f;
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
        if (GameManager.Instance != null &&
            (GameManager.Instance.IsGameOverOrWon || GameManager.Instance.IsPaused))
            return;

        if (player == null) return;
        if (meleeEnemyPrefab == null || rangedEnemyPrefab == null) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer > 0f) return;

        float t = 0f;
        if (GameManager.Instance != null)
            t = Mathf.Clamp01(Time.timeSinceLevelLoad / Mathf.Max(1f, GameManager.Instance.gameDurationSeconds));

        float spawnsPerSecond = Mathf.Lerp(startSpawnsPerSecond, endSpawnsPerSecond, t);
        float interval = 1f / Mathf.Max(0.05f, spawnsPerSecond);

        SpawnOne();
        _spawnTimer = interval;
    }

    private void SpawnOne()
    {
        Vector2 spawnPos = FindSpawnPosition();
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
        for (int i = 0; i < 12; i++)
        {
            Vector2 candidate = (Vector2)player.position +
                Random.insideUnitCircle.normalized * Random.Range(minSpawnDistanceFromPlayer, spawnRadius);
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
