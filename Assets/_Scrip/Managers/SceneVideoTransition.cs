using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Persistent Singleton Manager điều khiển Video Chuyển Cảnh (Video Out & Video In) mượt mà 100% qua tất cả các Scene.
/// Đã tối ưu 100% cho nền tảng WebGL thông qua StreamingAssets URL & Autoplay Unblock.
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
    public float inDuration = 3.25f;  // Số giây chạy hiệu ứng khi VÀO scene
    public float outDuration = 3.46f; // Số giây chạy hiệu ứng khi RA scene
    public float inFadeOutDuration = 1.0f; // Số giây mờ dần ở cuối video In (Mặc định 1s)

    [Header("4. Cấu hình Safe Timeout")]
    [Tooltip("Thời gian nạp video tối đa (giây). Giữ alpha = 0 trong thời gian này để chống cháy trắng.")]
    public float prepareTimeout = 5.0f;

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

        SetupObject(inChuyenCanh, inClip, "In_ChuyenCanh.mp4");
        SetupObject(outChuyenCanh, outClip, "Out_ChuyenCanh.mp4");
    }

    public void UpdateClips()
    {
        if (inChuyenCanh != null)
        {
            VideoPlayer vp = inChuyenCanh.GetComponent<VideoPlayer>();
            if (vp != null) ConfigureVideoSource(vp, inClip, "In_ChuyenCanh.mp4");
        }
        if (outChuyenCanh != null)
        {
            VideoPlayer vp = outChuyenCanh.GetComponent<VideoPlayer>();
            if (vp != null) ConfigureVideoSource(vp, outClip, "Out_ChuyenCanh.mp4");
        }
    }

    private void ConfigureVideoSource(VideoPlayer vp, VideoClip clip, string fileName)
    {
        if (vp == null) return;

        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.audioOutputMode = VideoAudioOutputMode.None; // Tắt audio để không bị browser WebGL autoplay block

        #if UNITY_WEBGL && !UNITY_EDITOR
        string videoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        vp.source = VideoSource.Url;
        vp.url = videoUrl;
        #else
        if (clip != null)
        {
            vp.source = VideoSource.VideoClip;
            vp.clip = clip;
        }
        else
        {
            string videoUrl = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
            vp.source = VideoSource.Url;
            vp.url = videoUrl;
        }
        #endif
    }

    private void SetupObject(GameObject obj, VideoClip clip, string fileName)
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
        cg.alpha = 0f; // Mặc định ẩn 100%
        cg.blocksRaycasts = false; // Mặc định không chặn button

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

        ConfigureVideoSource(vp, clip, fileName);
    }

    /// <summary>
    /// Phát Video In khi mở màn hình
    /// </summary>
    public void PlayInVideo()
    {
        if (inChuyenCanh != null)
        {
            inChuyenCanh.SetActive(true);
            if (outChuyenCanh != null)
            {
                CanvasGroup outCg = outChuyenCanh.GetComponent<CanvasGroup>();
                if (outCg != null) outCg.blocksRaycasts = false;
                outChuyenCanh.SetActive(false);
            }

            StartCoroutine(PlayInRoutine());
        }
    }

    private IEnumerator PlayInRoutine()
    {
        if (inChuyenCanh == null) yield break;

        CanvasGroup cg = inChuyenCanh.GetComponent<CanvasGroup>();
        if (cg == null) cg = inChuyenCanh.AddComponent<CanvasGroup>();
        cg.alpha = 0f; // 🚫 GIỮ ẨN 100% LÚC NẠP DỮ LIỆU ĐỂ CHỐNG CHÁY TRẮNG
        cg.blocksRaycasts = true;

        VideoPlayer vp = inChuyenCanh.GetComponent<VideoPlayer>();
        if (vp != null) ConfigureVideoSource(vp, inClip, "In_ChuyenCanh.mp4");

        float duration = inDuration > 0 ? inDuration : 3.25f;

        if (vp != null)
        {
            if (vp.clip != null && vp.clip.length > 0) duration = (float)vp.clip.length;
            else if (vp.length > 0) duration = (float)vp.length;
            else if (inDuration > 0) duration = inDuration;

            vp.Prepare();
            float timer = 0f;
            while (!vp.isPrepared && timer < prepareTimeout)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Đã nạp xong Frame 0 ➔ Bắt đầu phát Video rồi mới mở Alpha = 1
            vp.Play();
            yield return null;
            cg.alpha = 1f;
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
            float fadeStart = Mathf.Max(0f, duration - fadeOutDuration);
            if (elapsed >= fadeStart)
            {
                cg.alpha = Mathf.Lerp(1f, 0f, (elapsed - fadeStart) / fadeOutDuration);
            }
            else
            {
                cg.alpha = 1f;
            }
            yield return null;
        }

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        inChuyenCanh.SetActive(false);
    }

    /// <summary>
    /// Gọi chuyển Scene từ bất kỳ đâu
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;

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
        Time.timeScale = 1f;
        StopAllCoroutines();
        isTransitioning = true;

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

        // 1. Prepare Video Out trước khi chuyển Scene (Giữ alpha = 0f chống nháy trắng)
        if (outChuyenCanh != null)
        {
            CanvasGroup cg = outChuyenCanh.GetComponent<CanvasGroup>();
            if (cg == null) cg = outChuyenCanh.AddComponent<CanvasGroup>();
            cg.alpha = 0f; // 🚫 GIỮ ẨN 100% LÚC PREPARE ➔ CHỐNG CHÁY TRẮNG!
            cg.blocksRaycasts = true;

            outChuyenCanh.SetActive(true);

            RawImage raw = outChuyenCanh.GetComponent<RawImage>();
            if (raw != null) raw.color = Color.white;

            VideoPlayer vp = outChuyenCanh.GetComponent<VideoPlayer>();
            if (vp != null) ConfigureVideoSource(vp, outClip, "Out_ChuyenCanh.mp4");

            float duration = outDuration > 0 ? outDuration : 3.46f;

            if (vp != null)
            {
                if (outDuration > 0) duration = outDuration;
                else if (vp.clip != null && vp.clip.length > 0) duration = (float)vp.clip.length;

                vp.Prepare();
                float timer = 0f;
                while (!vp.isPrepared && timer < prepareTimeout)
                {
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }

                // Đã nạp xong Frame 0 ➔ Phát video & hiện alpha = 1
                vp.Play();
                yield return null;
                cg.alpha = 1f;
            }
            else
            {
                // Nếu không có video clip -> Fade in từ 0 -> 1 mượt mà
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

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            cg.alpha = 1f;
        }
        else
        {
            yield return new WaitForSecondsRealtime(outDuration);
        }

        // 2. Load Scene B ngầm trong background (Async)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Dọn dẹp listener/pools cũ
        if (MultiEnemyPool.Instance != null) Destroy(MultiEnemyPool.Instance.gameObject);
        if (LifeManager.Instance != null) LifeManager.Instance.ClearListeners();

        // 3. Kích hoạt đổi sang Scene B ngay lập tức
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield return null;
        yield return new WaitForEndOfFrame();

        // 4. PHÁT VIDEO IN Ở SCENE B (DOUBLE BUFFERING CHỐNG CHÁY TRẮNG)
        if (inChuyenCanh != null)
        {
            CanvasGroup cgIn = inChuyenCanh.GetComponent<CanvasGroup>();
            if (cgIn == null) cgIn = inChuyenCanh.AddComponent<CanvasGroup>();
            cgIn.alpha = 0f; // 🚫 GIỮ ẨN 100% LÚC PREPARE VIDEO IN!
            cgIn.blocksRaycasts = true;

            inChuyenCanh.SetActive(true);
            inChuyenCanh.transform.SetAsLastSibling();

            RawImage rawIn = inChuyenCanh.GetComponent<RawImage>();
            if (rawIn != null) rawIn.color = Color.white;

            VideoPlayer vpIn = inChuyenCanh.GetComponent<VideoPlayer>();
            if (vpIn != null) ConfigureVideoSource(vpIn, inClip, "In_ChuyenCanh.mp4");

            float durationIn = inDuration > 0 ? inDuration : 3.25f;

            if (vpIn != null)
            {
                if (vpIn.clip != null && vpIn.clip.length > 0) durationIn = (float)vpIn.clip.length;
                else if (vpIn.length > 0) durationIn = (float)vpIn.length;
                else if (inDuration > 0) durationIn = inDuration;

                vpIn.Prepare();
                float timerIn = 0f;
                while (!vpIn.isPrepared && timerIn < prepareTimeout)
                {
                    timerIn += Time.unscaledDeltaTime;
                    yield return null;
                }

                vpIn.Play();
                yield return null;
                cgIn.alpha = 1f;
            }
            else
            {
                cgIn.alpha = 1f;
            }

            // ĐẾN ĐÂY VIDEO IN ĐÃ SẴN SÀNG 100% FRAME 0 ➔ MỚI TẮT VIDEO OUT (Zero-Flash!)
            if (outChuyenCanh != null)
            {
                CanvasGroup outCg = outChuyenCanh.GetComponent<CanvasGroup>();
                if (outCg != null) outCg.blocksRaycasts = false;
                outChuyenCanh.SetActive(false);
            }

            float elapsedIn = 0f;
            float fadeOutDurationIn = inFadeOutDuration > 0 ? inFadeOutDuration : 1.0f;

            while (elapsedIn < durationIn)
            {
                elapsedIn += Time.unscaledDeltaTime;
                float fadeStartIn = Mathf.Max(0f, durationIn - fadeOutDurationIn);
                if (elapsedIn >= fadeStartIn)
                {
                    cgIn.alpha = Mathf.Lerp(1f, 0f, (elapsedIn - fadeStartIn) / fadeOutDurationIn);
                }
                else
                {
                    cgIn.alpha = 1f;
                }
                yield return null;
            }

            cgIn.alpha = 0f;
            cgIn.blocksRaycasts = false;
            inChuyenCanh.SetActive(false);
        }
        else
        {
            if (outChuyenCanh != null)
            {
                CanvasGroup outCg = outChuyenCanh.GetComponent<CanvasGroup>();
                if (outCg != null) outCg.blocksRaycasts = false;
                outChuyenCanh.SetActive(false);
            }
        }

        isTransitioning = false;
    }
}
