using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Danh sách âm thanh hiệu ứng trong trận (asset đặt tại Resources/GameSfx).
/// Phát qua AudioManager nếu có. Mỗi loại âm thanh có giới hạn tần suất để không bị chồng tiếng khi đánh nhau đông.
/// </summary>
[CreateAssetMenu(fileName = "GameSfx", menuName = "Game/Game Sfx")]
public class GameSfx : ScriptableObject
{
    public enum Kind { EnemyHit, EnemyDeath, CellHit, CellDeath, AcidWave, LifeLost, ItemUse, Merge }

    [Header("Âm thanh")]
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

    [Header("Âm lượng")]
    [Range(0f, 1f)] public float hitVolume = 0.35f;
    [Range(0f, 1f)] public float eventVolume = 0.8f;

    [Tooltip("Khoảng cách tối thiểu (giây, thời gian thực) giữa 2 lần phát cùng 1 loại âm thanh")]
    public float minInterval = 0.08f;

    private static GameSfx instance;
    private static bool loaded;
    private static readonly Dictionary<Kind, float> lastPlayTime = new Dictionary<Kind, float>();

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
        if (lastPlayTime.TryGetValue(kind, out float last) && now - last < sfx.minInterval) return;
        lastPlayTime[kind] = now;

        bool isHit = kind == Kind.EnemyHit || kind == Kind.CellHit;
        float volume = isHit ? sfx.hitVolume : sfx.eventVolume;
        // Âm trúng đòn: lệch cao độ ngẫu nhiên nhẹ để đỡ nhàm
        float pitch = isHit ? Random.Range(0.9f, 1.1f) : 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, volume, pitch);
        }
        else if (Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
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
            case Kind.ItemUse: return itemUse;
            case Kind.Merge: return merge;
            case Kind.LifeLost: return lifeLost;
            default: return null;
        }
    }
}
