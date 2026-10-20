using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Hiệu ứng chuyển scene bằng cách trượt các khối UI ra khỏi màn hình rồi trượt vào ở scene mới.
/// - Chỉ trượt các con trực tiếp của Canvas gốc; ảnh nền / panel phủ toàn màn hình đứng yên.
/// - Lớp đen mờ ngắn che đúng lúc đổi scene để vật thể thế giới (ô bàn cờ, tế bào...) không bị nháy.
/// - Trượt vào sau khi frame đầu của scene mới chạy xong (các script đã ghi nhớ vị trí trong Awake/Start).
/// Dùng qua SceneVideoTransition.LoadScene (video chỉ dành cho MainMenuScene ↔ MainMap).
/// </summary>
public class SceneSlideTransition : MonoBehaviour
{
    private const float SlideOutDuration = 0.4f;
    private const float SlideInDuration = 0.55f;
    private const float FadeDuration = 0.18f;
    private const float StaggerPerItem = 0.025f;

    private static SceneSlideTransition instance;
    private CanvasGroup overlayGroup;
    private bool isTransitioning;

    private struct SlideItem
    {
        public RectTransform rect;
        public Vector2 home;
        public Vector2 offset;
        public float delay;
    }

    public static void LoadScene(string sceneName)
    {
        if (instance == null)
        {
            var go = new GameObject("SceneSlideTransition");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SceneSlideTransition>();
            instance.BuildOverlay();
        }
        if (instance.isTransitioning) return;
        instance.StartCoroutine(instance.TransitionRoutine(sceneName));
    }

    private void BuildOverlay()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        gameObject.AddComponent<GraphicRaycaster>();

        var overlay = new GameObject("Fade", typeof(RectTransform));
        var rt = (RectTransform)overlay.transform;
        rt.SetParent(transform, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        overlay.AddComponent<Image>().color = Color.black;

        overlayGroup = overlay.AddComponent<CanvasGroup>();
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"❌ Scene '{sceneName}' chưa được thêm vào Build Settings!");
            yield break;
        }

        isTransitioning = true;
        Time.timeScale = 1f;
        overlayGroup.blocksRaycasts = true; // chặn bấm lung tung trong lúc chuyển

        // 1. Trượt UI của scene hiện tại ra ngoài, cuối cùng phủ đen ngắn
        List<SlideItem> outItems = CollectItems(SceneManager.GetActiveScene());
        float outTotal = SlideOutDuration + StaggerPerItem * outItems.Count;
        for (float t = 0f; t < outTotal; t += Time.unscaledDeltaTime)
        {
            foreach (var item in outItems)
            {
                if (item.rect == null) continue;
                float k = Mathf.Clamp01((t - item.delay) / SlideOutDuration);
                item.rect.anchoredPosition = item.home + item.offset * (k * k * k); // tăng tốc dần
            }
            overlayGroup.alpha = Mathf.Clamp01((t - (outTotal - FadeDuration)) / FadeDuration);
            yield return null;
        }
        overlayGroup.alpha = 1f;

        // 2. Nạp scene mới (dọn pool/listener cũ giống SceneVideoTransition)
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        load.allowSceneActivation = false;
        while (load.progress < 0.9f) yield return null;
        if (MultiEnemyPool.Instance != null) Destroy(MultiEnemyPool.Instance.gameObject);
        if (LifeManager.Instance != null) LifeManager.Instance.ClearListeners();
        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;

        // Chờ frame đầu (mọi Awake/Start đã chạy, layout đã tính) rồi mới dời UI ra ngoài
        yield return null;
        yield return new WaitForEndOfFrame();

        // 3. Trượt UI của scene mới vào, đồng thời bỏ lớp đen
        List<SlideItem> inItems = CollectItems(SceneManager.GetActiveScene());
        foreach (var item in inItems) item.rect.anchoredPosition = item.home + item.offset;

        float inTotal = SlideInDuration + StaggerPerItem * inItems.Count;
        for (float t = 0f; t < inTotal; t += Time.unscaledDeltaTime)
        {
            foreach (var item in inItems)
            {
                if (item.rect == null) continue;
                float k = Mathf.Clamp01((t - item.delay) / SlideInDuration);
                item.rect.anchoredPosition = item.home + item.offset * (1f - EaseOutBack(k));
            }
            overlayGroup.alpha = 1f - Mathf.Clamp01(t / FadeDuration);
            yield return null;
        }

        foreach (var item in inItems)
        {
            if (item.rect != null) item.rect.anchoredPosition = item.home;
        }
        overlayGroup.alpha = 0f;
        overlayGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private static float EaseOutBack(float x)
    {
        const float c1 = 1.2f;
        const float c3 = c1 + 1f;
        float p = x - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }

    /// <summary>
    /// Lấy các khối UI cần trượt: con trực tiếp, đang bật của các Canvas gốc trong scene.
    /// Bỏ qua khối phủ (gần) toàn màn hình như ảnh nền, panel Pause/Win/Lose.
    /// </summary>
    private List<SlideItem> CollectItems(Scene scene)
    {
        var items = new List<SlideItem>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null || !canvas.isActiveAndEnabled) continue;

            RectTransform canvasRect = (RectTransform)canvas.transform;
            Vector2 canvasSize = canvasRect.rect.size;
            if (canvasSize.x <= 0f || canvasSize.y <= 0f) continue;

            foreach (Transform child in root.transform)
            {
                RectTransform rt = child as RectTransform;
                if (rt == null || !child.gameObject.activeInHierarchy) continue;
                if (IsFullScreen(rt, canvasSize)) continue;

                // Vị trí tâm khối so với tâm canvas -> trượt ra phía mép gần nhất
                Vector2 center = canvasRect.InverseTransformPoint(rt.TransformPoint(rt.rect.center));
                Vector2 normalized = new Vector2(center.x / canvasSize.x, center.y / canvasSize.y);
                Vector2 dir;
                if (normalized.magnitude < 0.05f) dir = Vector2.down;
                else if (Mathf.Abs(normalized.x) >= Mathf.Abs(normalized.y)) dir = new Vector2(Mathf.Sign(normalized.x), 0f);
                else dir = new Vector2(0f, Mathf.Sign(normalized.y));

                Vector2 size = rt.rect.size;
                float distance = dir.x != 0f ? canvasSize.x * 0.6f + size.x : canvasSize.y * 0.6f + size.y;

                items.Add(new SlideItem
                {
                    rect = rt,
                    home = rt.anchoredPosition,
                    offset = dir * distance,
                    delay = StaggerPerItem * items.Count,
                });
            }
        }
        return items;
    }

    private static bool IsFullScreen(RectTransform rt, Vector2 canvasSize)
    {
        Vector2 size = rt.rect.size;
        return size.x >= canvasSize.x * 0.9f && size.y >= canvasSize.y * 0.9f;
    }
}
