using UnityEngine;

/// <summary>
/// Quản lý audio chung cho game (Singleton Pattern)
/// Hỗ trợ play sound effects với pooling và volume control
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;      // Cho background music
    [SerializeField] private AudioSource sfxSource;        // Cho sound effects

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Khởi tạo AudioSource nếu chưa có
    /// </summary>
    private void InitializeAudioSources()
    {
        // Tạo SFX source nếu chưa có
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_AudioSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }

        // Tạo Music source nếu chưa có
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music_AudioSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    /// <summary>
    /// Play một shot sound effect (dùng AudioSource chung - có thể overlap)
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("⚠️ AudioClip is null, không thể play!");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("❌ SFX AudioSource chưa được khởi tạo!");
            return;
        }

        // Play với volume đã được điều chỉnh
        float finalVolume = masterVolume * sfxVolume * volume;
        sfxSource.PlayOneShot(clip, finalVolume);
        sfxSource.pitch = pitch;
    }

    /// <summary>
    /// Play sound effect tại một vị trí cụ thể (3D sound)
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        float finalVolume = masterVolume * sfxVolume * volume;
        AudioSource.PlayClipAtPoint(clip, position, finalVolume);
    }

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.volume = masterVolume * musicVolume;
        musicSource.loop = loop;
        musicSource.Play();
    }

    /// <summary>
    /// Stop music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Set Master Volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }

    /// <summary>
    /// Set Music Volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }

    /// <summary>
    /// Set SFX Volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Cập nhật volume cho music source
    /// </summary>
    private void UpdateVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = masterVolume * musicVolume;
        }
    }
}

