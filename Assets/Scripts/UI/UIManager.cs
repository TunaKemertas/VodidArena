using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VoidSurvivors.Weapons;

/// <summary>
/// In-game HUD and overlay panels (pause, settings, game over, victory, level-up).
/// Keeps the same public API used by gameplay scripts.
/// </summary>
public class UIManager : MonoBehaviour
{
    private Image _hpFill;
    private RectTransform _hpFillRect;
    private Image _xpFill;
    private RectTransform _xpFillRect;
    private Text _timerText;
    private Text _levelText;
    private RectTransform _weaponsHud;
    private readonly Dictionary<WeaponId, Text> _weaponHudLabels = new Dictionary<WeaponId, Text>();
    private WeaponManager _weaponManager;

    private GameObject _pausePanel;
    private GameObject _settingsPanel;
    private GameObject _gameOverPanel;
    private GameObject _victoryPanel;
    private RectTransform _levelUpPanel;
    private Text _levelUpText;
    private readonly Button[] _levelUpChoices = new Button[3];
    private readonly Text[] _levelUpChoiceLabels = new Text[3];
    private Action<WeaponUpgradeChoice> _onPickLevelUpChoice;
    private List<WeaponUpgradeChoice> _currentChoices;

    private Image _gameOverOverlay;
    private RectTransform _gameOverTitle;
    private Button _gameOverRestart;
    private Button _gameOverMenu;

    private Image _victoryOverlay;
    private RectTransform _victoryTitle;
    private Button _victoryMenu;

    private void Awake()
    {
        BuildUI();
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

    private void BuildUI()
    {
        RectTransform safe = UICanvasFactory.CreateCanvasRoot("GameCanvas", out _);
        UICanvasFactory.CreatePanel(safe, "DimBackground", new Color(0, 0, 0, 0f));

        // Overlays first; HUD layer is built after pause so bars stay visible on top when paused.
        BuildPausePanel(safe);
        BuildHudLayer(safe);
        BuildLevelUpPanel(safe);
        BuildGameOverPanel(safe);
        BuildVictoryPanel(safe);
        BuildSettingsPanel(safe);
    }

    private void BuildHudLayer(Transform safe)
    {
        GameObject hudGo = new GameObject("HudLayer");
        hudGo.transform.SetParent(safe, false);
        RectTransform hudRt = hudGo.AddComponent<RectTransform>();
        UICanvasFactory.StretchFull(hudRt);

        BuildHud(hudGo.transform);
    }

    private void BuildHud(Transform safe)
    {
        // Bars first (left): drawn behind timer / pause so overlap never hides fill.
        Image hpBarRoot = UICanvasFactory.CreateBar(safe, "HPBar", new Vector2(40, -60), new Vector2(420, 36),
            new Color(0.92f, 0.22f, 0.28f, 1f), out _hpFill);
        _hpFillRect = _hpFill.rectTransform;
        UICanvasFactory.CreateText(hpBarRoot.transform, "Label", "HP", 24, TextAnchor.MiddleLeft)
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(12, 0);

        Image xpBarRoot = UICanvasFactory.CreateBar(safe, "XPBar", new Vector2(40, -110), new Vector2(420, 28),
            new Color(0.28f, 0.95f, 0.48f, 1f), out _xpFill);
        _xpFillRect = _xpFill.rectTransform;
        UICanvasFactory.CreateText(xpBarRoot.transform, "Label", "XP", 22, TextAnchor.MiddleLeft);

        // Timer — top center
        _timerText = UICanvasFactory.CreateText(safe, "Timer", "3:00", 40, TextAnchor.UpperCenter);
        RectTransform trt = _timerText.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -42f);
        trt.sizeDelta = new Vector2(280f, 56f);

        // Level — top right, under pause button (pause built last so it stays clickable on top).
        _levelText = UICanvasFactory.CreateText(safe, "Level", "LV 1", 32, TextAnchor.UpperRight);
        RectTransform lrt = _levelText.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(1f, 1f);
        lrt.anchorMax = new Vector2(1f, 1f);
        lrt.pivot = new Vector2(1f, 1f);
        lrt.anchoredPosition = new Vector2(-40f, -128f);
        lrt.sizeDelta = new Vector2(200f, 40f);

        // Pause — top right
        GameObject pauseGo = new GameObject("PauseButton");
        pauseGo.transform.SetParent(safe, false);
        RectTransform prt = pauseGo.AddComponent<RectTransform>();
        prt.anchorMin = new Vector2(1f, 1f);
        prt.anchorMax = new Vector2(1f, 1f);
        prt.pivot = new Vector2(1f, 1f);
        prt.anchoredPosition = new Vector2(-40f, -40f);
        prt.sizeDelta = new Vector2(180f, 72f);
        Image pImg = pauseGo.AddComponent<Image>();
        pImg.color = new Color(0.14f, 0.14f, 0.18f, 1f);
        Button pauseBtn = pauseGo.AddComponent<Button>();
        pauseBtn.targetGraphic = pImg;
        Text pLabel = UICanvasFactory.CreateText(pauseGo.transform, "Label", "Pause", 30, TextAnchor.MiddleCenter);
        UICanvasFactory.StretchFull(pLabel.rectTransform);
        pauseGo.AddComponent<UIButtonFeedback>();
        pauseBtn.onClick.AddListener(() => GameManager.Instance?.TogglePause());

        BuildWeaponsHud(safe);
        MobileJoystickInput.Create(safe);
    }

