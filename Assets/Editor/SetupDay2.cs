using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SetupDay2 : Editor
{
    [MenuItem("Tools/Create Day2 Scenario")]
    public static void CreateDay2()
    {
        ScenarioData data = ScriptableObject.CreateInstance<ScenarioData>();

        // Situation 1
        SituationData s1 = new SituationData();
        s1.timeText = "08:00";
        s1.description = "Sáng thức dậy trễ vào cuối tuần, bạn cảm thấy mệt mỏi và lười nấu ăn.";
        
        ChoiceData c1a = new ChoiceData { choiceText = "Uống vội ly cà phê đen đá và nhịn ăn sáng.", delayAfterChoice = 2f };
        c1a.impacts.Add(new ImpactData { targetStat = StatImpactType.GastricAcid, value = 20f });
        c1a.impacts.Add(new ImpactData { targetStat = StatImpactType.MucosaHp, value = -0.2f });
        
        ChoiceData c1b = new ChoiceData { choiceText = "Dậy nấu bát mì tôm kèm rau xanh và trứng.", delayAfterChoice = 2.5f };
        c1b.impacts.Add(new ImpactData { targetStat = StatImpactType.CellDamage, value = 0.1f });
        
        ChoiceData c1c = new ChoiceData { choiceText = "Uống ngũ cốc ấm và ăn bánh mì.", delayAfterChoice = 2f };
        c1c.impacts.Add(new ImpactData { targetStat = StatImpactType.MucosaHp, value = 0.2f });
        c1c.impacts.Add(new ImpactData { targetStat = StatImpactType.AtpRecovery, value = 0.1f });
        
        s1.choices.Add(c1a); s1.choices.Add(c1b); s1.choices.Add(c1c);
        data.situations.Add(s1);

        // Situation 2
        SituationData s2 = new SituationData();
        s2.timeText = "12:30";
        s2.description = "Bạn đi ăn tiệc buffet BBQ. Có rất nhiều thịt nướng trên bàn.";
        
        ChoiceData c2a = new ChoiceData { choiceText = "Ăn thả ga thịt nướng cháy xém và uống bia.", delayAfterChoice = 3f };
        c2a.impacts.Add(new ImpactData { targetStat = StatImpactType.ToxinObstacles, value = 2f });
        c2a.impacts.Add(new ImpactData { targetStat = StatImpactType.GastricAcid, value = 30f });
        
        ChoiceData c2b = new ChoiceData { choiceText = "Ăn thịt nướng kèm theo xà lách, rau củ.", delayAfterChoice = 2.5f };
        c2b.impacts.Add(new ImpactData { targetStat = StatImpactType.CellAttackSpeed, value = 0.2f });
        
        s2.choices.Add(c2a); s2.choices.Add(c2b);
        data.situations.Add(s2);

        // Situation 3
        SituationData s3 = new SituationData();
        s3.timeText = "16:00";
        s3.description = "Đang bị stress, đồng nghiệp rủ order đồ ăn vặt giải khuây.";
        
        ChoiceData c3a = new ChoiceData { choiceText = "Gọi trà sữa trân châu đường đen size L.", delayAfterChoice = 2.5f };
        c3a.impacts.Add(new ImpactData { targetStat = StatImpactType.ToxinObstacles, value = 1f });
        c3a.impacts.Add(new ImpactData { targetStat = StatImpactType.CellDamage, value = -0.1f });
        
        ChoiceData c3b = new ChoiceData { choiceText = "Uống nước lọc và ăn chút trái cây.", delayAfterChoice = 2f };
        c3b.impacts.Add(new ImpactData { targetStat = StatImpactType.AtpRecovery, value = 0.2f });
        
        s3.choices.Add(c3a); s3.choices.Add(c3b);
        data.situations.Add(s3);

        // Situation 4
        SituationData s4 = new SituationData();
        s4.timeText = "22:00";
        s4.description = "Đang cày phim thì bụng sôi sùng sục vì đói.";
        
        ChoiceData c4a = new ChoiceData { choiceText = "Đặt ngay suất gà rán cay và khoai tây chiên.", delayAfterChoice = 3.5f };
        c4a.impacts.Add(new ImpactData { targetStat = StatImpactType.GastricAcid, value = 40f });
        c4a.impacts.Add(new ImpactData { targetStat = StatImpactType.ToxinObstacles, value = 2f });
        
        ChoiceData c4b = new ChoiceData { choiceText = "Cố gắng đi ngủ luôn để dạ dày được nghỉ ngơi.", delayAfterChoice = 2f };
        c4b.impacts.Add(new ImpactData { targetStat = StatImpactType.AtpRecovery, value = 0.3f });
        
        s4.choices.Add(c4a); s4.choices.Add(c4b);
        data.situations.Add(s4);

        if (!System.IO.Directory.Exists("Assets/_Scrip/Data")) {
            AssetDatabase.CreateFolder("Assets/_Scrip", "Data");
        }
        AssetDatabase.CreateAsset(data, "Assets/_Scrip/Data/Day2_FullScenario.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Created Day 2 Scenario successfully.");
    }
}
