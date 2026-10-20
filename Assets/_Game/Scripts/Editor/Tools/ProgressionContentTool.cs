using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tạo / cập nhật nội dung cho Sổ tay sức khỏe, vật phẩm lối sống và hình ngôi sao:
/// - Resources/NotebookCatalog.asset (trang Tế bào, Mầm bệnh)
/// - Resources/ItemCatalog.asset (4 vật phẩm + nền ô)
/// - UISkin.starFilled/starEmpty/levelBadge, GameSfx.itemUse/merge
/// Chạy lại an toàn: ghi đè nội dung mặc định, không đụng scene.
/// </summary>
public static class ProgressionContentTool
{
    private const string UnitSpriteFolder = "Assets/_Game/Art/Sprites/Units";
    private const string IconFolder = "Assets/ThirdParty/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored";

    [MenuItem("Tools/Progression/Build Notebook & Item Catalogs")]
    public static void Build()
    {
        var log = new StringBuilder();
        BuildNotebook(log);
        BuildItems(log);
        AssignStarsAndSfx(log);
        AssetDatabase.SaveAssets();
        Debug.Log("[ProgressionContentTool]\n" + log);
    }

    private static void BuildNotebook(StringBuilder log)
    {
        NotebookCatalog catalog = LoadOrCreate<NotebookCatalog>("Assets/Resources/NotebookCatalog.asset");
        catalog.heroes = new List<NotebookCatalog.UnitEntry>
        {
            Unit("Cell_HongCau", "Hồng cầu", "Hong Cau",
                "Hồng cầu chở oxy từ phổi đi khắp cơ thể nhờ huyết sắc tố (hemoglobin). Trong trận, Hồng cầu tạo ATP – năng lượng để gọi thêm tế bào. Uống đủ nước giúp Hồng cầu làm việc nhanh hơn."),
            Unit("Cell_BachCau", "Bạch cầu", "Bach Cau",
                "Bạch cầu trung tính là lính tuần tra: lao tới ổ nhiễm khuẩn đầu tiên và tiêu diệt vi khuẩn. Khi bạn bị sốt, đó thường là lúc bạch cầu đang chiến đấu. Đánh cận chiến, máu dày."),
            Unit("Cell_ThucBao", "Đại thực bào", "ĐẠI THỰC BÀO",
                "Đại thực bào \"nuốt\" và tiêu hóa mầm bệnh cùng xác tế bào chết, rồi báo động cho các tế bào miễn dịch khác. Là tấm khiên chắn đường cho cả đội."),
            Unit("Cell_B", "Tế bào B", "TẾ BÀO B",
                "Tế bào B (lympho B) sản xuất kháng thể – những \"viên đạn\" bám vào mầm bệnh để vô hiệu hóa chúng. Tiêm vắc-xin giúp cơ thể có sẵn tế bào B ghi nhớ mầm bệnh."),
        };
        catalog.enemies = new List<NotebookCatalog.UnitEntry>
        {
            Unit("VirusCumA", "Virus cúm A", "V_CÚM A",
                "Virus cúm A lây qua giọt bắn khi ho, hắt hơi, gây sốt, đau họng, mệt mỏi. Đeo khẩu trang, rửa tay thường xuyên và tiêm vắc-xin cúm hằng năm giúp phòng bệnh."),
            Unit("VirusEcoli", "Vi khuẩn E. coli", "V_Ecoli",
                "Phần lớn E. coli sống hiền lành trong ruột, nhưng một số chủng gây tiêu chảy, đau bụng khi ăn đồ bẩn hoặc chưa nấu chín. Ăn chín uống sôi, rửa tay trước khi ăn để phòng."),
            Unit("VirusPheCau", "Phế cầu khuẩn", "V_PHẾ CẦU KHUẨN",
                "Phế cầu khuẩn có thể gây viêm phổi, viêm tai giữa, viêm màng não; trẻ nhỏ và người lớn tuổi dễ mắc nặng. Có vắc-xin phế cầu để phòng bệnh."),
        };
        EditorUtility.SetDirty(catalog);

        foreach (var e in catalog.heroes) if (e.icon == null) log.AppendLine("Thiếu icon tế bào: " + e.assetName);
        foreach (var e in catalog.enemies) if (e.icon == null) log.AppendLine("Thiếu icon mầm bệnh: " + e.assetName);
        log.AppendLine($"NotebookCatalog: {catalog.heroes.Count} tế bào, {catalog.enemies.Count} mầm bệnh");
    }

