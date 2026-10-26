using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cài đặt người chơi (bảng Setting ở MainMenuScene), lưu bằng PlayerPrefs:
/// - Âm lượng tổng (AudioListener), nhạc nền (MusicPlayer) và hiệu ứng (GameSfx, AudioManager)
/// - Xem lại hướng dẫn, chơi lại từ đầu
/// Khi GlobalAudioMuter đang tắt tiếng cho bản thử nghiệm thì không bật lại âm thanh.
/// </summary>
public static class GameSettings
{
    private const string MasterVolumeKey = "CallOutMyCells_MasterVolume";
    private const string SfxVolumeKey = "CallOutMyCells_SfxVolume";
    private const string MusicVolumeKey = "CallOutMyCells_MusicVolume";

    public static float MasterVolume => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
    public static float SfxVolume => PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
    public static float MusicVolume => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);

    public static void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
        MusicPlayer.ApplyVolume();
    }

    /// <summary>Bản thử nghiệm đang tắt toàn bộ âm thanh (GlobalAudioMuter.isTestMuteActive).</summary>
    public static bool IsTestMuted => GlobalAudioMuter.isTestMuteActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Apply();
        SceneManager.sceneLoaded += (scene, mode) => Apply();
    }

    public static void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
        Apply();
    }

    public static void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
        Apply();
    }

    public static void Apply()
    {
        if (!IsTestMuted)
        {
            AudioListener.pause = false;
            AudioListener.volume = MasterVolume;
        }
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(SfxVolume);
    }

    /// <summary>Cho xem lại mọi hướng dẫn (giới thiệu tế bào, Cách chơi, gợi ý ghép).</summary>
    public static void ResetTutorials()
    {
        HeroTutorialManager.ClearSeenFlag();
        HowToPlayUI.ResetSeen();
        MergeHintUI.ResetLearned();
        UpgradeTutorialUI.ResetSeen();
    }

    /// <summary>Chơi lại từ đầu: khóa lại các ngày, xóa sao, nâng cấp, Sổ tay và hướng dẫn. Giữ cài đặt âm thanh.</summary>
    public static void ResetProgress()
    {
        GameProgress.ResetAll();
        UpgradeProgress.ResetAll();
        NotebookProgress.ResetAll();
        ResetTutorials();
    }
}
