using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class SetupSceneDescriptionPanel
{
    [MenuItem("Tools/Visual Novel/Setup Active Scene Description Panel")]
    public static void Setup()
    {
        ScenarioPlayer player = Object.FindFirstObjectByType<ScenarioPlayer>();
        if (player == null)
        {
            Debug.LogError("Không tìm thấy Component ScenarioPlayer trong Scene đang mở!");
            return;
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Component Canvas trong Scene đang mở!");
            return;
        }

        // Tìm hoặc tạo mới DescriptionPanel
        Transform panelTransform = canvas.transform.Find("DescriptionPanel");
        GameObject panelObj = null;

        if (panelTransform == null)
        {
            panelObj = new GameObject("DescriptionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(950f, 220f);
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 40f);

            Transform choices = canvas.transform.Find("ChoicesContainer");
            if (choices != null)
            {
                panelObj.transform.SetSiblingIndex(choices.GetSiblingIndex());
            }

            Image panelImage = panelObj.GetComponent<Image>();
            if (player.defaultDescriptionBgSprite != null)
            {
                panelImage.sprite = player.defaultDescriptionBgSprite;
                panelImage.type = Image.Type.Sliced;
            }
            else
            {
                panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.88f);
            }
        }
        else
        {
            panelObj = panelTransform.gameObject;
        }

        // Tìm DescText trong Canvas hoặc trong Panel
        Transform descTextTr = canvas.transform.Find("DescText");
        if (descTextTr == null && panelObj != null)
        {
            descTextTr = panelObj.transform.Find("DescText");
        }

        if (descTextTr != null)
        {
            // Chuyển DescText vào làm con của DescriptionPanel
            descTextTr.SetParent(panelObj.transform, false);

            RectTransform textRect = descTextTr.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(60f, 35f);   // Left = 60, Bottom = 35
            textRect.offsetMax = new Vector2(-60f, -35f); // Right = 60, Top = 35

            TextMeshProUGUI tmp = descTextTr.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.enableWordWrapping = true;
            }
        }

        // Tìm hoặc tạo SpeakerNameText (Ô hiển thị tên ở nhãn nhỏ màu hồng góc trên bên trái)
        Transform nameTextTr = panelObj.transform.Find("SpeakerNameText");
        if (nameTextTr == null)
        {
            GameObject nameObj = new GameObject("SpeakerNameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(panelObj.transform, false);
            nameTextTr = nameObj.transform;

            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 1f);
            nameRect.anchorMax = new Vector2(0f, 1f);
            nameRect.pivot = new Vector2(0f, 0.5f);
            nameRect.anchoredPosition = new Vector2(70f, -8f);
            nameRect.sizeDelta = new Vector2(250f, 40f);

            TextMeshProUGUI nameTmp = nameObj.GetComponent<TextMeshProUGUI>();
            nameTmp.text = "Tên Nhân Vật";
            nameTmp.fontSize = 20;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/Baloo2-Bold SDF.asset");
            if (font != null) nameTmp.font = font;
        }

        // Gán tham chiếu vào ScenarioPlayer
        Undo.RecordObject(player, "Setup Description Panel");
        player.descriptionPanel = panelObj;
        player.descriptionBgImage = panelObj.GetComponent<Image>();
        if (descTextTr != null)
        {
            player.descriptionTextUI = descTextTr.GetComponent<TextMeshProUGUI>();
        }
        if (nameTextTr != null)
        {
            player.speakerNameTextUI = nameTextTr.GetComponent<TextMeshProUGUI>();
        }

        if (player.statImpactPopupPrefab == null)
        {
            player.statImpactPopupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VisualNovelSystem/Prefabs/StatImpactPopupPrefab.prefab");
        }

        if (player.immunityIcon == null)
            player.immunityIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Armour/Shield.png");
        if (player.energyIcon == null)
            player.energyIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Bolt/Bolt - Yellow.png");
        if (player.toxicityIcon == null)
            player.toxicityIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Skull/Skull.png");
        if (player.hydrationIcon == null)
            player.hydrationIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Potion/Potion - Blue.png");
        if (player.recoveryIcon == null)
            player.recoveryIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GUIPackCartoon/Demo/Sprites/Icons/Icons Colored/Heart - Health/Heart - Red.png");

        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        Debug.Log("<color=green>[ScenarioPlayer]</color> Đã tự động tạo và gán DescriptionPanel & SpeakerNameText thành công!");
    }
}
