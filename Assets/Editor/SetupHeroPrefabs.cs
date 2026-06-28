using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SetupHeroPrefabs
{
    [MenuItem("Tools/Setup PvZ Hero and Health UI")]
    public static void Setup() 
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/_Prefabs/Hero" });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            // 1. Thêm Canvas UI máu
            if (instance.GetComponentInChildren<HeroHealthUI>() == null)
            {
                // Tạo Canvas
                GameObject canvasObj = new GameObject("HealthCanvas");
                canvasObj.transform.SetParent(instance.transform, false);
                canvasObj.transform.localPosition = new Vector3(0, 1.5f, 0);
                
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                
                RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(2f, 0.3f);
                canvasRect.localScale = new Vector3(1, 1, 1);

                // Thêm CanvasScaler
                canvasObj.AddComponent<CanvasScaler>();

                // Tạo Slider (Background)
                GameObject sliderObj = new GameObject("HealthSlider");
                sliderObj.transform.SetParent(canvasObj.transform, false);
                Slider slider = sliderObj.AddComponent<Slider>();
                slider.interactable = false;
                slider.transition = Selectable.Transition.None;
                
                RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0, 0);
                sliderRect.anchorMax = new Vector2(1, 1);
                sliderRect.offsetMin = Vector2.zero;
                sliderRect.offsetMax = Vector2.zero;

                // Tạo Background Image
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(sliderObj.transform, false);
                Image bgImage = bgObj.AddComponent<Image>();
                bgImage.color = Color.black;
                RectTransform bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.anchorMin = new Vector2(0, 0);
                bgRect.anchorMax = new Vector2(1, 1);
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;

                // Tạo Fill Area
                GameObject fillAreaObj = new GameObject("Fill Area");
                fillAreaObj.transform.SetParent(sliderObj.transform, false);
                RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
                fillAreaRect.anchorMin = new Vector2(0, 0);
                fillAreaRect.anchorMax = new Vector2(1, 1);
                fillAreaRect.offsetMin = Vector2.zero;
                fillAreaRect.offsetMax = Vector2.zero;

                // Tạo Fill Image
                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(fillAreaObj.transform, false);
                Image fillImage = fillObj.AddComponent<Image>();
                fillImage.color = Color.green;
                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = new Vector2(0, 0);
                fillRect.anchorMax = new Vector2(1, 1);
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;

                // Assign to slider
                slider.fillRect = fillRect;
                slider.value = 1;

                // Add HeroHealthUI script
                HeroHealthUI healthUI = instance.AddComponent<HeroHealthUI>();
                healthUI.healthSlider = slider;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            GameObject.DestroyImmediate(instance);
        }

        // 2. Tạo một PvZ Hero từ Yasuo
        string sourcePath = "Assets/_Prefabs/Hero/Yasuo.prefab";
        string destPath = "Assets/_Prefabs/Hero/PvZHero.prefab";
        
        if (!AssetDatabase.LoadAssetAtPath<GameObject>(destPath))
        {
            AssetDatabase.CopyAsset(sourcePath, destPath);
        }

        GameObject pvzPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(destPath);
        if (pvzPrefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(pvzPrefab);
            
            HeroStateMachine oldState = instance.GetComponent<HeroStateMachine>();
            if (oldState != null && oldState.GetType() == typeof(HeroStateMachine))
            {
                LayerMask enemyLayer = oldState.enemyLayer;
                float detRange = oldState.detectionRange;
                float attRange = oldState.attackRange;
                float attRate = oldState.attackRate;

                GameObject.DestroyImmediate(oldState, true);
                
                LineHeroStateMachine newState = instance.AddComponent<LineHeroStateMachine>();
                newState.enemyLayer = enemyLayer;
                newState.detectionRange = detRange;
                newState.attackRange = attRange;
                newState.attackRate = attRate;
                newState.checkDirection = Vector2.right;
                newState.checkHeight = 1f;

                PrefabUtility.SaveAsPrefabAsset(instance, destPath);
            }
            GameObject.DestroyImmediate(instance);
        }

        Debug.Log("Hero Prefabs Setup Completed!");
    }
}
