using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SetupDualCharacterUI
{
    [MenuItem("Tools/VN/Setup Dual Character UI in Scene")]
    public static void SetupScene()
    {
        var player = Object.FindObjectOfType<ScenarioPlayer>();
        if (player == null)
        {
            Debug.LogError("[SetupDualCharacterUI] Cannot find ScenarioPlayer in current scene!");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(player.gameObject, "Setup Dual Character UI");

        // 1. Identify Character 1 (Charter)
        Transform canvas = null;
        Transform charter1 = null;

        if (player.character1Root != null)
        {
            charter1 = player.character1Root.transform;
            canvas = charter1.parent;
        }
        else
        {
            charter1 = GameObject.Find("Charter")?.transform;
            if (charter1 == null && player.characterPoseImage != null)
            {
                charter1 = player.characterPoseImage.transform.parent;
            }
            if (charter1 != null)
            {
                canvas = charter1.parent;
                player.character1Root = charter1.gameObject;
            }
        }

        if (charter1 == null)
        {
            Debug.LogError("[SetupDualCharacterUI] Cannot find Charter (Character 1) in the Scene!");
            return;
        }

        if (canvas == null) canvas = charter1.parent;

        // 2. Find or Create Character 2 (Charter_Left)
        Transform charter2 = canvas.Find("Charter_Left");
        if (charter2 == null)
        {
            GameObject newObj = Object.Instantiate(charter1.gameObject, canvas);
            newObj.name = "Charter_Left";
            charter2 = newObj.transform;
            Undo.RegisterCreatedObjectUndo(newObj, "Create Charter_Left");

            // Set sibling index right after charter1
            charter2.SetSiblingIndex(charter1.GetSiblingIndex() + 1);

            // Symmetrical position
            RectTransform rt1 = charter1.GetComponent<RectTransform>();
            RectTransform rt2 = charter2.GetComponent<RectTransform>();
            if (rt1 != null && rt2 != null)
            {
                rt2.anchoredPosition = new Vector2(-rt1.anchoredPosition.x, rt1.anchoredPosition.y);
                // Flip X scale so character faces towards center
                Vector3 scale = rt1.localScale;
                scale.x = -Mathf.Abs(scale.x);
                rt2.localScale = scale;
            }
        }

        // Rename children for clarity
        Transform pose2 = charter2.Find("MainCharter") ?? charter2.Find("MainCharter_2");
        if (pose2 != null) pose2.name = "MainCharter_2";

        Transform exp2 = charter2.Find("Expention") ?? charter2.Find("Expention_2");
        if (exp2 != null) exp2.name = "Expention_2";

        Transform hair2 = charter2.Find("Hair") ?? charter2.Find("Hair_2");
        if (hair2 != null) hair2.name = "Hair_2";

        Transform pop2 = charter2.Find("pointPopUp");
        if (pop2 != null) pop2.name = "pointPopUp_2";

        // 3. Link references in ScenarioPlayer
        player.character1Root = charter1.gameObject;
        player.character2Root = charter2.gameObject;

        if (pose2 != null) player.character2PoseImage = pose2.GetComponent<Image>();
        if (exp2 != null) player.character2ExpressionImage = exp2.GetComponent<Image>();
        if (hair2 != null) player.character2HairImage = hair2.GetComponent<Image>();

        // Ensure both roots are initially active
        charter1.gameObject.SetActive(true);
        charter2.gameObject.SetActive(true);

        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);

        Debug.Log("[SetupDualCharacterUI] Successfully set up Dual Character UI in Scene! Character 1: " + charter1.name + ", Character 2: " + charter2.name);
    }
}