    private void BuildWeaponsHud(Transform safe)
    {
        GameObject go = new GameObject("WeaponsHUD");
        go.transform.SetParent(safe, false);
        _weaponsHud = go.AddComponent<RectTransform>();
        _weaponsHud.anchorMin = new Vector2(0.5f, 1f);
        _weaponsHud.anchorMax = new Vector2(0.5f, 1f);
        _weaponsHud.pivot = new Vector2(0.5f, 1f);
        _weaponsHud.anchoredPosition = new Vector2(0f, -108f);
        _weaponsHud.sizeDelta = new Vector2(900f, 54f);
    }

    public void SetWeaponManager(WeaponManager manager)
    {
        if (_weaponManager == manager) return;

        if (_weaponManager != null)
            _weaponManager.OnLoadoutChanged -= RefreshWeaponsHud;

        _weaponManager = manager;
        if (_weaponManager != null)
            _weaponManager.OnLoadoutChanged += RefreshWeaponsHud;

        RefreshWeaponsHud();
    }

    private void RefreshWeaponsHud()
    {
        if (_weaponsHud == null) return;
        if (_weaponManager == null) return;

        // Clear old
        for (int i = _weaponsHud.childCount - 1; i >= 0; i--)
            Destroy(_weaponsHud.GetChild(i).gameObject);
        _weaponHudLabels.Clear();

        // Show main + owned specials (max 3).
        List<WeaponId> ids = new List<WeaponId> { WeaponId.MainGun };
        foreach (WeaponId id in _weaponManager.GetOwnedSpecialWeapons())
            ids.Add(id);

        float x = -Mathf.Min(380f, (ids.Count - 1) * 140f * 0.5f);
        for (int i = 0; i < ids.Count; i++)
        {
            WeaponId id = ids[i];
            int lvl = _weaponManager.GetLevel(id);
            CreateWeaponHudChip(_weaponsHud, id, lvl, new Vector2(x + i * 140f, 0f));
        }
    }

    private void CreateWeaponHudChip(Transform parent, WeaponId id, int level, Vector2 pos)
    {
        GameObject chip = new GameObject($"{id}_Chip");
        chip.transform.SetParent(parent, false);
        RectTransform rt = chip.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(132f, 46f);

        Image bg = chip.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.45f);

