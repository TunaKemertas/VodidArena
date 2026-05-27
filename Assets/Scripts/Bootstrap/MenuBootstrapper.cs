using UnityEngine;

/// <summary>
/// Boots the main menu scene (UI canvas + GameManager services).
/// </summary>
public class MenuBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        CleanupStarterSceneJunk();
        EnsureGameManager();

        if (FindFirstObjectByType<MainMenuUI>() == null)
            new GameObject("MainMenuUI").AddComponent<MainMenuUI>();
    }

    private void CleanupStarterSceneJunk()
    {
        DestroyObjectIfFound("Global Volume");
        DestroyObjectIfFound("Directional Light");
        DestroyObjectIfFound("Cube");
    }

    private void DestroyObjectIfFound(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        if (go != null) Destroy(go);
    }

    private void EnsureGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.mainMenuSceneName = "MainMenu";
            GameManager.Instance.gameSceneName = "Game";
            return;
        }

        GameObject gm = new GameObject("GameManager");
        GameManager mgr = gm.AddComponent<GameManager>();
        mgr.mainMenuSceneName = "MainMenu";
        mgr.gameSceneName = "Game";
    }
}
