using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tiếng bấm cho mọi Button: mỗi khi nạp scene, gắn tiếng GameSfx.UIClick vào mọi nút trong scene (kể cả đang ẩn).
/// Bỏ qua nút đã có ButtonClickAudio (tiếng riêng chỉnh tay). Nút tạo bằng code qua RuntimeUI tự có tiếng sẵn;
/// nút sinh ra lúc chạy từ prefab thì gọi UIClickSounds.Attach.
/// </summary>
public static class UIClickSounds
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        AttachToScene(SceneManager.GetActiveScene());
        SceneManager.sceneLoaded += (scene, mode) => AttachToScene(scene);
    }

    private static void AttachToScene(Scene scene)
    {
        foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (button.gameObject.scene == scene) Attach(button);
        }
    }

    /// <summary>Gắn tiếng bấm 1 lần cho nút (gọi lại nhiều lần vẫn an toàn).</summary>
    public static void Attach(Button button)
    {
        if (button == null || button.GetComponent<ClickSoundTag>() != null || button.GetComponent<ButtonClickAudio>() != null) return;
        button.gameObject.AddComponent<ClickSoundTag>();
        button.onClick.AddListener(() => GameSfx.Play(GameSfx.Kind.UIClick));
    }
}
