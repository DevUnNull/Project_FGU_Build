using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sinh nội dung các ngày chơi:
/// - Resources/Days/Day1.asset: trỏ tới kịch bản Day1_v1 + bộ wave TuneConfigMap1_*
/// - Kịch bản Ngày 2 (cuối tuần) + bộ wave Ngày 2 + Resources/Days/Day2.asset
/// Ảnh minh họa/biểu cảm Ngày 2 tạm dùng lại từ Ngày 1 (tìm theo tên situation). Chạy lại nhiều lần vẫn an toàn.
/// </summary>
public static class SetupDay2
{
    private const string DaysFolder = "Assets/Resources/Days";
    private const string Day1ScenarioPath = "Assets/_Game/Data/Scenarios/Scenario_Day1.asset";
    private const string Day2ScenarioPath = "Assets/_Game/Data/Scenarios/Scenario_Day2.asset";
    private const string TuneFolder = "Assets/_Game/Data/Waves/Day1";
    private const string Day2WaveFolder = "Assets/_Game/Data/Waves/Day2";

    [MenuItem("Tools/Days/Generate Day Content")]
    public static void GenerateAll()
    {
        EnsureFolder(DaysFolder);
        EnsureFolder(Day2WaveFolder);

        var day1Scenario = AssetDatabase.LoadAssetAtPath<ScenarioData>(Day1ScenarioPath);
        var day1Tunes = Enumerable.Range(1, 5)
            .Select(i => AssetDatabase.LoadAssetAtPath<TuneConfig>($"{TuneFolder}/TuneConfigMap1_{i}.asset"))
            .ToList();
        SaveDayConfig(1, "Ngày 1: Một ngày đi học", day1Scenario, day1Tunes);

        ScenarioData day2Scenario = CreateDay2Scenario(day1Scenario);
        List<TuneConfig> day2Tunes = CreateDay2Waves();
        SaveDayConfig(2, "Ngày 2: Cuối tuần thả ga", day2Scenario, day2Tunes);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ [Days] Đã sinh nội dung Ngày 1 & Ngày 2 (Resources/Days).");
    }

    // ======================= DAY CONFIG =======================

