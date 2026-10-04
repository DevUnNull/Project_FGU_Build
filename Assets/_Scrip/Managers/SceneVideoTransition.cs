using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Persistent Singleton Manager điều khiển Video Chuyển Cảnh (Video Out & Video In) mượt mà 100% qua tất cả các Scene.
/// Khắc phục hoàn toàn cả lỗi nháy đen và nháy trắng bằng cơ chế Zero-Flash Pre-Preparation.
/// </summary>
public class SceneVideoTransition : MonoBehaviour
{
    public static SceneVideoTransition Instance { get; private set; }

    [Header("1. Khung hiển thị Video (Kéo 2 GameObject In/Out vào đây)")]
    public GameObject inChuyenCanh;   // GameObject chứa RawImage & VideoPlayer của hiệu ứng VÀO scene
    public GameObject outChuyenCanh;  // GameObject chứa RawImage & VideoPlayer của hiệu ứng RA scene

    [Header("2. Video Clip (Tùy chọn: Kéo file video .mp4 vào đây nếu muốn thay đổi)")]
    public VideoClip inClip;
    public VideoClip outClip;

    [Header("3. Cấu hình thời gian (Số giây chạy hiệu ứng - Chỉnh tùy ý trên Inspector)")]
    public float inDuration = 3.25f;  // Số giây chạy hiệu ứng khi VÀO scene (được đo chính xác từ file In_ChuyenCanh.mp4: 3.25s)
    public float outDuration = 3.46f; // Số giây chạy hiệu ứng khi RA scene (được đo chính xác từ file Out_ChuyenCanh.mp4: 3.46s)
    public float inFadeOutDuration = 1.0f; // Số giây mờ dần ở cuối video In (Mặc định 1s)

