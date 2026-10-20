using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Báo cáo cân bằng theo ngày: duyệt MỌI nhánh lựa chọn trong ScenarioData, tính chỉ số cuối ngày (DayStats)
/// rồi quy ra số liệu trận đấu và "chỉ số sức mạnh" ước tính so với tổng máu quái.
/// Chỉ số sức mạnh là thước đo TƯƠNG ĐỐI để so sánh các nhánh, không thay thế chơi thử.
/// Chạy: Tools → Balance → Day Reports. Mỗi ngày (Resources/Days/Day{n}) ghi ra Assets/_Game/Docs/BalanceReport_Day{n}.md
/// </summary>
public static class BalanceReportTool
{
    private const string HeroDataFolder = "Assets/_Game/Data/Heroes";

    // Thông số nền lấy từ Inspector của Ingame_1 (GoldManager, LifeManager, WaveManager)
    private const float BaseStartATP = 250f;
    private const float AtpPerTick = 5f;
    private const float AtpTickSeconds = 4f;
    private const float BaseHeartLife = 10f;
    private const float DelayBetweenWaves = 30f;
    private const float AttackClipLength = 0.55f; // độ dài clip tấn công (sau khi sửa frame cuối)

    private struct PathResult
    {
        public List<string> choices;
        public DayStats stats;
    }

    [MenuItem("Tools/Balance/Day Reports")]
    public static void GenerateAllReports()
    {
        int generated = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:DayConfig"))
        {
            var config = AssetDatabase.LoadAssetAtPath<DayConfig>(AssetDatabase.GUIDToAssetPath(guid));
            if (config == null || config.scenario == null) continue;
            GenerateReport(config);
            generated++;
        }
        if (generated == 0) Debug.LogWarning("[Balance] Chưa có DayConfig nào. Chạy Tools → Days → Generate Day Content trước.");
    }

    private static void GenerateReport(DayConfig config)
    {
        ScenarioData scenario = config.scenario;
        string reportPath = $"Assets/_Game/Docs/BalanceReport_Day{config.dayNumber}.md";

        List<PathResult> paths = EnumeratePaths(scenario);
        float enemyHp = CalculateEnemyLoad(config.tunes, out int enemyCount, out float rewardTotal, out int rounds);
        float battleSeconds = rounds * DelayBetweenWaves;
        List<HeroData> heroes = LoadHeroData();

        var rows = paths.Select(p => new
        {
            path = p,
            power = EstimatePower(p.stats, heroes, battleSeconds, rewardTotal, enemyHp)
        }).OrderByDescending(r => r.path.stats.HealthScore).ToList();

        float bestPower = rows.Max(r => r.power);
        float worstPower = rows.Min(r => r.power);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"# Báo cáo cân bằng – {config.displayName}");
        sb.AppendLine();
        sb.AppendLine("> Tự sinh bởi **Tools → Balance → Day Reports** (`Assets/Editor/BalanceReportTool.cs`). Không sửa tay.");
        sb.AppendLine();
        sb.AppendLine($"- Số nhánh lựa chọn: **{paths.Count}**");
        sb.AppendLine($"- Tổng mầm bệnh: **{enemyCount}** con, tổng máu **{enemyHp:0}**, thưởng diệt tối đa **{rewardTotal:0} ATP**");
        sb.AppendLine($"- Thời lượng trận ước tính: **{battleSeconds:0}s** ({rounds} lượt × {DelayBetweenWaves:0}s)");
        sb.AppendLine($"- Chỉ số sức mạnh = (tổng ATP trong trận × sát thương/giây trên mỗi ATP của tế bào hiệu quả nhất × thời gian triển khai trung bình) ÷ tổng máu quái. **> 1.0 nghĩa là về lý thuyết đủ sức tiêu diệt hết quái.**");
        sb.AppendLine();
        sb.AppendLine($"**Khoảng sức mạnh:** {worstPower:0.00} (nhánh yếu nhất) → {bestPower:0.00} (nhánh mạnh nhất), chênh lệch ×{(worstPower > 0 ? bestPower / worstPower : 0):0.0}.");
        sb.AppendLine();
        sb.AppendLine("| # | Lựa chọn | Kháng thể | Thể lực | Nước | Hồi phục | Ô khóa | Axit | Điểm | Scene 2 | ATP đầu | Máu tim | Sức mạnh |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|");

