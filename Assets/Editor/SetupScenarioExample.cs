using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetupScenarioExample : Editor
{
    [MenuItem("Tools/Create Example Scenario")]
    public static void CreateExample()
    {
        // 1. Create ScriptableObject Data
        ScenarioData data = ScriptableObject.CreateInstance<ScenarioData>();
        
        SituationData s1 = new SituationData();
        s1.timeText = "07:30";
        s1.description = "Bụng đói cồn cào sau một đêm dài.";
        
        ChoiceData c1a = new ChoiceData();
        c1a.choiceText = "Ăn Phở bò nóng + Nước ấm";
        ImpactData i1a = new ImpactData();
        i1a.targetStat = StatImpactType.CellDamage;
        i1a.value = 0.2f;
        c1a.impacts.Add(i1a);
        
        ChoiceData c1b = new ChoiceData();
        c1b.choiceText = "Bỏ bữa sáng, lướt điện thoại";
        ImpactData i1b = new ImpactData();
        i1b.targetStat = StatImpactType.GastricAcid;
        i1b.value = 35.0f;
        c1b.impacts.Add(i1b);
        
        s1.choices.Add(c1a);
        s1.choices.Add(c1b);
        data.situations.Add(s1);

        if (!System.IO.Directory.Exists("Assets/_Scrip/Data")) {
            AssetDatabase.CreateFolder("Assets/_Scrip", "Data");
        }
        AssetDatabase.CreateAsset(data, "Assets/_Scrip/Data/Day1_Scenario.asset");
        AssetDatabase.SaveAssets();

        // 2. Open Scene1 and update UI
        string scene1Path = "Assets/_ScenesCallOurMyCell/Scene1_LifeSimulation.unity";
        var scene1 = EditorSceneManager.OpenScene(scene1Path, OpenSceneMode.Single);
        
        Scene1Manager manager = GameObject.FindObjectOfType<Scene1Manager>();
        if (manager != null)
        {
            manager.currentScenario = data;

            Canvas canvas = GameObject.FindObjectOfType<Canvas>();

            // Clean up old UI
            GameObject btnA = GameObject.Find("ChoiceAButton");
            if (btnA != null) DestroyImmediate(btnA);
            GameObject btnB = GameObject.Find("ChoiceBButton");
            if (btnB != null) DestroyImmediate(btnB);

            // Container
            GameObject containerObj = GameObject.Find("ChoicesContainer");
            if (containerObj == null) {
                containerObj = new GameObject("ChoicesContainer");
                containerObj.transform.SetParent(canvas.transform, false);
                RectTransform containerRt = containerObj.AddComponent<RectTransform>();
                containerRt.anchorMin = new Vector2(0.2f, 0.1f);
                containerRt.anchorMax = new Vector2(0.8f, 0.5f);
                containerRt.offsetMin = Vector2.zero;
                containerRt.offsetMax = Vector2.zero;

                VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = true;
                vlg.childControlWidth = true;
                vlg.spacing = 20;
            }
            manager.choicesContainer = containerObj.transform;

            // Prefab
            GameObject btnObj = new GameObject("ChoiceButtonPrefab");
            Image img = btnObj.AddComponent<Image>();
            img.color = Color.white;
            Button btn = btnObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "Choice Text";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;
            tmp.color = Color.black;
            RectTransform txtRt = textObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            if (!System.IO.Directory.Exists("Assets/_Prefabs")) {
                AssetDatabase.CreateFolder("Assets", "_Prefabs");
            }
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(btnObj, "Assets/_Prefabs/ChoiceButton.prefab");
            DestroyImmediate(btnObj);
            
            manager.choiceButtonPrefab = prefab;

            // Illus
            GameObject illObj = GameObject.Find("IllustrationImage");
            if (illObj == null) {
                illObj = new GameObject("IllustrationImage");
                illObj.transform.SetParent(canvas.transform, false);
                Image illImg = illObj.AddComponent<Image>();
                RectTransform illRt = illObj.GetComponent<RectTransform>();
                illRt.anchorMin = new Vector2(0.5f, 0.6f);
                illRt.anchorMax = new Vector2(0.5f, 0.6f);
                illRt.sizeDelta = new Vector2(400, 250);
                illImg.gameObject.SetActive(false);
            }
            manager.illustrationImage = illObj.GetComponent<Image>();

            // Setup References
            manager.timeTextUI = GameObject.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            manager.descriptionTextUI = GameObject.Find("DescText")?.GetComponent<TextMeshProUGUI>();

            EditorSceneManager.SaveScene(scene1, scene1Path);
            Debug.Log("Created Day1_Scenario.asset and updated Scene 1 successfully!");
        }
    }
}
