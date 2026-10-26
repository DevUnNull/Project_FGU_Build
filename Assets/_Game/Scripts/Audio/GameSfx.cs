using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Danh sách âm thanh hiệu ứng của cả game (asset đặt tại Resources/GameSfx): trận đấu, giao diện, phần thưởng.
/// Phát qua một nhóm AudioSource 2D riêng (DontDestroyOnLoad) nên scene nào cũng nghe được,
/// theo âm lượng "Hiệu ứng âm thanh" trong Cài đặt. Mỗi loại có giới hạn tần suất để không bị chồng tiếng khi đánh nhau đông.
/// </summary>
[CreateAssetMenu(fileName = "GameSfx", menuName = "Game/Game Sfx")]
public class GameSfx : ScriptableObject
{
    public enum Kind
    {
        EnemyHit, EnemyDeath, CellHit, CellDeath, AcidWave, LifeLost, ItemUse, Merge, RollSpin, RollLock,
        UIClick, PanelOpen, Place, Coin, Error, Toast, Reward, Lose, Typing
    }

    [Header("Trận đấu")]
    public AudioClip enemyHit;
    public AudioClip enemyDeath;
    public AudioClip cellHit;
    public AudioClip cellDeath;
    public AudioClip acidWave;
    public AudioClip lifeLost;
    [Tooltip("Dùng vật phẩm lối sống trong trận")]
    public AudioClip itemUse;
    [Tooltip("Ghép 2 tế bào thành công")]
    public AudioClip merge;
    [Tooltip("Bấm Roll / khay tự đổi khi sang wave mới")]
    public AudioClip rollSpin;
    [Tooltip("Từng ô khay dừng quay")]
    public AudioClip rollLock;
    [Tooltip("Đặt tế bào xuống ô")]
    public AudioClip place;
    [Tooltip("Nhặt xu ATP / bán tế bào")]
    public AudioClip coin;
    [Tooltip("Thua trận")]
    public AudioClip lose;

    [Header("Giao diện")]
    [Tooltip("Bấm mọi nút")]
    public AudioClip uiClick;
    [Tooltip("Mở bảng (thẻ kiến thức, Sổ tay, Cài đặt…)")]
    public AudioClip panelOpen;
    [Tooltip("Thông báo lỗi / không hợp lệ (thiếu ATP, ô độc tố…)")]
    public AudioClip error;
    [Tooltip("Thông báo mẹo trong trận")]
    public AudioClip toast;
    [Tooltip("Nhận phần thưởng (vật phẩm, sao)")]
    public AudioClip reward;
    [Tooltip("Tiếng gõ chữ khi thoại Scene 1 hiện ra")]
    public AudioClip typing;

    [Header("Âm lượng")]
    [Range(0f, 1f)] public float hitVolume = 0.35f;
    [Range(0f, 1f)] public float eventVolume = 0.8f;
    [Range(0f, 1f)] public float uiVolume = 0.6f;

    [Header("Âm lượng cảnh báo (nhỏ hơn để không gắt tai khi lặp lại nhiều)")]
    [Range(0f, 1f)] public float errorVolume = 0.3f;
    [Range(0f, 1f)] public float toastVolume = 0.25f;
    [Range(0f, 1f)] public float lifeLostVolume = 0.45f;
    [Range(0f, 1f)] public float acidWaveVolume = 0.4f;
    [Range(0f, 1f)] public float typingVolume = 0.15f;

    [Tooltip("Khoảng cách tối thiểu (giây, thời gian thực) giữa 2 lần phát cùng 1 loại âm thanh")]
    public float minInterval = 0.08f;

    [Tooltip("Khoảng cách tối thiểu riêng cho tiếng cảnh báo: thao tác sai nhiều lần liên tiếp chỉ kêu 1 lần")]
    public float warningInterval = 1f;

