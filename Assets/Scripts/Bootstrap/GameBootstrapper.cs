using UnityEngine;
using VoidSurvivors.Weapons;

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
    public int playerHP = 55;

    [Header("Weapon")]
    public int weaponDamage = 10;
    [Tooltip("Shots per second (lower = slower).")]
    public float weaponFireRate = 0.75f;
    public float playerProjectileSpeed = 12f;

    [Header("Enemies")]
    public float meleeSpeed = 3.1f;
    public int meleeHP = 20;
    public int meleeContactDamage = 14;
    public float meleeContactCooldown = 0.38f;

    public float rangedSpeed = 2.35f;
    public int rangedHP = 16;
    public float rangedDesiredDistance = 6f;
    public float rangedStopDistance = 5f;
    public float rangedShootInterval = 1.25f;
    public int rangedProjectileDamage = 11;
    public float rangedProjectileSpeed = 7f;

    [Header("Spawning")]
    public float spawnRadius = 14f;
    public float minSpawnDistanceFromPlayer = 5f;
    public float startSpawnsPerSecond = 0.45f;
    public float endSpawnsPerSecond = 1.45f;

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
        GameObject floor0 = RuntimePrefabLoader.LoadPrefab("FloorPrefab0");
        GameObject floor1 = RuntimePrefabLoader.LoadPrefab("FloorPrefab1");

        if (floor0 == null || floor1 == null)
        {
            GameObject bg = new GameObject("Background");
            AutoSprite2D.AddTo(bg, new Color(0.08f, 0.08f, 0.12f, 1f), sortingOrder: -100);
            bg.transform.localScale = new Vector3(150f, 150f, 1f);
            return;
        }

        GameObject root = new GameObject("MixedFloor");
        const int halfTiles = 18;
        const float tileSize = 1.5f;
        const float spriteScale = 9.375f;

        for (int x = -halfTiles; x <= halfTiles; x++)
        {
            for (int y = -halfTiles; y <= halfTiles; y++)
            {
                GameObject prefab = ((x + y) & 1) == 0 ? floor0 : floor1;
                GameObject tile = Instantiate(prefab, new Vector3(x * tileSize, y * tileSize, 1f), Quaternion.identity);
                tile.name = $"Floor_{x}_{y}";
                tile.transform.SetParent(root.transform, true);
                tile.transform.localScale = Vector3.one * spriteScale;
                SetSortingOrder(tile, -100);
            }
        }
    }

    private GameObject CreatePlayer()
    {
        GameObject playerPrefab = RuntimePrefabLoader.LoadPrefab("CharacterPrefab");
        GameObject player = playerPrefab != null ? Instantiate(playerPrefab) : new GameObject("Player");
        player.name = "Player";
        player.transform.position = Vector3.zero;
        player.transform.localScale = Vector3.one * 5.25f;

        if (player.GetComponent<SpriteRenderer>() != null)
            SetSortingOrder(player, 10);
        else
            AutoSprite2D.AddTo(player, new Color(0.95f, 0.95f, 0.2f, 1f), sortingOrder: 10);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 6f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = player.AddComponent<CircleCollider2D>();
        col.radius = 0.0389f;
        col.isTrigger = false;

        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = playerMoveSpeed;
        pc.maxHP = playerHP;
        pc.currentHP = playerHP;

        XPManager xp = player.AddComponent<XPManager>();

        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        // Spawn shots from the character center. With scaled sprites, side offsets become too large and make aim feel inaccurate.
        firePoint.transform.localPosition = Vector3.zero;

        WeaponController weapon = player.AddComponent<WeaponController>();
        weapon.damage = weaponDamage;
        weapon.fireRate = weaponFireRate;
        weapon.projectileSpeed = playerProjectileSpeed;
        weapon.firePoint = firePoint.transform;
        weapon.projectilePrefab = CreatePlayerProjectileTemplate().GetComponent<Projectile>();

        WeaponManager weaponManager = player.AddComponent<WeaponManager>();
        weaponManager.mainGun = weapon;

        xp.weapon = weapon;
        xp.weaponManager = weaponManager;

        return player;
    }

    private GameObject CreatePlayerProjectileTemplate()
    {
        GameObject prefab = RuntimePrefabLoader.LoadPrefab("McProjectilePrefab");
        GameObject t = prefab != null ? Instantiate(prefab) : new GameObject("PlayerBullet_Template");
        t.name = "PlayerBullet_Template";
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        if (t.GetComponent<SpriteRenderer>() != null)
            SetSortingOrder(t, 20);
        else
            AutoSprite2D.AddTo(t, new Color(0.75f, 1f, 0.95f, 1f), sortingOrder: 20);
        t.transform.localScale = new Vector3(3.6f, 3.6f, 1f);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.0156f;

        Projectile p = t.AddComponent<Projectile>();
        p.lifetime = 2.5f;

        return t;
    }

    private GameObject CreateEnemyProjectileTemplate()
    {
        GameObject prefab = RuntimePrefabLoader.LoadPrefab("EnemyProjectilePrefab");
        GameObject t = prefab != null ? Instantiate(prefab) : new GameObject("EnemyProjectile_Template");
        t.name = "EnemyProjectile_Template";
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        if (t.GetComponent<SpriteRenderer>() != null)
            SetSortingOrder(t, 20);
        else
            AutoSprite2D.AddTo(t, new Color(1f, 0.55f, 0.65f, 1f), sortingOrder: 20);
        t.transform.localScale = new Vector3(3.3f, 3.3f, 1f);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.0111f;

        Projectile p = t.AddComponent<Projectile>();
        p.lifetime = 3f;

        return t;
    }

    private XpGem CreateXpGemTemplate()
    {
        GameObject prefab = RuntimePrefabLoader.LoadPrefab("ExpPrefab");
        GameObject t = prefab != null ? Instantiate(prefab) : new GameObject("XpGem_Template");
        t.name = "XpGem_Template";
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        if (t.GetComponent<SpriteRenderer>() != null)
            SetSortingOrder(t, 5);
        else
            AutoSprite2D.AddTo(t, new Color(0.4f, 1f, 0.55f, 1f), sortingOrder: 5);
        t.transform.localScale = new Vector3(4.2f, 4.2f, 1f);

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.0133f;

        XpGem gem = t.AddComponent<XpGem>();
        gem.xpAmount = 5;
        return gem;
    }

    private EnemyAI CreateMeleeEnemyTemplate(XpGem gemTemplate)
    {
        GameObject prefab = RuntimePrefabLoader.LoadPrefab("MeleeEnemyPrefab");
        GameObject t = prefab != null ? Instantiate(prefab) : new GameObject("EnemyMelee_Template");
        t.name = "EnemyMelee_Template";
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        if (t.GetComponent<SpriteRenderer>() != null)
            SetSortingOrder(t, 9);
        else
            AutoSprite2D.AddTo(t, new Color(0.8f, 0.25f, 0.9f, 1f), sortingOrder: 9);
        t.transform.localScale = new Vector3(4.8f, 4.8f, 1f);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 0f;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.0367f;

        EnemyAI e = t.AddComponent<EnemyAI>();
        e.maxHP = meleeHP;
        e.moveSpeed = meleeSpeed;
        e.contactDamage = meleeContactDamage;
        e.contactDamageCooldown = meleeContactCooldown;
        e.xpGemPrefab = gemTemplate;
        e.xpValue = 5;

        return e;
    }

    private RangedEnemyAI CreateRangedEnemyTemplate(XpGem gemTemplate, Projectile enemyProjectileTemplate)
    {
        GameObject prefab = RuntimePrefabLoader.LoadPrefab("RangedEnemyPrefab");
        GameObject t = prefab != null ? Instantiate(prefab) : new GameObject("EnemyRanged_Template");
        t.name = "EnemyRanged_Template";
        t.SetActive(false);
        t.transform.position = new Vector3(9999, 9999, 0);

        if (t.GetComponent<SpriteRenderer>() != null)
            SetSortingOrder(t, 9);
        else
            AutoSprite2D.AddTo(t, new Color(1f, 0.6f, 0.2f, 1f), sortingOrder: 9);
        t.transform.localScale = new Vector3(4.8f, 4.8f, 1f);

        Rigidbody2D rb = t.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 0f;

        CircleCollider2D col = t.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.0367f;

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

    private void SetSortingOrder(GameObject go, int sortingOrder)
    {
        SpriteRenderer[] renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sortingOrder = sortingOrder;
    }
}

