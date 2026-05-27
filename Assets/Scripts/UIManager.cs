using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Minimal HUD + screens using IMGUI (no UnityEngine.UI package required).
/// Player/XP/GameManager call the setters, and this draws everything in OnGUI.
/// </summary>
public class UIManager : MonoBehaviour
{
    private int _hpCurrent = 100;
    private int _hpMax = 100;

    private int _xpCurrent = 0;
    private int _xpRequired = 20;

    private int _level = 1;

    private float _elapsed;
    private float _duration = 180f;

    private bool _showGameOver;
    private bool _showVictory;

    private GUIStyle _titleStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _buttonStyle;
    private Texture2D _tex;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Clear any old screen state when switching scenes (fixes "restart overlay stays").
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HideScreens();
    }

    private void EnsureStyles()
    {
        if (_titleStyle != null) return;

        _tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _tex.SetPixel(0, 0, Color.white);
        _tex.Apply();

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        _labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = Color.white }
        };

        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18
        };
    }

    public void SetHP(int current, int max)
    {
        _hpMax = Mathf.Max(1, max);
        _hpCurrent = Mathf.Clamp(current, 0, _hpMax);
    }

    public void SetXP(int current, int required)
    {
        _xpRequired = Mathf.Max(1, required);
        _xpCurrent = Mathf.Clamp(current, 0, _xpRequired);
    }

    public void SetLevel(int level)
    {
        _level = Mathf.Max(1, level);
    }

    public void SetTimer(float elapsedSeconds, float durationSeconds)
    {
        _elapsed = Mathf.Max(0f, elapsedSeconds);
        _duration = Mathf.Max(1f, durationSeconds);
    }

    public void ShowGameOver()
    {
        _showGameOver = true;
        _showVictory = false;
    }

    public void ShowVictory()
    {
        _showVictory = true;
        _showGameOver = false;
    }

    public void HideScreens()
    {
        _showGameOver = false;
        _showVictory = false;
    }

    private void OnGUI()
    {
        EnsureStyles();

        DrawHud();

        if (_showGameOver)
            DrawCenterScreen("GAME OVER", showRestart: true);

        if (_showVictory)
            DrawCenterScreen("YOU SURVIVED", showRestart: false);
    }

    private void DrawHud()
    {
        float pad = 16f;
        float barW = 260f;
        float hpH = 18f;
        float xpH = 14f;

        // HP Bar
        Rect hpRect = new Rect(pad, pad, barW, hpH);
        DrawBar(hpRect, SafeRatio(_hpCurrent, _hpMax), new Color(0.9f, 0.2f, 0.25f, 1f), "HP");

        // XP Bar
        Rect xpRect = new Rect(pad, pad + 28f, barW, xpH);
        DrawBar(xpRect, SafeRatio(_xpCurrent, _xpRequired), new Color(0.25f, 0.95f, 0.45f, 1f), "XP");

        // Timer + Level (top-right)
        float rightW = 160f;
        Rect timerRect = new Rect(Screen.width - rightW - pad, pad, rightW, 28f);
        Rect levelRect = new Rect(Screen.width - rightW - pad, pad + 26f, rightW, 28f);

        GUI.Label(timerRect, FormatTimerRemaining(), _labelStyle);
        GUI.Label(levelRect, $"LV {_level}", _labelStyle);
    }

    private void DrawBar(Rect rect, float t, Color fill, string label)
    {
        // Solid rectangles so it's always visible (no dependency on GUI skin textures).
        DrawRect(rect, new Color(0f, 0f, 0f, 0.75f));

        Rect fillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * Mathf.Clamp01(t), rect.height - 4);
        DrawRect(fillRect, fill);

        // Outline for contrast
        DrawOutline(rect, Color.black);

        // Label (with a tiny shadow)
        Rect labelRect = new Rect(rect.x + 6, rect.y - 2, rect.width, rect.height + 6);
        GUI.Label(new Rect(labelRect.x + 1, labelRect.y + 1, labelRect.width, labelRect.height), label, ShadowStyle());
        GUI.Label(labelRect, label, _labelStyle);
    }

    private GUIStyle ShadowStyle()
    {
        GUIStyle s = new GUIStyle(_labelStyle);
        s.normal.textColor = new Color(0f, 0f, 0f, 0.85f);
        return s;
    }

    private void DrawRect(Rect r, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, _tex);
        GUI.color = prev;
    }

    private void DrawOutline(Rect r, Color c)
    {
        DrawRect(new Rect(r.x, r.y, r.width, 1), c);
        DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), c);
        DrawRect(new Rect(r.x, r.y, 1, r.height), c);
        DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), c);
    }

    private void DrawCenterScreen(string title, bool showRestart)
    {
        // Dark overlay
        DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.78f));

        float panelW = 360f;
        float panelH = 220f;
        Rect panel = new Rect((Screen.width - panelW) * 0.5f, (Screen.height - panelH) * 0.5f, panelW, panelH);

        DrawRect(panel, new Color(0.08f, 0.08f, 0.1f, 1f));
        DrawOutline(panel, Color.black);
        GUI.Label(new Rect(panel.x, panel.y + 26f, panel.width, 60f), title, _titleStyle);

        float btnW = 180f;
        float btnH = 38f;
        float btnX = panel.x + (panel.width - btnW) * 0.5f;

        float y = panel.y + 110f;
        if (showRestart)
        {
            if (GUI.Button(new Rect(btnX, y, btnW, btnH), "Restart", _buttonStyle))
                GameManager.Instance.Restart();
            y += 48f;
        }

        if (GUI.Button(new Rect(btnX, y, btnW, btnH), "Menu", _buttonStyle))
            GameManager.Instance.ReturnToMenu();
    }

    private float SafeRatio(int a, int b)
    {
        if (b <= 0) return 0f;
        return Mathf.Clamp01(a / (float)b);
    }

    private string FormatTimerRemaining()
    {
        float remaining = Mathf.Max(0f, _duration - _elapsed);
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        return $"{minutes:0}:{seconds:00}";
    }
}

