using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Phát nhạc nền xuyên scene theo MusicLibrary: chuyển bài mượt (crossfade), giữ nguyên bài nếu scene mới dùng cùng bài,
/// theo âm lượng "Nhạc nền" trong Cài đặt. Tự khởi tạo khi game chạy, không cần đặt vào scene.
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    private MusicLibrary library;
    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource active;
    private MusicLibrary.Track current;

    // Hạ nhỏ nhạc nền tạm thời (bảng Thắng/Thua) để nghe rõ nhạc kết quả; trả về 1 khi sang scene mới
    private float duck = 1f;
    private Coroutine duckRoutine;

    // Nguồn nhạc riêng của scene (trận đấu) và âm lượng gốc của chúng
    private readonly Dictionary<AudioSource, float> sceneMusicBase = new Dictionary<AudioSource, float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (instance != null) return;
        var go = new GameObject("MusicPlayer");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MusicPlayer>();
        instance.Init();
        instance.OnSceneLoaded(SceneManager.GetActiveScene());
        SceneManager.sceneLoaded += (scene, mode) => { if (instance != null) instance.OnSceneLoaded(scene); };
    }

    private void Init()
    {
        library = Resources.Load<MusicLibrary>("MusicLibrary");
        sourceA = CreateSource();
        sourceB = CreateSource();
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        return source;
    }

    private void OnSceneLoaded(Scene scene)
    {
        if (library == null) return;

        if (duckRoutine != null) StopCoroutine(duckRoutine);
        duckRoutine = null;
        duck = 1f;

        sceneMusicBase.Clear();
        if (library.scenesWithOwnMusic.Contains(scene.name))
        {
            CollectSceneMusic(scene);
            ApplySceneMusicVolume();
            FadeTo(null);
            return;
        }
        FadeTo(library.Find(scene.name));
    }

    private void FadeTo(MusicLibrary.Track track)
    {
        // Cùng bài với scene trước -> chạy tiếp, chỉ cập nhật âm lượng
        if (track != null && current != null && track.clip == current.clip && active != null && active.isPlaying)
        {
            current = track;
            active.volume = TargetVolume(track);
            return;
        }
        StopAllCoroutines();
        StartCoroutine(Crossfade(track));
    }

    private IEnumerator Crossfade(MusicLibrary.Track track)
    {
        AudioSource from = active;
        AudioSource to = active == sourceA ? sourceB : sourceA;
        bool hasNext = track != null && track.clip != null;
        if (hasNext)
        {
            to.clip = track.clip;
            to.volume = 0f;
            to.Play();
        }

        float fromStart = from != null ? from.volume : 0f;
        float duration = Mathf.Max(0.05f, library.crossfadeSeconds);
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            if (from != null) from.volume = Mathf.Lerp(fromStart, 0f, k);
            if (hasNext) to.volume = Mathf.Lerp(0f, TargetVolume(track), k);
            yield return null;
        }

        if (from != null) from.Stop();
        current = hasNext ? track : null;
        active = hasNext ? to : null;
        if (active != null) active.volume = TargetVolume(track);
    }

    private static float TargetVolume(MusicLibrary.Track track) =>
        track.volume * GameSettings.MusicVolume * (instance != null ? instance.duck : 1f);

    private void CollectSceneMusic(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (SceneUIController controller in root.GetComponentsInChildren<SceneUIController>(true))
            {
                AudioSource source = controller.GetComponent<AudioSource>();
                if (source != null && !sceneMusicBase.ContainsKey(source)) sceneMusicBase[source] = source.volume;
            }
        }
    }

    private void ApplySceneMusicVolume()
    {
        foreach (var pair in sceneMusicBase)
        {
            if (pair.Key != null) pair.Key.volume = pair.Value * GameSettings.MusicVolume * duck;
        }
    }

    /// <summary>Gọi khi người chơi kéo thanh "Nhạc nền".</summary>
    public static void ApplyVolume()
    {
        if (instance == null) return;
        if (instance.active != null && instance.current != null) instance.active.volume = TargetVolume(instance.current);
        instance.ApplySceneMusicVolume();
    }

    /// <summary>
    /// Hạ nhạc nền xuống level (0–1) trong seconds giây (thời gian thực), ví dụ khi hiện bảng Thắng/Thua.
    /// Tự trả về bình thường khi sang scene khác.
    /// </summary>
    public static void Duck(float level, float seconds = 0.6f)
    {
        if (instance == null) return;
        if (instance.duckRoutine != null) instance.StopCoroutine(instance.duckRoutine);
        instance.duckRoutine = instance.StartCoroutine(instance.DuckRoutine(Mathf.Clamp01(level), seconds));
    }

    private IEnumerator DuckRoutine(float target, float seconds)
    {
        float start = duck;
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
        {
            duck = Mathf.Lerp(start, target, t / seconds);
            ApplyVolume();
            yield return null;
        }
        duck = target;
        ApplyVolume();
        duckRoutine = null;
    }
}