        Text t = UICanvasFactory.CreateText(chip.transform, "Label", "", 22, TextAnchor.MiddleCenter);
        RectTransform trt = t.GetComponent<RectTransform>();
        UICanvasFactory.StretchFull(trt);

        string shortName = WeaponManager.GetWeaponName(id);
        if (shortName.Length > 10) shortName = shortName.Substring(0, 10);
        t.text = $"{shortName}\nLV {level}";
        _weaponHudLabels[id] = t;
    }

    private void BuildPausePanel(Transform safe)
    {
        _pausePanel = new GameObject("PausePanel");
        _pausePanel.transform.SetParent(safe, false);
        RectTransform rt = _pausePanel.AddComponent<RectTransform>();
        UICanvasFactory.StretchFull(rt);
        // Leave the top HUD strip (HP/XP/timer) uncovered so bars stay readable while paused.
        rt.offsetMax = new Vector2(0f, -170f);
        _pausePanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.75f);
        _pausePanel.SetActive(false);

        UICanvasFactory.CreateText(_pausePanel.transform, "Title", "PAUSED", 64, TextAnchor.MiddleCenter)
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 260);

        Button resume = UICanvasFactory.CreateButton(_pausePanel.transform, "Resume", new Vector2(0, 80), new Vector2(420, 90));
        resume.onClick.AddListener(() => GameManager.Instance?.Resume());

        Button restart = UICanvasFactory.CreateButton(_pausePanel.transform, "Restart", new Vector2(0, -40), new Vector2(420, 90));
        restart.onClick.AddListener(() => GameManager.Instance?.Restart());

        Button menu = UICanvasFactory.CreateButton(_pausePanel.transform, "Main Menu", new Vector2(0, -160), new Vector2(420, 90));
        menu.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());

        Button settings = UICanvasFactory.CreateButton(_pausePanel.transform, "Settings", new Vector2(0, -280), new Vector2(420, 90));
        settings.onClick.AddListener(() => _settingsPanel.SetActive(true));
    }

    private void BuildSettingsPanel(Transform safe)
    {
        _settingsPanel = new GameObject("SettingsPanel");
        _settingsPanel.transform.SetParent(safe, false);
        RectTransform rt = _settingsPanel.AddComponent<RectTransform>();
        UICanvasFactory.StretchFull(rt);
        _settingsPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.88f);
        _settingsPanel.SetActive(false);

        UICanvasFactory.CreateText(_settingsPanel.transform, "Title", "SETTINGS", 56, TextAnchor.MiddleCenter)
            .GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 320);

        SettingsManager s = SettingsManager.Instance;
        Slider master = UICanvasFactory.CreateSlider(_settingsPanel.transform, "Master", new Vector2(0, 120), new Vector2(760, 36), "Master Volume", s != null ? s.MasterVolume : 1f);
        Slider music = UICanvasFactory.CreateSlider(_settingsPanel.transform, "Music", new Vector2(0, 20), new Vector2(760, 36), "Music Volume", s != null ? s.MusicVolume : 0.7f);
        Slider sfx = UICanvasFactory.CreateSlider(_settingsPanel.transform, "SFX", new Vector2(0, -80), new Vector2(760, 36), "SFX Volume", s != null ? s.SfxVolume : 1f);

        master.onValueChanged.AddListener(v => SettingsManager.Instance?.SetMaster(v));
        music.onValueChanged.AddListener(v => SettingsManager.Instance?.SetMusic(v));
        sfx.onValueChanged.AddListener(v => SettingsManager.Instance?.SetSfx(v));

        Button close = UICanvasFactory.CreateButton(_settingsPanel.transform, "Close", new Vector2(0, -240), new Vector2(420, 90));
        close.onClick.AddListener(() => _settingsPanel.SetActive(false));
    }

    private void BuildLevelUpPanel(Transform safe)
    {
        GameObject go = new GameObject("LevelUpPanel");
        go.transform.SetParent(safe, false);
        _levelUpPanel = go.AddComponent<RectTransform>();
        _levelUpPanel.anchorMin = new Vector2(0.5f, 0.55f);
        _levelUpPanel.anchorMax = new Vector2(0.5f, 0.55f);
        _levelUpPanel.sizeDelta = new Vector2(820, 560);
        go.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.12f, 0.92f);
        go.SetActive(false);

        _levelUpText = UICanvasFactory.CreateText(go.transform, "LevelUpText", "LEVEL UP!", 52, TextAnchor.UpperCenter);
        RectTransform trt = _levelUpText.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0, -24);
        trt.sizeDelta = new Vector2(780, 120);
        _levelUpText.fontStyle = FontStyle.Bold;

        // Three upgrade buttons
        for (int i = 0; i < 3; i++)
        {
            Vector2 pos = new Vector2(0, 160 - i * 160);
            Button b = UICanvasFactory.CreateButton(go.transform, $"Choice {i + 1}", pos, new Vector2(740, 120));
            _levelUpChoices[i] = b;
            _levelUpChoiceLabels[i] = b.GetComponentInChildren<Text>();

            int index = i;
            b.onClick.AddListener(() =>
            {
                if (_currentChoices == null) return;
                if (index < 0 || index >= _currentChoices.Count) return;
                _onPickLevelUpChoice?.Invoke(_currentChoices[index]);
            });
        }
    }

    private void BuildGameOverPanel(Transform safe)
    {
        _gameOverPanel = new GameObject("GameOverPanel");
        _gameOverPanel.transform.SetParent(safe, false);
        RectTransform rt = _gameOverPanel.AddComponent<RectTransform>();
        UICanvasFactory.StretchFull(rt);
        _gameOverOverlay = _gameOverPanel.AddComponent<Image>();
        _gameOverOverlay.color = new Color(0, 0, 0, 1f);
        _gameOverPanel.SetActive(false);

        Text title = UICanvasFactory.CreateText(_gameOverPanel.transform, "Title", "GAME OVER", 72, TextAnchor.MiddleCenter);
        _gameOverTitle = title.GetComponent<RectTransform>();
        _gameOverTitle.anchoredPosition = new Vector2(0, 180);
        _gameOverTitle.sizeDelta = new Vector2(800, 100);
        title.fontStyle = FontStyle.Bold;

        _gameOverRestart = UICanvasFactory.CreateButton(_gameOverPanel.transform, "Restart", new Vector2(0, 20), new Vector2(420, 90));
        _gameOverRestart.onClick.AddListener(() => GameManager.Instance?.Restart());

        _gameOverMenu = UICanvasFactory.CreateButton(_gameOverPanel.transform, "Main Menu", new Vector2(0, -100), new Vector2(420, 90));
        _gameOverMenu.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
    }

    private void BuildVictoryPanel(Transform safe)
    {
        _victoryPanel = new GameObject("VictoryPanel");
        _victoryPanel.transform.SetParent(safe, false);
        RectTransform rt = _victoryPanel.AddComponent<RectTransform>();
        UICanvasFactory.StretchFull(rt);
        _victoryOverlay = _victoryPanel.AddComponent<Image>();
        _victoryOverlay.color = new Color(0, 0, 0, 1f);
        _victoryPanel.SetActive(false);

        Text title = UICanvasFactory.CreateText(_victoryPanel.transform, "Title", "YOU SURVIVED", 64, TextAnchor.MiddleCenter);
        _victoryTitle = title.GetComponent<RectTransform>();
        _victoryTitle.anchoredPosition = new Vector2(0, 180);
        _victoryTitle.sizeDelta = new Vector2(900, 100);
        title.fontStyle = FontStyle.Bold;

        _victoryMenu = UICanvasFactory.CreateButton(_victoryPanel.transform, "Main Menu", new Vector2(0, -40), new Vector2(420, 90));
        _victoryMenu.onClick.AddListener(() => GameManager.Instance?.ReturnToMenu());
    }

    public void SetHP(int current, int max)
    {
        if (_hpFillRect == null) return;
        UICanvasFactory.SetBarFillAmount(_hpFillRect, current / (float)Mathf.Max(1, max));
    }

    public void SetXP(int current, int required)
    {
        if (_xpFillRect == null) return;
        UICanvasFactory.SetBarFillAmount(_xpFillRect, current / (float)Mathf.Max(1, required));
    }

    public void SetLevel(int level)
    {
        if (_levelText != null)
            _levelText.text = $"LV {level}";
    }

    public void SetTimer(float elapsedSeconds, float durationSeconds)
    {
        if (_timerText == null) return;
        float remaining = Mathf.Max(0f, durationSeconds - elapsedSeconds);
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        _timerText.text = $"{minutes:0}:{seconds:00}";
    }

    public void ShowPause()
    {
        if (_pausePanel != null) _pausePanel.SetActive(true);
    }

    public void HidePause()
    {
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        HidePause();
        _gameOverPanel.SetActive(true);
        UIAnimations.PlayGameOver(_gameOverOverlay, _gameOverTitle,
            new[] { _gameOverRestart, _gameOverMenu });
    }

    public void ShowVictory()
    {
        HidePause();
        _victoryPanel.SetActive(true);
        UIAnimations.PlayVictory(_victoryOverlay, _victoryTitle, new[] { _victoryMenu });
    }

    public void HideScreens()
    {
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_victoryPanel != null) _victoryPanel.SetActive(false);
        if (_levelUpPanel != null) _levelUpPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called by XPManager when the player levels up (DOTween sequence #2).
    /// </summary>
    public void PlayLevelUp(int level)
    {
        if (_levelUpText != null)
            _levelUpText.text = $"LEVEL UP!\nLV {level}";

        AudioManager.Instance?.PlayLevelUp();
        UIAnimations.PlayLevelUp(_levelUpPanel, _levelUpText, _xpFill);
    }

    public void ShowLevelUpChoices(int level, List<WeaponUpgradeChoice> choices, Action<WeaponUpgradeChoice> onPick)
    {
        if (_levelUpPanel == null) return;
        _currentChoices = choices;
        _onPickLevelUpChoice = onPick;

        if (_levelUpText != null)
            _levelUpText.text = $"LEVEL UP!\nLV {level}";

        for (int i = 0; i < _levelUpChoices.Length; i++)
        {
            Button b = _levelUpChoices[i];
            if (b == null) continue;

            bool has = choices != null && i < choices.Count && choices[i] != null;
            b.gameObject.SetActive(has);
            if (!has) continue;

            WeaponUpgradeChoice c = choices[i];
            string name = WeaponManager.GetWeaponName(c.WeaponId);
            string line1 = c.IsUnlock ? $"{name} (Unlock)" : $"{name} Lv{c.CurrentLevel} → Lv{c.NextLevel}";
            string line2 = c.Description ?? string.Empty;

            if (_levelUpChoiceLabels[i] != null)
            {
                _levelUpChoiceLabels[i].text = $"{line1}\n<size=24>{line2}</size>";
                _levelUpChoiceLabels[i].alignment = TextAnchor.MiddleCenter;
            }
        }

        AudioManager.Instance?.PlayLevelUp();
        _levelUpPanel.gameObject.SetActive(true);
        // Keep choice panel open until the player clicks.
        // (UIAnimations.PlayLevelUp auto-fades and closes, so we do not use it here.)
        _levelUpPanel.localScale = Vector3.one;
    }

    public void HideLevelUpChoices()
    {
        _onPickLevelUpChoice = null;
        _currentChoices = null;
        if (_levelUpPanel != null) _levelUpPanel.gameObject.SetActive(false);
    }
}