    private static void SaveDayConfig(int day, string displayName, ScenarioData scenario, List<TuneConfig> tunes)
    {
        string path = $"{DaysFolder}/Day{day}.asset";
        var config = AssetDatabase.LoadAssetAtPath<DayConfig>(path);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<DayConfig>();
            AssetDatabase.CreateAsset(config, path);
        }
        config.dayNumber = day;
        config.displayName = displayName;
        config.scenario = scenario;
        config.tunes = tunes.Where(t => t != null).ToList();
        EditorUtility.SetDirty(config);
    }

    // ======================= KỊCH BẢN NGÀY 2 =======================

    private class Option
    {
        public string text;
        public string tip;
        public (StatImpactType stat, float value)[] impacts;
        public string reply; // câu thoại sau khi chọn (để trống = không có)
    }

    private static ScenarioData CreateDay2Scenario(ScenarioData day1)
    {
        var data = AssetDatabase.LoadAssetAtPath<ScenarioData>(Day2ScenarioPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<ScenarioData>();
            AssetDatabase.CreateAsset(data, Day2ScenarioPath);
        }
        data.situations = new List<SituationData>();

        SituationData start = Narration("Start", "00:00", "...", new[] { "Ngày 2 – Cuối tuần", "Ấn để bắt đầu ngày" }, day1, "Start");

        SituationData s1 = Scene("CẢNH 1: 08:00 SÁNG – CUỐI TUẦN DẬY TRỄ", "08:00", day1, "CẢNH 1",
            new[] { "Ưm... 8 giờ rồi à? Cuối tuần mà sao người vẫn mệt rã rời thế này.", "Lười nấu quá... ăn gì bây giờ nhỉ?" },
            new Option
            {
                text = "Uống vội ly cà phê đen đá, nhịn ăn sáng",
                impacts = new[] { (StatImpactType.Toxicity, 10f), (StatImpactType.Immunity, -10f), (StatImpactType.Energy, 5f) },
                tip = "Cà phê không thay được bữa sáng. Uống cà phê đậm khi bụng đói dễ gây cồn cào, khó chịu dạ dày và làm bạn mệt trở lại nhanh.",
            },
            new Option
            {
                text = "Nấu bát mì tôm, thêm rau xanh và trứng",
                impacts = new[] { (StatImpactType.Immunity, 10f), (StatImpactType.Energy, 10f) },
                tip = "Thêm rau và trứng giúp bát mì đủ chất hơn. Nhưng mì gói nhiều muối, không nên ăn thường xuyên.",
            },
            new Option
            {
                text = "Ăn ngũ cốc ấm, bánh mì và uống sữa",
                impacts = new[] { (StatImpactType.Immunity, 15f), (StatImpactType.Energy, 10f), (StatImpactType.Hydration, 5f) },
                tip = "Bữa sáng có tinh bột, đạm và sữa giúp no lâu và cung cấp năng lượng ổn định cho buổi sáng.",
            });

        SituationData s2 = Scene("CẢNH 2: 12:30 TRƯA – BUFFET NƯỚNG CÙNG BẠN", "12:30", day1, "4.B",
            new[] { "Hội bạn rủ đi buffet nướng, bàn nào cũng đầy thịt!", "Ăn sao cho đáng tiền đây ta..." },
            new Option
            {
                text = "Ăn thả ga thịt nướng cháy xém, uống bia",
                impacts = new[] { (StatImpactType.Toxicity, 25f), (StatImpactType.Hydration, -15f), (StatImpactType.Immunity, -10f) },
                tip = "Phần thịt nướng cháy đen chứa chất có hại cho cơ thể. Bia rượu làm mất nước và tăng gánh nặng cho gan, dạ dày.",
            },
            new Option
            {
                text = "Ăn thịt nướng kèm nhiều rau, uống nước lọc",
                impacts = new[] { (StatImpactType.Hydration, 15f), (StatImpactType.Immunity, 10f) },
                tip = "Rau xanh giàu chất xơ giúp tiêu hóa tốt hơn khi ăn nhiều thịt. Nhớ bỏ phần bị cháy đen.",
            });

        SituationData s3 = Scene("CẢNH 3: 16:00 CHIỀU – CĂNG THẲNG DEADLINE", "16:00", day1, "CẢNH 3",
            new[] { "Đồ án sắp tới hạn, đầu óc căng như dây đàn...", "Có nên làm gì đó cho đỡ stress không?" },
            new Option
            {
                text = "Gọi trà sữa trân châu đường đen size L",
                impacts = new[] { (StatImpactType.Toxicity, 10f), (StatImpactType.Energy, 10f), (StatImpactType.Hydration, -10f) },
                tip = "Một ly trà sữa lớn có thể chứa lượng đường vượt mức khuyến nghị cả ngày. Đường giúp vui tạm thời rồi khiến bạn mệt hơn.",
            },
            new Option
            {
                text = "Uống nước, ăn trái cây, đi dạo 15 phút",
                impacts = new[] { (StatImpactType.Hydration, 20f), (StatImpactType.Recovery, 10f) },
                tip = "Đi bộ nhẹ giúp giảm căng thẳng. Trái cây và nước lọc bổ sung vitamin, nước mà không làm tăng đường đột ngột.",
            });

        SituationData s4 = Scene("CẢNH 4: 22:00 ĐÊM – CÀY PHIM VÀ CƠN ĐÓI", "22:00", day1, "CẢNH 5",
            new[] { "Đang tới tập hay thì bụng sôi ùng ục...", "Mai được nghỉ, thức khuya tí chắc không sao đâu nhỉ?" },
            new Option
            {
                text = "Đặt gà rán cay, khoai chiên, xem tới 2 giờ sáng",
                impacts = new[] { (StatImpactType.Toxicity, 25f), (StatImpactType.Recovery, -20f), (StatImpactType.Immunity, -10f) },
                tip = "Ăn khuya đồ chiên, cay dễ gây đầy bụng, trào ngược axit và khó ngủ. Thức khuya làm cơ thể không kịp phục hồi.",
            },
            new Option
            {
                text = "Uống cốc sữa ấm, ăn quả chuối rồi đi ngủ",
                impacts = new[] { (StatImpactType.Recovery, 20f), (StatImpactType.Immunity, 5f) },
                tip = "Nếu đói về khuya, chọn món nhẹ, dễ tiêu. Ngủ đủ giấc là lúc cơ thể sửa chữa và tăng cường miễn dịch.",
            });

        // Nối các cảnh: Start -> s1 -> s2 -> s3 -> s4 -> END
        start.autoTargetType = TargetType.Situation;
        start.autoTargetGuid = s1.guid;
        LinkChoices(s1, s2);
        LinkChoices(s2, s3);
        LinkChoices(s3, s4);
        foreach (var c in s4.choices) c.targetType = TargetType.EndScene;

        data.situations.AddRange(new[] { start, s1, s2, s3, s4 });
        data.entryGuid = start.guid;

        // Vị trí node cho Scenario Graph Editor
        for (int i = 0; i < data.situations.Count; i++)
            data.situations[i].editorPosition = new Vector2(i * 350f, 100f);

        EditorUtility.SetDirty(data);
        return data;
    }

    private static void LinkChoices(SituationData from, SituationData to)
    {
        foreach (var c in from.choices)
        {
            c.targetType = TargetType.Situation;
            c.targetGuid = to.guid;
        }
    }

    private static SituationData Narration(string id, string time, string speaker, string[] lines, ScenarioData day1, string artFromDay1)
    {
        SituationData sit = NewSituation(id, time, day1, artFromDay1);
        foreach (string line in lines)
            sit.dialogueLines.Add(new DialogueLineData { speakerName = speaker, text = line, advanceMode = DialogueAdvanceMode.WaitForPlayer });
        return sit;
    }

    private static SituationData Scene(string id, string time, ScenarioData day1, string artFromDay1, string[] lines, params Option[] options)
    {
        SituationData sit = NewSituation(id, time, day1, artFromDay1);
        foreach (string line in lines)
            sit.dialogueLines.Add(new DialogueLineData { speakerName = "Tôi", text = line, advanceMode = DialogueAdvanceMode.WaitForPlayer });

        foreach (Option o in options)
        {
            var choice = new ChoiceData { choiceText = o.text, delayAfterChoice = 1.5f, knowledgeTip = o.tip };
            foreach (var (stat, value) in o.impacts)
                choice.impacts.Add(new ImpactData { targetStat = stat, value = value });
            sit.choices.Add(choice);
        }
        return sit;
    }

    /// <summary>Tạo situation mới, sao chép ảnh nền & nhân vật từ situation Ngày 1 có tên bắt đầu bằng artFromDay1.</summary>
    private static SituationData NewSituation(string id, string time, ScenarioData day1, string artFromDay1)
    {
        var sit = new SituationData
        {
            guid = System.Guid.NewGuid().ToString(),
            situationId = id,
            timeText = time,
            mediaType = BackgroundMediaType.Image,
            hasMigratedDialogues = true,
        };

        SituationData art = day1 != null ? day1.situations.FirstOrDefault(s => s.situationId.StartsWith(artFromDay1)) : null;
        if (art != null)
        {
            sit.illustration = art.illustration;
            sit.descriptionBgSprite = art.descriptionBgSprite;
            sit.characterPose = art.characterPose;
            sit.characterExpression = art.characterExpression;
            sit.characterHair = art.characterHair;
        }
        return sit;
    }

    // ======================= WAVE NGÀY 2 =======================

    // Mỗi phần tử: (lượt, làn 1..5, loại quái, số lượng, khoảng cách sinh)
    private static readonly (int round, int lane, EnemyType type, int count, float interval)[] Day2Waves =
    {
        (1, 1, EnemyType.Corona, 2, 4f), (1, 3, EnemyType.Corona, 2, 4f),
        (2, 2, EnemyType.Ecoli, 2, 4f), (2, 4, EnemyType.Corona, 2, 3f),
        (3, 1, EnemyType.Corona, 2, 3f), (3, 1, EnemyType.Staphyloccot, 1, 3f), (3, 5, EnemyType.Corona, 2, 3f),
        (4, 2, EnemyType.Corona, 2, 3f), (4, 3, EnemyType.Ecoli, 2, 3f), (4, 4, EnemyType.Staphyloccot, 1, 3f),
        (5, 1, EnemyType.Corona, 3, 2.5f), (5, 3, EnemyType.Staphyloccot, 1, 3f), (5, 5, EnemyType.Ecoli, 2, 3f),
        (6, 2, EnemyType.Staphyloccot, 2, 3f), (6, 4, EnemyType.Corona, 3, 2.5f),
        (7, 1, EnemyType.Ecoli, 2, 2.5f), (7, 3, EnemyType.Ecoli, 2, 2.5f), (7, 5, EnemyType.Staphyloccot, 2, 3f),
        (8, 1, EnemyType.Corona, 2, 2f), (8, 1, EnemyType.Staphyloccot, 1, 2f),
        (8, 2, EnemyType.Ecoli, 2, 2f), (8, 2, EnemyType.Staphyloccot, 1, 2f),
        (8, 3, EnemyType.Corona, 3, 2f),
        (8, 4, EnemyType.Staphyloccot, 2, 2.5f),
        (8, 5, EnemyType.Ecoli, 2, 2f), (8, 5, EnemyType.Corona, 2, 2f),
    };

    private static List<TuneConfig> CreateDay2Waves()
    {
        int rounds = Day2Waves.Max(w => w.round);
        var tunes = new List<TuneConfig>();

        for (int lane = 1; lane <= 5; lane++)
        {
            string tunePath = $"{Day2WaveFolder}/TuneConfigDay2_{lane}.asset";
            var tune = LoadOrCreate<TuneConfig>(tunePath);
            tune.waves = new List<WaveConfig>();

            for (int round = 1; round <= rounds; round++)
            {
                var wave = LoadOrCreate<WaveConfig>($"{Day2WaveFolder}/WaveConfigDay2_L{lane}_R{round}.asset");
                wave.pathID = (PathID)(lane - 1);
                wave.groups = Day2Waves
                    .Where(w => w.round == round && w.lane == lane)
                    .Select(w => new WaveConfig.SpawnGroup { type = w.type, count = w.count, interval = w.interval })
                    .ToList();
                wave.isEmptyWave = wave.groups.Count == 0;
                EditorUtility.SetDirty(wave);
                tune.waves.Add(wave);
            }

            EditorUtility.SetDirty(tune);
            tunes.Add(tune);
        }
        return tunes;
    }

    // ======================= TIỆN ÍCH =======================

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
