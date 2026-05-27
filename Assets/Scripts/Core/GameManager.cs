using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Rules")]
    [Tooltip("How long the player must survive (seconds). 180 = 3 minutes.")]
    public float gameDurationSeconds = 180f;

    [Header("Scene")]
    public string mainMenuSceneName = "MainMenu";
    public string gameSceneName = "Game";

    public bool IsGameOverOrWon { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsLevelUp { get; private set; }

    private float _elapsed;
    private EnemySpawner _spawner;
    private UIManager _ui;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureCoreServices();
    }

    private void Start()
    {
        TryCacheSceneReferences();
    }

    private void Update()
    {
        if (IsInGameScene() && !IsGameOverOrWon && !IsLevelUp && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (IsGameOverOrWon || IsPaused) return;

        if (_ui == null || _spawner == null)
            TryCacheSceneReferences();

        _elapsed += Time.unscaledDeltaTime;
        _ui?.SetTimer(_elapsed, gameDurationSeconds);

        if (_elapsed >= gameDurationSeconds)
            Win();
    }

    private void EnsureCoreServices()
    {
        if (GetComponent<SettingsManager>() == null)
            gameObject.AddComponent<SettingsManager>();
        if (GetComponent<AudioManager>() == null)
            gameObject.AddComponent<AudioManager>();
    }

    private bool IsInGameScene()
    {
        return SceneManager.GetActiveScene().name == gameSceneName;
    }

    private void TryCacheSceneReferences()
    {
        if (!IsInGameScene()) return;
        _spawner = FindFirstObjectByType<EnemySpawner>();
        _ui = FindFirstObjectByType<UIManager>();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        IsGameOverOrWon = false;
        IsPaused = false;
        _elapsed = 0f;
        _ui?.HideScreens();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        IsGameOverOrWon = false;
        IsPaused = false;
        _elapsed = 0f;
        _ui?.HideScreens();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void TogglePause()
    {
        if (IsGameOverOrWon || !IsInGameScene()) return;
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsGameOverOrWon || IsPaused) return;
        IsPaused = true;
        IsLevelUp = false;
        Time.timeScale = 0f;
        _ui?.ShowPause();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        IsLevelUp = false;
        Time.timeScale = 1f;
        _ui?.HidePause();
    }

    /// <summary>
    /// Used by the level-up system: pauses gameplay WITHOUT showing the pause menu.
    /// </summary>
    public void EnterLevelUpPause()
    {
        if (IsGameOverOrWon) return;
        IsPaused = true;
        IsLevelUp = true;
        Time.timeScale = 0f;
        _ui?.HidePause();
    }

    public void ExitLevelUpPause()
    {
        if (!IsLevelUp) return;
        IsPaused = false;
        IsLevelUp = false;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (IsGameOverOrWon) return;

        IsGameOverOrWon = true;
        IsPaused = false;
        IsLevelUp = false;
        _spawner?.StopSpawning();
        Time.timeScale = 0f;
        _ui?.ShowGameOver();
    }

    private void Win()
    {
        if (IsGameOverOrWon) return;

        IsGameOverOrWon = true;
        IsPaused = false;
        IsLevelUp = false;
        _spawner?.StopSpawning();
        Time.timeScale = 0f;
        _ui?.ShowVictory();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        IsGameOverOrWon = false;
        IsPaused = false;
        IsLevelUp = false;
        _elapsed = 0f;
        _ui?.HideScreens();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