    [Tooltip("Khoảng cách giữa 2 tiếng gõ chữ (chữ hiện rất nhanh nên không gõ theo từng ký tự)")]
    public float typingInterval = 0.06f;

    private const int VoiceCount = 10;

    private static GameSfx instance;
    private static bool loaded;
    private static readonly Dictionary<Kind, float> lastPlayTime = new Dictionary<Kind, float>();
    private static AudioSource[] voices;
    private static int nextVoice;

    private static GameSfx Instance
    {
        get
        {
            if (!loaded)
            {
                instance = Resources.Load<GameSfx>("GameSfx");
                loaded = true;
            }
            return instance;
        }
    }

    public static void Play(Kind kind)
    {
        GameSfx sfx = Instance;
        if (sfx == null) return;

        AudioClip clip = sfx.GetClip(kind);
        if (clip == null) return;

        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(kind, out float last) && now - last < sfx.IntervalFor(kind)) return;
        lastPlayTime[kind] = now;

        bool isHit = kind == Kind.EnemyHit || kind == Kind.CellHit;
        // Âm trúng đòn và gõ chữ: lệch cao độ ngẫu nhiên nhẹ để đỡ nhàm
        float pitch = isHit ? Random.Range(0.9f, 1.1f) : kind == Kind.Typing ? Random.Range(0.92f, 1.12f) : 1f;
        float volume = sfx.VolumeFor(kind);

        AudioSource voice = NextVoice();
        voice.pitch = pitch;
        voice.PlayOneShot(clip, volume * GameSettings.SfxVolume);
    }

    // Nhóm nguồn 2D dùng xoay vòng (mỗi nguồn 1 cao độ riêng), sống xuyên scene
    private static AudioSource NextVoice()
    {
        if (voices == null || voices[0] == null)
        {
            var go = new GameObject("GameSfxPlayer");
            Object.DontDestroyOnLoad(go);
            voices = new AudioSource[VoiceCount];
            for (int i = 0; i < VoiceCount; i++)
            {
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                voices[i] = source;
            }
        }
        AudioSource voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % VoiceCount;
        return voice;
    }

    private float VolumeFor(Kind kind)
    {
        switch (kind)
        {
            case Kind.EnemyHit:
            case Kind.CellHit: return hitVolume;
            case Kind.UIClick:
            case Kind.PanelOpen: return uiVolume;
            case Kind.Error: return errorVolume;
            case Kind.Toast: return toastVolume;
            case Kind.LifeLost: return lifeLostVolume;
            case Kind.AcidWave: return acidWaveVolume;
            case Kind.Typing: return typingVolume;
            default: return eventVolume;
        }
    }

    private float IntervalFor(Kind kind)
    {
        switch (kind)
        {
            case Kind.Error:
            case Kind.Toast:
            case Kind.LifeLost: return warningInterval;
            case Kind.AcidWave: return 3f; // file dài ~4s, không chồng tiếng
            case Kind.Typing: return typingInterval;
            default: return minInterval;
        }
    }

    private AudioClip GetClip(Kind kind)
    {
        switch (kind)
        {
            case Kind.EnemyHit: return enemyHit;
            case Kind.EnemyDeath: return enemyDeath;
            case Kind.CellHit: return cellHit;
            case Kind.CellDeath: return cellDeath;
            case Kind.AcidWave: return acidWave;
            case Kind.LifeLost: return lifeLost;
            case Kind.ItemUse: return itemUse;
            case Kind.Merge: return merge;
            case Kind.RollSpin: return rollSpin;
            case Kind.RollLock: return rollLock;
            case Kind.UIClick: return uiClick;
            case Kind.PanelOpen: return panelOpen;
            case Kind.Place: return place;
            case Kind.Coin: return coin;
            case Kind.Error: return error;
            case Kind.Toast: return toast;
            case Kind.Reward: return reward;
            case Kind.Lose: return lose;
            case Kind.Typing: return typing;
            default: return null;
        }
    }
}
