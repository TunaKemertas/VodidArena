using UnityEngine;

/// <summary>
/// Simple audio service with procedural placeholder clips (no external files required).
/// Replace clips later by assigning AudioClips in the Inspector if desired.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private AudioClip _clickClip;
    private AudioClip _hitClip;
    private AudioClip _levelUpClip;
    private AudioClip _collectClip;
    private AudioClip _musicClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;

        BuildPlaceholderClips();
        ApplyVolumes();
    }

    private void Start()
    {
        PlayMusic();
    }

    private void BuildPlaceholderClips()
    {
        _clickClip = Tone(880f, 0.05f, 0.25f);
        _hitClip = Tone(180f, 0.12f, 0.35f);
        _levelUpClip = Tone(660f, 0.18f, 0.3f);
        _collectClip = Tone(1200f, 0.08f, 0.22f);
        _musicClip = AmbientLoop(220f, 4f, 0.08f);
    }

    public void ApplyVolumes()
    {
        if (SettingsManager.Instance == null) return;
        _musicSource.volume = SettingsManager.Instance.GetEffectiveMusic();
        _sfxSource.volume = SettingsManager.Instance.GetEffectiveSfx();
    }

    public void PlayMusic()
    {
        if (_musicSource.isPlaying) return;
        _musicSource.clip = _musicClip;
        ApplyVolumes();
        _musicSource.Play();
    }

    public void PlayClick() => PlaySfx(_clickClip);
    public void PlayHit() => PlaySfx(_hitClip);
    public void PlayLevelUp() => PlaySfx(_levelUpClip);
    public void PlayCollect() => PlaySfx(_collectClip);

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        ApplyVolumes();
        _sfxSource.PlayOneShot(clip);
    }

    private static AudioClip Tone(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.Max(1, (int)(sampleRate * duration));
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = 1f - (t / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
        }

        AudioClip clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static AudioClip AmbientLoop(float frequency, float duration, float volume)
    {
        int sampleRate = 44100;
        int samples = Mathf.Max(1, (int)(sampleRate * duration));
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            data[i] = (Mathf.Sin(2f * Mathf.PI * frequency * t) * 0.6f
                       + Mathf.Sin(2f * Mathf.PI * (frequency * 0.5f) * t) * 0.4f) * volume;
        }

        AudioClip clip = AudioClip.Create("music", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
