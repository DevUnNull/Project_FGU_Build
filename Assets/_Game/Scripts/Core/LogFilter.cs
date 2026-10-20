using UnityEngine;

/// <summary>
/// Bản build phát hành (không bật Development Build): bỏ Debug.Log thường, chỉ giữ Warning/Error.
/// Trên WebGL mỗi dòng log in ra console trình duyệt kèm stack trace, khi đông quái sẽ gây khựng hình.
/// Trong Editor và bản Development vẫn log đầy đủ.
/// </summary>
public static class LogFilter
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        if (!Debug.isDebugBuild) Debug.unityLogger.filterLogType = LogType.Warning;
    }
}