    private static void BuildItems(StringBuilder log)
    {
        ItemCatalog catalog = LoadOrCreate<ItemCatalog>("Assets/Resources/ItemCatalog.asset");
        catalog.items = new List<ItemCatalog.Entry>
        {
            Item(LifestyleItem.WaterBottle, "Bình nước", "Potion/Potion - Blue.png",
                $"ATP sinh ra nhanh gấp đôi trong {BattleBuffs.WaterDuration:0} giây. Uống đủ nước giúp máu lưu thông và hồng cầu làm việc tốt."),
            Item(LifestyleItem.VitaminC, "Vitamin C", "Food/Apple.png",
                $"Tế bào gây thêm {Mathf.RoundToInt((BattleBuffs.VitaminDamage - 1f) * 100f)}% sát thương trong {BattleBuffs.VitaminDuration:0} giây. Rau xanh, trái cây giàu vitamin C hỗ trợ hệ miễn dịch."),
            Item(LifestyleItem.GoodSleep, "Giấc ngủ ngon", "Heart - Health/Heart - Red.png",
                $"Hồi {BattleBuffs.SleepHeartHeal} máu tim và hồi đầy máu mọi tế bào trên bàn. Ngủ đủ giấc là lúc cơ thể tự sửa chữa."),
            Item(LifestyleItem.WarmSoup, "Bữa ăn ấm", "Food/Pudding.png",
                $"Nhận ngay {BattleBuffs.SoupAtp} ATP. Bữa ăn đủ chất cho cơ thể năng lượng để chống lại mầm bệnh."),
        };
        catalog.slotBackground = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/GUIPackCartoon/Demo/Sprites/Buttons/Circles/Neutral.png");
        EditorUtility.SetDirty(catalog);

        foreach (var e in catalog.items) if (e.icon == null) log.AppendLine("Thiếu icon vật phẩm: " + e.type);
        if (catalog.slotBackground == null) log.AppendLine("Thiếu nền ô vật phẩm");
        log.AppendLine($"ItemCatalog: {catalog.items.Count} vật phẩm");
    }

    private static void AssignStarsAndSfx(StringBuilder log)
    {
        UISkin skin = AssetDatabase.LoadAssetAtPath<UISkin>("Assets/Resources/UISkin.asset");
        if (skin != null)
        {
            skin.starFilled = AssetDatabase.LoadAssetAtPath<Sprite>(IconFolder + "/Stars/Star Yellow.png");
            skin.starEmpty = AssetDatabase.LoadAssetAtPath<Sprite>(IconFolder + "/Stars/Star Gray.png");
            skin.levelBadge = LoadSubSprite("Assets/_Game/Art/UI/Battle/level_Charactor.png", "level_Charactor_0");

            const string settingFolder = "Assets/_Game/Art/UI/Setting/";
            skin.sliderTrack = LoadSubSprite(settingFolder + "BackGroundBar.png", "BackGroundBar_0");
            skin.sliderFill = LoadSubSprite(settingFolder + "Fill.png", "Fill_0");
            skin.sliderKnob = LoadSubSprite(settingFolder + "Knob.png", "Knob_0");
            skin.musicIcon = LoadSubSprite(settingFolder + "Music.png", "Music_0");
            skin.sfxIcon = LoadSubSprite(settingFolder + "EFX.png", "EFX_0");
            skin.heartIcon = LoadSubSprite("Assets/_Game/Art/UI/Battle/tym.png", "tym_0");
            skin.atpIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/HongCauDroppedCoin.png");
            log.AppendLine($"UISkin setting sprites: {(skin.sliderTrack && skin.sliderFill && skin.sliderKnob && skin.musicIcon && skin.sfxIcon ? "OK" : "THIẾU")}, heart: {(skin.heartIcon ? "OK" : "THIẾU")}, atp: {(skin.atpIcon ? "OK" : "THIẾU")}");
            EditorUtility.SetDirty(skin);
            log.AppendLine($"UISkin stars: {(skin.starFilled != null && skin.starEmpty != null ? "OK" : "THIẾU")}, levelBadge: {(skin.levelBadge != null ? skin.levelBadge.name : "THIẾU")}");
        }

        GameSfx sfx = AssetDatabase.LoadAssetAtPath<GameSfx>("Assets/Resources/GameSfx.asset");
        if (sfx != null)
        {
            if (sfx.itemUse == null) sfx.itemUse = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/coin_selling.mp3");
            if (sfx.merge == null) sfx.merge = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Game/Audio/SFX/Champion-buy.mp3");
            EditorUtility.SetDirty(sfx);
            log.AppendLine($"GameSfx.itemUse: {(sfx.itemUse != null ? "OK" : "THIẾU")}, merge: {(sfx.merge != null ? "OK" : "THIẾU")}");
        }
    }

    private static NotebookCatalog.UnitEntry Unit(string assetName, string displayName, string spriteName, string description)
    {
        return new NotebookCatalog.UnitEntry
        {
            assetName = assetName,
            displayName = displayName,
            icon = FindSprite(UnitSpriteFolder, spriteName),
            description = description
        };
    }

    private static ItemCatalog.Entry Item(LifestyleItem type, string displayName, string iconPath, string effectText)
    {
        return new ItemCatalog.Entry
        {
            type = type,
            displayName = displayName,
            icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconFolder + "/" + iconPath),
            effectText = effectText
        };
    }

    // Tên file tiếng Việt có thể lưu dạng Unicode tổ hợp (NFD) -> so khớp sau khi chuẩn hóa
    private static Sprite FindSprite(string folder, string fileNameWithoutExt)
    {
        string wanted = fileNameWithoutExt.Normalize(NormalizationForm.FormC);
        foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path).Normalize(NormalizationForm.FormC);
            if (name == wanted) return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return null;
    }

    // Ảnh cắt nhiều sprite (Sprite Mode = Multiple): lấy đúng sprite con theo tên
    private static Sprite LoadSubSprite(string path, string spriteName)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is Sprite sprite && sprite.name == spriteName) return sprite;
        }
        return null;
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
