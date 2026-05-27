using UnityEngine;

/// <summary>
/// Simple main menu using IMGUI (no UnityEngine.UI package required).
/// </summary>
public class MenuBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        CleanupStarterSceneJunk();
        EnsureGameManager();
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

    private void OnGUI()
    {
        // Background
        Color prev = GUI.color;
        GUI.color = new Color(0.03f, 0.03f, 0.05f, 1f);
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);
        GUI.color = prev;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 44,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 22
        };

        float centerX = Screen.width * 0.5f;
        GUI.Label(new Rect(centerX - 350, 120, 700, 70), "VOID SURVIVORS", titleStyle);

        float btnW = 240;
        float btnH = 46;
        float x = centerX - btnW * 0.5f;
        float y = 260;

        if (GUI.Button(new Rect(x, y, btnW, btnH), "Start Game", buttonStyle))
            GameManager.Instance.StartGame();

        if (GUI.Button(new Rect(x, y + 60, btnW, btnH), "Quit", buttonStyle))
            Application.Quit();
    }
}

