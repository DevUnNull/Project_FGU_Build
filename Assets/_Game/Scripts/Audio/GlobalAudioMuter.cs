using UnityEngine;

/// <summary>
/// Mute toàn bộ âm thanh (Game Audio & Video Audio) tự động trước khi bất kỳ Scene nào được nạp.
/// Hữu ích cho việc đẩy bản Test WebGL/Mobile không có âm thanh.
/// </summary>
public static class GlobalAudioMuter
{
    // 🔴 BẬT (true) để tắt sạch âm thanh toàn game | 🟢 TẮT (false) để bật lại âm thanh bình thường
    public static bool isTestMuteActive = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyGlobalMute()
    {
        if (isTestMuteActive)
        {
            AudioListener.volume = 0f;
            AudioListener.pause = true;
            Debug.Log("🔇 [GlobalAudioMuter] Đã tắt toàn bộ âm thanh game cho bản Test!");
        }
    }
}