    private bool isTransitioning = false;
    private Canvas transitionCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (inClip != null) Instance.inClip = inClip;
            if (outClip != null) Instance.outClip = outClip;
            Instance.UpdateClips();

            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent != null)
        {
            transform.SetParent(null);
        }
        DontDestroyOnLoad(gameObject);

        EnsureCanvas();
        AutoSetup();
    }

    private void Start()
    {
        if (!isTransitioning)
        {
            PlayInVideo();
        }
    }

    private void EnsureCanvas()
    {
        transitionCanvas = GetComponent<Canvas>();
        if (transitionCanvas == null)
        {
            transitionCanvas = gameObject.AddComponent<Canvas>();
        }
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 32767; // Màn che phủ trên cùng

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    public void AutoSetup()
    {
        EnsureCanvas();

        RectTransform managerRt = GetComponent<RectTransform>();
        if (managerRt != null)
        {
            managerRt.anchorMin = Vector2.zero;
            managerRt.anchorMax = Vector2.one;
            managerRt.offsetMin = Vector2.zero;
            managerRt.offsetMax = Vector2.zero;
            managerRt.pivot = new Vector2(0.5f, 0.5f);
        }

        if (inChuyenCanh == null)
        {
            Transform t = transform.Find("In_ChuyenCanh");
            if (t != null) inChuyenCanh = t.gameObject;
        }

        if (outChuyenCanh == null)
        {
            Transform t = transform.Find("Out_ChuyenCanh");
            if (t != null) outChuyenCanh = t.gameObject;
        }

        SetupObject(inChuyenCanh, inClip);
        SetupObject(outChuyenCanh, outClip);
    }

    public void UpdateClips()
    {
        if (inChuyenCanh != null && inClip != null)
        {
            VideoPlayer vp = inChuyenCanh.GetComponent<VideoPlayer>();
            if (vp != null) vp.clip = inClip;
        }
        if (outChuyenCanh != null && outClip != null)
        {
            VideoPlayer vp = outChuyenCanh.GetComponent<VideoPlayer>();
            if (vp != null) vp.clip = outClip;
        }
    }

    private void SetupObject(GameObject obj, VideoClip clip)
    {
        if (obj == null) return;

        if (obj.transform.parent != transform)
        {
            obj.transform.SetParent(transform, false);
        }

        obj.transform.SetAsLastSibling();

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;
        }

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        cg.alpha = 0f; // Mặc định ẩn 100% không nháy màn hình

        RawImage raw = obj.GetComponent<RawImage>();
        if (raw == null) raw = obj.AddComponent<RawImage>();
        raw.uvRect = new Rect(0, 0, 1, 1);
        raw.color = Color.white;

        RectTransform rawRt = raw.rectTransform;
        if (rawRt != null)
        {
            rawRt.anchorMin = Vector2.zero;
            rawRt.anchorMax = Vector2.one;
            rawRt.offsetMin = Vector2.zero;
            rawRt.offsetMax = Vector2.zero;
            rawRt.pivot = new Vector2(0.5f, 0.5f);
            rawRt.sizeDelta = Vector2.zero;
        }

        VideoPlayer vp = obj.GetComponent<VideoPlayer>();
        if (vp == null) vp = obj.AddComponent<VideoPlayer>();

        if (vp.targetTexture == null || vp.targetTexture.width != 1920 || vp.targetTexture.height != 1080)
        {
            RenderTexture renderTex = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
            renderTex.Create();
            vp.targetTexture = renderTex;
        }

        raw.texture = vp.targetTexture;
        if (raw.material != null) raw.material.mainTexture = vp.targetTexture;

        if (clip != null)
        {
            vp.clip = clip;
            vp.playOnAwake = false;
            vp.renderMode = VideoRenderMode.RenderTexture;
        }
        else
        {
            if (vp.clip != null)
            {
                raw.texture = vp.targetTexture;
            }
            else
            {
                raw.texture = null;
            }
        }
    }

    /// <summary>
    /// Phát Video In khi mở màn hình
    /// </summary>
    public void PlayInVideo()
    {
        if (inChuyenCanh != null)
        {
            inChuyenCanh.SetActive(true);
            if (outChuyenCanh != null) outChuyenCanh.SetActive(false);

            StartCoroutine(PlayInRoutine());
        }
    }

    private IEnumerator PlayInRoutine()
    {
        if (inChuyenCanh == null) yield break;

        CanvasGroup cg = inChuyenCanh.GetComponent<CanvasGroup>();
        if (cg == null) cg = inChuyenCanh.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        VideoPlayer vp = inChuyenCanh.GetComponent<VideoPlayer>();
        bool hasVideo = vp != null && (vp.clip != null || !string.IsNullOrEmpty(vp.url));
        float duration = inDuration > 0 ? inDuration : 0.5f;

        if (hasVideo)
        {
            vp.Prepare();
            while (!vp.isPrepared)
            {
                yield return null;
            }

            if (vp.clip != null && vp.clip.length > 0) duration = (float)vp.clip.length;
            else if (vp.length > 0) duration = (float)vp.length;
            else if (inDuration > 0) duration = inDuration;

            cg.alpha = 1f;
            vp.Play();
        }
        else
        {
            cg.alpha = 1f;
        }

        float elapsed = 0f;
        float fadeOutDuration = inFadeOutDuration > 0 ? inFadeOutDuration : 1.0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (!hasVideo)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            }
            else
            {
                float fadeStart = Mathf.Max(0f, duration - fadeOutDuration);
                if (elapsed >= fadeStart)
                {
                    cg.alpha = Mathf.Lerp(1f, 0f, (elapsed - fadeStart) / fadeOutDuration);
                }
                else
                {
                    cg.alpha = 1f;
                }
            }
            yield return null;
        }

        cg.alpha = 0f;
        inChuyenCanh.SetActive(false);
    }

    /// <summary>
    /// Gọi chuyển Scene từ bất kỳ đâu
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        SceneVideoTransition script = Instance;
        if (script == null || script.gameObject == null)
        {
            script = FindFirstObjectByType<SceneVideoTransition>();
        }

        if (script != null)
        {
            script.PlayOutVideoAndLoad(sceneName);
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }

    public void PlayOutVideoAndLoad(string sceneName)
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Time.timeScale = 1f;
        StartCoroutine(TransitionFullRoutine(sceneName));
    }

    private IEnumerator TransitionFullRoutine(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"❌ Scene '{sceneName}' chưa được thêm vào Build Settings!");
            isTransitioning = false;
            yield break;
        }

        // 1. Nạp Scene B ngầm trong background (Async)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 2. Prepare Video Out trước khi hiển thị (Zero-Flash)
        if (outChuyenCanh != null)
        {
            CanvasGroup cg = outChuyenCanh.GetComponent<CanvasGroup>();
            if (cg == null) cg = outChuyenCanh.AddComponent<CanvasGroup>();
            cg.alpha = 0f; // Giữ ẩn 100% trong lúc nạp video

            outChuyenCanh.SetActive(true);

            RawImage raw = outChuyenCanh.GetComponent<RawImage>();
            if (raw != null) raw.color = Color.white;

            VideoPlayer vp = outChuyenCanh.GetComponent<VideoPlayer>();
            float duration = outDuration > 0 ? outDuration : 0.5f;

            if (vp != null && vp.clip != null)
            {
                if (outDuration > 0) duration = outDuration;
                else if (vp.clip.length > 0) duration = (float)vp.clip.length;

                vp.Prepare();
                while (!vp.isPrepared)
                {
                    yield return null;
                }

                // Đã nạp xong frame 0 của video ➔ Hiện màn che và phát video mượt lập tức
                cg.alpha = 1f;
                vp.Play();
            }
            else
            {
                // Nếu không có video, dùng CanvasGroup Fade mượt từ 0 -> 1
                float fadeElapsed = 0f;
                float fadeDur = 0.3f;
                while (fadeElapsed < fadeDur)
                {
                    fadeElapsed += Time.unscaledDeltaTime;
                    cg.alpha = Mathf.Lerp(0f, 1f, fadeElapsed / fadeDur);
                    yield return null;
                }
                cg.alpha = 1f;
            }

            bool isFadeOnly = (vp == null || vp.clip == null);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (isFadeOnly)
                {
                    cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                }
                yield return null;
            }
            cg.alpha = 1f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(outDuration);
        }

        // 3. Đợi cho đến khi Scene B đã được load 100% vào RAM
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Dọn dẹp listener/pools cũ
        if (MultiEnemyPool.Instance != null) Destroy(MultiEnemyPool.Instance.gameObject);
        if (LifeManager.Instance != null) LifeManager.Instance.ClearListeners();

        // 4. Kích hoạt đổi sang Scene B ngay lập tức
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return null;
        yield return new WaitForEndOfFrame();

        // 5. CHUẨN BỊ VIDEO IN TRƯỚC KHI TẮT VIDEO OUT (DOUBLE-BUFFERING OVERLAP)
        if (inChuyenCanh != null)
        {
            CanvasGroup cgIn = inChuyenCanh.GetComponent<CanvasGroup>();
            if (cgIn == null) cgIn = inChuyenCanh.AddComponent<CanvasGroup>();
            cgIn.alpha = 0f; // Ẩn hoàn toàn trong lúc prepare

            inChuyenCanh.SetActive(true);
            inChuyenCanh.transform.SetAsLastSibling();

            RawImage rawIn = inChuyenCanh.GetComponent<RawImage>();
            if (rawIn != null) rawIn.color = Color.white;

            VideoPlayer vpIn = inChuyenCanh.GetComponent<VideoPlayer>();
            bool hasVideoIn = vpIn != null && (vpIn.clip != null || !string.IsNullOrEmpty(vpIn.url));
            float durationIn = inDuration > 0 ? inDuration : 0.5f;

            if (hasVideoIn)
            {
                vpIn.Prepare();
                while (!vpIn.isPrepared)
                {
                    yield return null;
                }

                if (vpIn.clip != null && vpIn.clip.length > 0) durationIn = (float)vpIn.clip.length;
                else if (vpIn.length > 0) durationIn = (float)vpIn.length;
                else if (inDuration > 0) durationIn = inDuration;

                // Video In đã chuẩn bị xong 100% frame 0 ➔ hiện Video In và phát
                cgIn.alpha = 1f;
                vpIn.Play();

                yield return null;
            }
            else
            {
                cgIn.alpha = 1f;
            }

            // LÚC NÀY MỚI TẮT VIDEO OUT (Zero gap! Không bao giờ bị nháy hay khựng!)
            if (outChuyenCanh != null) outChuyenCanh.SetActive(false);

            float elapsedIn = 0f;
            float fadeOutDurationIn = inFadeOutDuration > 0 ? inFadeOutDuration : 1.0f;

            while (elapsedIn < durationIn)
            {
                elapsedIn += Time.unscaledDeltaTime;
                if (!hasVideoIn)
                {
                    cgIn.alpha = Mathf.Lerp(1f, 0f, elapsedIn / durationIn);
                }
                else
                {
                    float fadeStartIn = Mathf.Max(0f, durationIn - fadeOutDurationIn);
                    if (elapsedIn >= fadeStartIn)
                    {
                        cgIn.alpha = Mathf.Lerp(1f, 0f, (elapsedIn - fadeStartIn) / fadeOutDurationIn);
                    }
                    else
                    {
                        cgIn.alpha = 1f;
                    }
                }
                yield return null;
            }

            cgIn.alpha = 0f;
            inChuyenCanh.SetActive(false);
        }
        else
        {
            if (outChuyenCanh != null) outChuyenCanh.SetActive(false);
        }

        isTransitioning = false;
    }
}

