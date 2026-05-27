using UnityEngine;

/// <summary>
/// Builds the whole "Game" scene at runtime:
/// player, camera follow, UI, spawner, and runtime "prefabs".
/// This keeps the project beginner-friendly AND ready-to-play.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("Game Tuning")]
    public float gameDurationSeconds = 180f;

    [Header("Player")]
    public float playerMoveSpeed = 6f;
    public int playerHP = 100;

    [Header("Weapon")]
    public int weaponDamage = 10;
    public float weaponFireRate = 2f;
    public float playerProjectileSpeed = 12f;

    [Header("Enemies")]
    public float meleeSpeed = 2.8f;
    public int meleeHP = 20;
    public int meleeContactDamage = 10;

    public float rangedSpeed = 2.2f;
    public int rangedHP = 16;
    public float rangedDesiredDistance = 6f;
    public float rangedStopDistance = 5f;
    public float rangedShootInterval = 1.6f;
    public int rangedProjectileDamage = 8;
    public float rangedProjectileSpeed = 7f;

    [Header("Spawning")]
    public float spawnRadius = 14f;
    public float minSpawnDistanceFromPlayer = 5f;
    public float startSpawnsPerSecond = 0.6f;
    public float endSpawnsPerSecond = 2.0f;

    private void Awake()
    {
        CleanupStarterSceneJunk();
        EnsureGameManager();

        // Arena background (simple dark square)
        CreateBackground();

        // Player + systems
        GameObject player = CreatePlayer();

        // Camera
        SetupCamera(player.transform);

        // UI
        UIManager ui = EnsureUIManager();
        ui.SetHP(playerHP, playerHP);

        // Enemy spawner + runtime templates
        CreateSpawner(player.transform);
    }

    private void EnsureGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.gameDurationSeconds = gameDurationSeconds;
            GameManager.Instance.mainMenuSceneName = "MainMenu";
            GameManager.Instance.gameSceneName = "Game";
            return;
        }

        GameObject gm = new GameObject("GameManager");
        GameManager mgr = gm.AddComponent<GameManager>();
        mgr.gameDurationSeconds = gameDurationSeconds;
        mgr.mainMenuSceneName = "MainMenu";
        mgr.gameSceneName = "Game";
    }

    private UIManager EnsureUIManager()
    {
        UIManager existing = FindFirstObjectByType<UIManager>();
        if (existing != null) return existing;

        GameObject ui = new GameObject("UIManager");
        return ui.AddComponent<UIManager>();
    }

    private void CleanupStarterSceneJunk()
    {
        // Leftover URP template objects can cause bloom (white wash) and missing-script warnings.
        DestroyObjectIfFound("Global Volume");
        DestroyObjectIfFound("Directional Light");
        DestroyObjectIfFound("Cube");
    }

    private void DestroyObjectIfFound(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        if (go != null) Destroy(go);
    }

    private void CreateBackground()
    {
        GameObject bg = new GameObject("Background");
        AutoSprite2D.AddTo(bg, new Color(0.08f, 0.08f, 0.12f, 1f), sortingOrder: -100);
        bg.transform.localScale = new Vector3(100f, 100f, 1f);
    }

    private GameObject CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = Vector3.zero;

        AutoSprite2D.AddTo(player, new Color(0.95f, 0.95f, 0.2f, 1f), sortingOrder: 10);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 6f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.isTrigger = false;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = playerMoveSpeed;
        pc.maxHP = playerHP;
        pc.currentHP = playerHP;

        XPManager xp = player.AddComponent<XPManager>();

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0.6f, 0f, 0f);

        WeaponController weapon = player.AddComponent<WeaponController>();
        weapon.damage = weaponDamage;
        weapon.fireRate = weaponFireRate;
        weapon.projectileSpeed = playerProjectileSpeed;
        weapon.firePoint = firePoint.transform;
        weapon.projectilePrefab = CreatePlayerProjectileTemplate().GetComponent<Projectile>();

        xp.weapon = weapon;

        return player;
    }

    private GameObject CreatePlayerProjectileTemplate()
    {
        GameObject t = new GameObject("PlayerBullet_Template");
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        AutoSprite2D.AddTo(t, new Color(0.75f, 1f, 0.95f, 1f), sortingOrder: 20);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.22f;

        Projectile p = t.AddComponent<Projectile>();
        p.lifetime = 2.5f;

        return t;
    }

    private GameObject CreateEnemyProjectileTemplate()
    {
        GameObject t = new GameObject("EnemyProjectile_Template");
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        AutoSprite2D.AddTo(t, new Color(1f, 0.55f, 0.65f, 1f), sortingOrder: 20);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.14f;

        Projectile p = t.AddComponent<Projectile>();
        p.lifetime = 3f;

        return t;
    }

    private XpGem CreateXpGemTemplate()
    {
        GameObject t = new GameObject("XpGem_Template");
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        AutoSprite2D.AddTo(t, new Color(0.4f, 1f, 0.55f, 1f), sortingOrder: 5);

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        XpGem gem = t.AddComponent<XpGem>();
        gem.xpAmount = 5;
        return gem;
    }

    private EnemyAI CreateMeleeEnemyTemplate(XpGem gemTemplate)
    {
        GameObject t = new GameObject("EnemyMelee_Template");
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        AutoSprite2D.AddTo(t, new Color(0.8f, 0.25f, 0.9f, 1f), sortingOrder: 9);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 0f;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.33f;

        EnemyAI e = t.AddComponent<EnemyAI>();
        e.maxHP = meleeHP;
        e.moveSpeed = meleeSpeed;
        e.contactDamage = meleeContactDamage;
        e.xpGemPrefab = gemTemplate;
        e.xpValue = 5;

        return e;
    }

    private RangedEnemyAI CreateRangedEnemyTemplate(XpGem gemTemplate, Projectile enemyProjectileTemplate)
    {
        GameObject t = new GameObject("EnemyRanged_Template");
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        AutoSprite2D.AddTo(t, new Color(1f, 0.6f, 0.2f, 1f), sortingOrder: 9);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 0f;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.33f;

        RangedEnemyAI e = t.AddComponent<RangedEnemyAI>();
        e.maxHP = rangedHP;
        e.moveSpeed = rangedSpeed;
        e.desiredDistance = rangedDesiredDistance;
        e.stopDistance = rangedStopDistance;
        e.shootInterval = rangedShootInterval;
        e.projectileDamage = rangedProjectileDamage;
        e.projectileSpeed = rangedProjectileSpeed;
        e.projectilePrefab = enemyProjectileTemplate;
        e.xpGemPrefab = gemTemplate;
        e.xpValue = 7;

        return e;
    }

    private void CreateSpawner(Transform player)
    {
        GameObject sp = new GameObject("EnemySpawner");
        EnemySpawner spawner = sp.AddComponent<EnemySpawner>();
        spawner.player = player;
        spawner.spawnRadius = spawnRadius;
        spawner.minSpawnDistanceFromPlayer = minSpawnDistanceFromPlayer;
        spawner.startSpawnsPerSecond = startSpawnsPerSecond;
        spawner.endSpawnsPerSecond = endSpawnsPerSecond;

        XpGem gemTemplate = CreateXpGemTemplate();
        GameObject enemyProjectileTemplateGO = CreateEnemyProjectileTemplate();
        Projectile enemyProjectileTemplate = enemyProjectileTemplateGO.GetComponent<Projectile>();

        spawner.meleeEnemyPrefab = CreateMeleeEnemyTemplate(gemTemplate);
        spawner.rangedEnemyPrefab = CreateRangedEnemyTemplate(gemTemplate, enemyProjectileTemplate);

        // Disabled templates won't appear in EnemyAI scans, chase the player from off-map, or run physics accidentally.
        // EnemySpawner & shooting code explicitly activates spawned instances.
        spawner.meleeEnemyPrefab.gameObject.SetActive(false);
        spawner.rangedEnemyPrefab.gameObject.SetActive(false);
        gemTemplate.gameObject.SetActive(false);
        enemyProjectileTemplateGO.SetActive(false);

        Vector3 hidden = new Vector3(99999f, 99999f, 0f);
        spawner.meleeEnemyPrefab.transform.position = hidden;
        spawner.rangedEnemyPrefab.transform.position = hidden;
        gemTemplate.transform.position = hidden;
        enemyProjectileTemplateGO.transform.position = hidden;
    }

    private void SetupCamera(Transform player)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject c = new GameObject("Main Camera");
            cam = c.AddComponent<Camera>();
            c.tag = "MainCamera";
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f, 1f);
        cam.transform.position = new Vector3(0f, 0f, -10f);

        CameraFollow2D follow = cam.GetComponent<CameraFollow2D>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow2D>();
        follow.target = player;
        follow.smoothTime = 0.12f;
    }

    // (UI creation removed: HUD is drawn by UIManager via OnGUI)
}

