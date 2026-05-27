using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu panels: Play, Settings, Quit. Built at runtime by MenuBootstrapper.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    private GameObject _menuPanel;
    private GameObject _settingsPanel;
    private RectTransform _titleRect;
    private Button _playButton;

    private void Awake()
    {
        BuildUI();
    }

    private void Start()
    {
        UIAnimations.PlayMainMenuIntro(_titleRect, _playButton);
    }

    private void BuildUI()
    {
        RectTransform safe = UICanvasFactory.CreateCanvasRoot("MainMenuCanvas", out _);

        UICanvasFactory.CreatePanel(safe, "Background", new Color(0.03f, 0.03f, 0.06f, 1f));

        _menuPanel = new GameObject("MainMenuPanel");
        _menuPanel.transform.SetParent(safe, false);
        RectTransform menuRt = _menuPanel.AddComponent<RectTransform>();
        UICanvasFactory.StretchFull(menuRt);

        Text title = UICanvasFactory.CreateText(_menuPanel.transform, "Title", "VOID SURVIVORS", 72, TextAnchor.MiddleCenter);
        _titleRect = title.GetComponent<RectTransform>();
        _titleRect.anchorMin = new Vector2(0.5f, 0.72f);
        _titleRect.anchorMax = new Vector2(0.5f, 0.72f);
        _titleRect.pivot = new Vector2(0.5f, 0.5f);
        _titleRect.sizeDelta = new Vector2(900, 120);
        _titleRect.anchoredPosition = Vector2.zero;
        title.fontStyle = FontStyle.Bold;

        _playButton = UICanvasFactory.CreateButton(_menuPanel.transform, "Play", new Vector2(0, 40), new Vector2(420, 90));
        _playButton.onClick.AddListener(() => GameManager.Instance.StartGame());

        Button settingsBtn = UICanvasFactory.CreateButton(_menuPanel.transform, "Settings", new Vector2(0, -70), new Vector2(420, 90));
        settingsBtn.onClick.AddListener(ShowSettings);

        Button quitBtn = UICanvasFactory.CreateButton(_menuPanel.transform, "Quit", new Vector2(0, -180), new Vector2(420, 90));
        quitBtn.onClick.AddListener(Application.Quit);

        BuildSettingsPanel(safe);
    }

    private void BuildSettingsPanel(Transform safe)
    {
        _settingsPanel = new GameObject("SettingsPanel");
        _settingsPanel.transform.SetParent(safe, false);
        RectTransform rt = _settingsPanel.AddComponent<RectTransform>();
        UICanvasFactory.StretchFull(rt);
        Image bg = _settingsPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.82f);
        _settingsPanel.SetActive(false);

        Text header = UICanvasFactory.CreateText(_settingsPanel.transform, "Header", "SETTINGS", 56, TextAnchor.MiddleCenter);
        RectTransform hrt = header.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.5f, 0.78f);
        hrt.anchorMax = new Vector2(0.5f, 0.78f);
        hrt.sizeDelta = new Vector2(700, 80);

        SettingsManager settings = SettingsManager.Instance;
        float master = settings != null ? settings.MasterVolume : 1f;
        float music = settings != null ? settings.MusicVolume : 0.7f;
        float sfx = settings != null ? settings.SfxVolume : 1f;

        Slider masterSlider = UICanvasFactory.CreateSlider(_settingsPanel.transform, "Master", new Vector2(0, 120), new Vector2(760, 36), "Master Volume", master);
        Slider musicSlider = UICanvasFactory.CreateSlider(_settingsPanel.transform, "Music", new Vector2(0, 20), new Vector2(760, 36), "Music Volume", music);
        Slider sfxSlider = UICanvasFactory.CreateSlider(_settingsPanel.transform, "SFX", new Vector2(0, -80), new Vector2(760, 36), "SFX Volume", sfx);

        masterSlider.onValueChanged.AddListener(v => SettingsManager.Instance?.SetMaster(v));
        musicSlider.onValueChanged.AddListener(v => SettingsManager.Instance?.SetMusic(v));
        sfxSlider.onValueChanged.AddListener(v => SettingsManager.Instance?.SetSfx(v));

        Button close = UICanvasFactory.CreateButton(_settingsPanel.transform, "Close", new Vector2(0, -220), new Vector2(420, 90));
        close.onClick.AddListener(HideSettings);
    }

    private void ShowSettings()
    {
        _settingsPanel.SetActive(true);
    }

    private void HideSettings()
    {
        _settingsPanel.SetActive(false);
        AudioManager.Instance?.PlayClick();
    }
}
