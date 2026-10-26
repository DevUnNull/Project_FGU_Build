using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Gán toàn bộ âm thanh mặc định của game:
/// - Resources/GameSfx.asset: tiếng trận đấu + giao diện (file CC0 trong Audio/SFX, nguồn ở Docs/AudioCredits.md)
/// - Resources/MusicLibrary.asset: nhạc nền theo scene (Menu, Map, Scene 1–2; trận đấu dùng nhạc riêng của scene)
/// Chạy lại an toàn: ghi đè các ô về file mặc định.
/// </summary>
public static class AudioSetupTool
{
    private const string Sfx = "Assets/_Game/Audio/SFX/";
    private const string Music = "Assets/_Game/Audio/Music/";

    [MenuItem("Tools/Audio/Assign Default Game Sounds")]
    public static void Assign()
    {
        var log = new StringBuilder();

        GameSfx sfx = AssetDatabase.LoadAssetAtPath<GameSfx>("Assets/Resources/GameSfx.asset");
        sfx.enemyHit = Load(Sfx + "Virus_Hit.mp3", log);
        sfx.enemyDeath = Load(Sfx + "Virus_Pop.mp3", log);
        sfx.cellHit = Load(Sfx + "Cell_Hit.ogg", log);
        sfx.cellDeath = Load(Sfx + "Cell_Death.ogg", log);
        sfx.acidWave = Load(Sfx + "Acid_Bubbles.ogg", log);
        sfx.lifeLost = Load(Sfx + "Heart_Beat.wav", log);
        sfx.itemUse = Load(Sfx + "Item_PowerUp.ogg", log);
        sfx.merge = Load(Sfx + "Merge_LevelUp.ogg", log);
        sfx.rollSpin = Load(Sfx + "Roll_Shake.ogg", log);
        sfx.rollLock = Load(Sfx + "Roll_Lock.ogg", log);
        sfx.place = Load(Sfx + "Cell_Place.ogg", log);
        sfx.coin = Load(Sfx + "Coin_Collect.ogg", log);
        sfx.lose = Load(Music + "Lose game.mp3", log);
        sfx.uiClick = Load(Sfx + "UI_Click.ogg", log);
        sfx.panelOpen = Load(Sfx + "UI_Open.ogg", log);
        sfx.error = Load(Sfx + "UI_Error.ogg", log);
        sfx.toast = Load(Sfx + "UI_Toast.ogg", log);
        sfx.reward = Load(Sfx + "UI_Reward.ogg", log);
        sfx.typing = Load(Sfx + "UI_Type.ogg", log);
        EditorUtility.SetDirty(sfx);

        const string libraryPath = "Assets/Resources/MusicLibrary.asset";
        MusicLibrary library = AssetDatabase.LoadAssetAtPath<MusicLibrary>(libraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<MusicLibrary>();
            AssetDatabase.CreateAsset(library, libraryPath);
        }
        // Nhạc chill CC0 (nguồn ở Docs/AudioCredits.md). Bài dự phòng để đổi: Chill_LofiAgain, Chill_Sherwood, Chill_Heavenly
        AudioClip menu = Load(Music + "Chill_SlowStride.ogg", log);
        AudioClip day = Load(Music + "Chill_LofiLoop.ogg", log);
        library.tracks = new List<MusicLibrary.Track>
        {
            new MusicLibrary.Track { sceneName = "MainMenuScene", clip = menu, volume = 0.5f },
            // Bản đồ → một ngày sinh hoạt → tổng kết: cùng 1 bài lofi, nhạc chạy liền mạch
            new MusicLibrary.Track { sceneName = "MainMap", clip = day, volume = 0.5f },
            new MusicLibrary.Track { sceneName = "Scene1_LifeSimulation", clip = day, volume = 0.35f },
            new MusicLibrary.Track { sceneName = "Scene2_Transition", clip = day, volume = 0.45f },
        };
        library.scenesWithOwnMusic = new List<string> { "Ingame_1", "LuTile" };
        EditorUtility.SetDirty(library);

        AssetDatabase.SaveAssets();
        Debug.Log("[AudioSetupTool] Đã gán âm thanh." + (log.Length > 0 ? "\nThiếu:\n" + log : ""));
    }

    private static AudioClip Load(string path, StringBuilder log)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) log.AppendLine(path);
        return clip;
    }
}