        int i = 1;
        foreach (var r in rows)
        {
            DayStats s = r.path.stats;
            string tier = s.HealthScore >= 0.3f ? "Tốt" : (s.HealthScore >= -0.3f ? "Trung bình" : "Xấu");
            string choices = string.Join(" → ", r.path.choices);
            sb.AppendLine($"| {i++} | {choices} | {Pct(s.immunity)} | {Pct(s.energy)} | {Pct(s.hydration)} | {Pct(s.recovery)} | " +
                          $"{s.LockedTiles} | {(s.HasGastricAcidWave ? "Có" : "–")} | {s.HealthScore:+0.00;-0.00;0.00} | {tier} | " +
                          $"{BaseStartATP * s.energy:0} | {Mathf.RoundToInt(BaseHeartLife * s.recovery)} | {r.power:0.00} |");
        }

        sb.AppendLine();
        sb.AppendLine("## Gợi ý đọc báo cáo");
        sb.AppendLine("- Mô hình **không** tính tế bào bị đánh chết, sát thương của quái hay vị trí đặt quân, nên giá trị tuyệt đối bị thổi phồng. **Chỉ so sánh tỉ lệ giữa các nhánh.**");
        sb.AppendLine("- Cách hiệu chỉnh: chơi thử nhánh yếu nhất và nhánh mạnh nhất, ghi lại thắng/thua và máu còn lại, rồi lấy đó làm mốc cho cột *Sức mạnh*.");
        sb.AppendLine("- Chênh lệch giữa nhánh mạnh nhất và yếu nhất càng lớn thì hậu quả lối sống càng rõ, nhưng nếu quá lớn thì nhánh tốt quá dễ còn nhánh xấu gần như không thể thắng.");

        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.ImportAsset(reportPath);
        Debug.Log($"✅ [Balance] Đã ghi báo cáo {paths.Count} nhánh vào {reportPath}. Sức mạnh {worstPower:0.00} → {bestPower:0.00}");
    }

    private static string Pct(float multiplier) => $"{Mathf.RoundToInt(multiplier * 100f)}%";

    // ===================== DUYỆT NHÁNH KỊCH BẢN =====================

    private static List<PathResult> EnumeratePaths(ScenarioData scenario)
    {
        var byGuid = scenario.situations.Where(s => !string.IsNullOrEmpty(s.guid))
            .GroupBy(s => s.guid).ToDictionary(g => g.Key, g => g.First());
        var results = new List<PathResult>();
        Walk(byGuid, scenario.entryGuid, DayStats.Default, new List<string>(), results, 0);
        return results;
    }

    private static void Walk(Dictionary<string, SituationData> byGuid, string guid, DayStats stats,
        List<string> choices, List<PathResult> results, int depth)
    {
        // Chống vòng lặp vô hạn trong đồ thị kịch bản
        if (depth > 64 || string.IsNullOrEmpty(guid) || guid == "END" || !byGuid.TryGetValue(guid, out SituationData sit))
        {
            results.Add(new PathResult { choices = new List<string>(choices), stats = stats });
            return;
        }

        if (sit.choices == null || sit.choices.Count == 0)
        {
            if (sit.autoTargetType == TargetType.EndScene)
                results.Add(new PathResult { choices = new List<string>(choices), stats = stats });
            else
                Walk(byGuid, sit.autoTargetGuid, stats, choices, results, depth + 1);
            return;
        }

        foreach (ChoiceData choice in sit.choices)
        {
            DayStats next = stats;
            if (choice.impacts != null)
            {
                foreach (ImpactData impact in choice.impacts) next.ApplyImpact(impact.targetStat, impact.value);
            }

            choices.Add(ShortLabel(choice.choiceText));
            if (choice.targetType == TargetType.EndScene)
                results.Add(new PathResult { choices = new List<string>(choices), stats = next });
            else
                Walk(byGuid, choice.targetGuid, next, choices, results, depth + 1);
            choices.RemoveAt(choices.Count - 1);
        }
    }

    private static string ShortLabel(string text)
    {
        string t = (text ?? "").Replace("\n", " ").Replace("|", "/").Trim();
        int cut = t.IndexOfAny(new[] { ',', ':', '+' });
        if (cut > 8) t = t.Substring(0, cut);
        return t.Length > 28 ? t.Substring(0, 28).TrimEnd() + "…" : t;
    }

    // ===================== TẢI QUÁI & TẾ BÀO =====================

    private static float CalculateEnemyLoad(List<TuneConfig> dayTunes, out int count, out float rewardTotal, out int rounds)
    {
        count = 0;
        rewardTotal = 0f;
        float totalHp = 0f;
        Dictionary<EnemyType, EnemyData> enemyByType = LoadEnemyDataByType();

        var tunes = dayTunes.Where(t => t != null).ToList();

        int maxWaves = tunes.Count > 0 ? tunes.Max(t => t.waves.Count) : 0;
        rounds = 0;
        for (int w = 0; w < maxWaves; w++)
        {
            bool roundHasEnemies = false;
            foreach (TuneConfig tune in tunes)
            {
                if (w >= tune.waves.Count || tune.waves[w] == null || tune.waves[w].IsEmptyWave()) continue;
                foreach (var group in tune.waves[w].groups)
                {
                    if (group.type == EnemyType.None || group.count <= 0) continue;
                    if (!enemyByType.TryGetValue(group.type, out EnemyData data)) continue;
                    count += group.count;
                    totalHp += data.health * group.count;
                    rewardTotal += data.rewardGold * group.count;
                    roundHasEnemies = true;
                }
            }
            if (roundHasEnemies) rounds++;
        }
        return totalHp;
    }

    private static Dictionary<EnemyType, EnemyData> LoadEnemyDataByType()
    {
        var result = new Dictionary<EnemyType, EnemyData>();
        foreach (string guid in AssetDatabase.FindAssets("t:PoolConfig"))
        {
            var config = AssetDatabase.LoadAssetAtPath<PoolConfig>(AssetDatabase.GUIDToAssetPath(guid));
            if (config == null) continue;
            foreach (var entry in config.pools)
            {
                if (entry.prefab == null) continue;
                _Enemy enemy = entry.prefab.GetComponent<_Enemy>();
                if (enemy == null) continue;
                var data = new SerializedObject(enemy).FindProperty("enemyData")?.objectReferenceValue as EnemyData;
                if (data != null) result[entry.type] = data;
            }
        }
        return result;
    }

    private static List<HeroData> LoadHeroData()
    {
        return AssetDatabase.FindAssets("t:HeroData", new[] { HeroDataFolder })
            .Select(g => AssetDatabase.LoadAssetAtPath<HeroData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(h => h != null).ToList();
    }

    // ===================== ƯỚC TÍNH SỨC MẠNH =====================

    private static float EstimatePower(DayStats s, List<HeroData> heroes, float battleSeconds, float rewardTotal, float enemyHp)
    {
        // Tổng ATP: khởi điểm + hồi theo thời gian (nhanh hơn khi Energy cao) + thưởng diệt quái
        float startAtp = BaseStartATP * s.energy;
        float passiveAtp = battleSeconds / (AtpTickSeconds / Mathf.Max(0.1f, s.energy)) * AtpPerTick;
        float totalAtp = startAtp + passiveAtp + rewardTotal;

        // Hiệu suất sát thương/giây trên mỗi ATP của tế bào tấn công tốt nhất
        float bestDpsPerAtp = 0f;
        foreach (HeroData h in heroes)
        {
            if (h.damage <= 0 || h.price <= 0) continue;
            float speed = h.affectedByHydration ? Mathf.Max(0.1f, s.hydration) : 1f;
            float cycle = (Mathf.Max(0.1f, h.attackInterval) + AttackClipLength) / speed;
            float dps = h.damage * s.immunity / cycle;
            bestDpsPerAtp = Mathf.Max(bestDpsPerAtp, dps / h.price);
        }

        // Tế bào được mua dần nên trung bình chỉ đánh ~nửa trận
        float potentialDamage = totalAtp * bestDpsPerAtp * battleSeconds * 0.5f;

        // Ô bị khóa làm giảm chỗ đặt quân (30 ô có thể khóa)
        float tilePenalty = 1f - Mathf.Clamp01(s.LockedTiles / 30f) * 0.5f;

        return enemyHp > 0f ? potentialDamage * tilePenalty / enemyHp : 0f;
    }
}
