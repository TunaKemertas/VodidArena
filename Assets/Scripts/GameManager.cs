using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Rules")]
    [Tooltip("How long the player must survive (seconds). 180 = 3 minutes.")]
    public float gameDurationSeconds = 180f;

    [Header("Scene")]
    [Tooltip("Main menu scene name (must be added to Build Settings).")]
    public string mainMenuSceneName = "MainMenu";
    [Tooltip("Game scene name (must be added to Build Settings).")]
    public string gameSceneName = "Game";

    public bool IsGameOverOrWon { get; private set; }

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
    }

    private void Start()
    {
        // In the Game scene, we expect a spawner and UI to exist.
        TryCacheSceneReferences();
    }

    private void Update()
    {
        if (IsGameOverOrWon) return;

        // If we loaded a new scene, cache again.
        if (_ui == null || _spawner == null)
            TryCacheSceneReferences();

        _elapsed += Time.unscaledDeltaTime;
        _ui?.SetTimer(_elapsed, gameDurationSeconds);

        if (_elapsed >= gameDurationSeconds)
        {
            Win();
        }
    }

    private void TryCacheSceneReferences()
    {
        _spawner = FindFirstObjectByType<EnemySpawner>();
        _ui = FindFirstObjectByType<UIManager>();
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        IsGameOverOrWon = false;
        _elapsed = 0f;
        _ui?.HideScreens();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        IsGameOverOrWon = false;
        _elapsed = 0f;
        _ui?.HideScreens();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void GameOver()
    {
        if (IsGameOverOrWon) return;

        IsGameOverOrWon = true;
        _spawner?.StopSpawning();
        Time.timeScale = 0f; // freeze gameplay
        _ui?.ShowGameOver();
    }

    private void Win()
    {
        if (IsGameOverOrWon) return;

        IsGameOverOrWon = true;
        _spawner?.StopSpawning();
        Time.timeScale = 0f; // freeze gameplay
        _ui?.ShowVictory();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        IsGameOverOrWon = false;
        _elapsed = 0f;
        _ui?.HideScreens();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

