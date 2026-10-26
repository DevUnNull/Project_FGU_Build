using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nhạc nền theo từng scene (asset đặt tại Resources/MusicLibrary). MusicPlayer đọc bảng này mỗi khi đổi scene.
/// Hai scene liên tiếp dùng cùng 1 bài thì nhạc chạy tiếp, không phát lại từ đầu.
/// </summary>
[CreateAssetMenu(fileName = "MusicLibrary", menuName = "Game/Music Library")]
public class MusicLibrary : ScriptableObject
{
    [System.Serializable]
    public class Track
    {
        public string sceneName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.5f;
    }

    public List<Track> tracks = new List<Track>();

    [Tooltip("Scene tự phát nhạc riêng (VD trận đấu dùng AudioSource của SceneUIController): nhạc chung tắt dần, nhạc của scene được chỉnh theo âm lượng Nhạc nền")]
    public List<string> scenesWithOwnMusic = new List<string> { "Ingame_1", "LuTile" };

    [Tooltip("Thời gian chuyển bài (giây)")]
    public float crossfadeSeconds = 1.2f;

    public Track Find(string sceneName) => tracks.Find(t => t.sceneName == sceneName);
}
