using UnityEngine;

/// <summary>
/// Stores audio volume settings and persists them with PlayerPrefs.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 0.7f;
    public float SfxVolume { get; private set; } = 1f;

    private const string KeyMaster = "vs_master";
    private const string KeyMusic = "vs_music";
    private const string KeySfx = "vs_sfx";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(KeyMaster, 1f);
        MusicVolume = PlayerPrefs.GetFloat(KeyMusic, 0.7f);
        SfxVolume = PlayerPrefs.GetFloat(KeySfx, 1f);
    }

    public void SetMaster(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeyMaster, MasterVolume);
        AudioManager.Instance?.ApplyVolumes();
    }

    public void SetMusic(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeyMusic, MusicVolume);
        AudioManager.Instance?.ApplyVolumes();
    }

    public void SetSfx(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeySfx, SfxVolume);
    }

    public float GetEffectiveMusic() => MasterVolume * MusicVolume;
    public float GetEffectiveSfx() => MasterVolume * SfxVolume;
}
