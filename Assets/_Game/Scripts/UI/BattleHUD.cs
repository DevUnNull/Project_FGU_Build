using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD bổ sung cho trận phòng thủ. Đặt sẵn trong Scene (object "BattleHUD", tạo bằng Tools → Battle HUD)
/// để chỉnh tay vị trí các nút; scene nào không có thì GridManager tự gắn và mọi thứ được tạo bằng code:
/// - Nút tốc độ x1/x2 (cạnh nút Setting, góc trên phải), nhớ lựa chọn giữa các trận
/// - Nút "?" mở bảng Cách chơi (HowToPlayUI, tạm dừng trận); bảng tự hiện 1 lần ở trận đầu tiên khi bấm Bắt đầu
/// - Gợi ý ghép tế bào & bấm Roll ngay trên bàn cờ (MergeHintUI)
/// - Nút "Gọi wave sớm" gọi wave tiếp theo cho mọi làn đang đếm ngược
/// - Thanh vật phẩm lối sống (ItemBarUI)
/// - Thông báo ngắn (Toast) ở giữa phía trên: mẹo, cảnh báo, giải thích hậu quả lối sống
/// - Viền đỏ + rung màn hình khi mầm bệnh lọt tới Trái Tim, viền đỏ nhấp nháy khi máu thấp
/// </summary>
public class BattleHUD : MonoBehaviour
{
    private static BattleHUD instance;

    // Vị trí mặc định khi phải tạo bằng code (toạ độ tính từ tâm canvas 1920x1080)
    public static readonly Vector2 DefaultSpeedPosition = new Vector2(428.908569f, 466.051453f);
    public static readonly Vector2 DefaultHelpPosition = new Vector2(-279f, 0f);
    public static readonly Vector2 DefaultCallAllPosition = new Vector2(151f, 469f);
    public static readonly Vector2 DefaultToastPosition = new Vector2(-110f, -75f); // neo mép trên giữa
    public static readonly Vector2 DefaultToastSize = new Vector2(900f, 120f);
    public static readonly Vector2 DefaultItemBarPosition = new Vector2(-640f, -462f);

    [Header("Đặt sẵn trong Scene để chỉnh tay (để trống = tự tạo bằng code)")]
    [Tooltip("Nút x1/x2. Khung viên thuốc và sự kiện bấm được gắn lúc chạy")]
    [SerializeField] private Button speedButtonRef;
    [Tooltip("Nút \"?\" mở bảng Cách chơi")]
    [SerializeField] private Button helpButtonRef;
    [Tooltip("Nút \"Gọi wave sớm\" (tự ẩn khi chưa có làn nào gọi được)")]
    [SerializeField] private Button callAllButtonRef;
    [Tooltip("Vùng hiện thông báo (Toast): băng cảnh báo lấy đúng vị trí & kích thước vùng này")]
    [SerializeField] private RectTransform toastArea;
    [Tooltip("Vị trí ô vật phẩm ĐẦU TIÊN; các ô sau xếp sang phải")]
    [SerializeField] private RectTransform itemBarArea;

    private Canvas canvas;
    private TextMeshProUGUI speedLabel;
    private GameObject speedButton;
    private GameObject helpButton;
    private GameObject callAllButton;

    private CanvasGroup toastGroup;
    private TextMeshProUGUI toastText;
    private readonly Queue<(string message, Color color)> toastQueue = new Queue<(string, Color)>();
    private bool toastRunning;
    private readonly HashSet<string> shownOnce = new HashSet<string>();

    private Image vignette;
    private float vignetteFlash;
    private bool lowLife;

    private int lastLife = -1;
    private bool lifeHooked;
    private bool waveHooked;

    public static void Ensure(GameObject host)
    {
        if (instance == null && FindFirstObjectByType<BattleHUD>() == null) host.AddComponent<BattleHUD>();
    }

    /// <summary>Hiện thông báo ngắn. Nếu onceKey khác null, thông báo chỉ hiện 1 lần mỗi trận.</summary>
    public static void Toast(string message, Color? color = null, string onceKey = null)
    {
        if (instance == null) return;
        if (onceKey != null && !instance.shownOnce.Add(onceKey)) return;
        instance.toastQueue.Enqueue((message, color ?? RuntimeUI.TextDark));
        if (!instance.toastRunning) instance.StartCoroutine(instance.ToastRoutine());
    }

    // Màu CHỮ của thông báo (nền là băng cảnh báo CanhBao màu kem)
    public static readonly Color WarningColor = new Color(0.72f, 0.12f, 0.08f, 1f);
    public static readonly Color TipColor = new Color(0.1f, 0.45f, 0.2f, 1f);

    private void Awake()
    {
        instance = this;
        BuildUI();
        GameSpeed.LoadSaved();
        speedLabel.text = GameSpeed.Current > 1.5f ? "x2" : "x1";
        gameObject.AddComponent<ItemBarUI>().Build(canvas.transform, itemBarArea);
        gameObject.AddComponent<MergeHintUI>().Build(canvas);
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
        GameSpeed.ExitBattle();
        if (lifeHooked && LifeManager.Instance != null) LifeManager.Instance.OnLifeChanged -= HandleLifeChanged;
        if (waveHooked && WaveManager.Instance != null) WaveManager.Instance.OnWaveStart -= HandleWaveStart;
    }

    private void BuildUI()
    {
        // HUD đặt sẵn trong Scene đã có Canvas; tạo bằng code thì thêm Canvas con
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = RuntimeUI.CreateOverlayCanvas("BattleHUD", 300);
            canvas.transform.SetParent(transform, false);
        }

        // Viền đỏ (dưới cùng, không chặn click)
        RectTransform vignetteRt = RuntimeUI.Stretch(canvas.transform, "DangerVignette");
        vignetteRt.SetAsFirstSibling();
        vignette = RuntimeUI.AddImage(vignetteRt, new Color(1f, 0f, 0f, 0f), false);
        vignette.sprite = CreateVignetteSprite();

        Button speedBtn = SetupButton(speedButtonRef, "x1", new Vector2(150f, 80f), DefaultSpeedPosition, ToggleSpeed);
        speedLabel = speedBtn.GetComponentInChildren<TextMeshProUGUI>();
        speedButton = speedBtn.gameObject;

        helpButton = SetupButton(helpButtonRef, "?", new Vector2(100f, 80f), DefaultHelpPosition, OpenHelp).gameObject;

        callAllButton = SetupButton(callAllButtonRef, "Gọi wave sớm", new Vector2(260f, 80f), DefaultCallAllPosition, CallAllWaves).gameObject;
        callAllButton.SetActive(false);

        // Toast: băng cảnh báo CanhBao; có vùng toastArea trong Scene thì lấy đúng vị trí & kích thước vùng đó
        RectTransform toastRt;
        RectTransform toastContent;
        if (toastArea != null)
        {
            toastRt = RuntimeUI.CreateWarningBanner(toastArea, "Toast", toastArea.rect.size, out toastContent);
        }
        else
        {
            toastRt = RuntimeUI.CreateWarningBanner(canvas.transform, "Toast", DefaultToastSize, out toastContent);
            toastRt.anchorMin = toastRt.anchorMax = new Vector2(0.5f, 1f);
            toastRt.anchoredPosition = DefaultToastPosition;
        }
        toastText = RuntimeUI.CreateText(toastContent, "", 24f, RuntimeUI.TextDark);
        toastText.enableAutoSizing = true;
        toastText.fontSizeMin = 15f;
        toastText.fontSizeMax = 24f;
        toastGroup = toastRt.gameObject.AddComponent<CanvasGroup>();
        toastGroup.alpha = 0f;
        toastGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Nút đặt sẵn trong Scene: đổi sang khung viên thuốc + gắn sự kiện bấm.
    /// Không có thì tạo mới bằng code tại vị trí mặc định.
    /// </summary>
    private Button SetupButton(Button sceneButton, string label, Vector2 size, Vector2 defaultPosition, System.Action onClick)
    {
        if (sceneButton != null)
        {
            RuntimeUI.ApplyPillStyle(sceneButton);
            sceneButton.onClick.AddListener(() => onClick());
            return sceneButton;
        }

        Button btn = RuntimeUI.CreatePillButton(canvas.transform, label, size, onClick);
        RectTransform rt = (RectTransform)btn.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = defaultPosition;
        return btn;
    }

    private void OpenHelp()
    {
        HowToPlayUI.Show();
    }

    private void CallAllWaves()
    {
        if (WaveManager.Instance != null) WaveManager.Instance.SkipAllReadyWaves();
    }

    private void ToggleSpeed()
    {
        GameSpeed.Set(GameSpeed.Current > 1.5f ? 1f : 2f);
        speedLabel.text = GameSpeed.Current > 1.5f ? "x2" : "x1";
    }

    private void Update()
    {
        HookEvents();

        // Ẩn nút tốc độ khi game đang dừng (Pause, bảng Thắng/Thua) để không đè lên các bảng đó
        bool running = Time.timeScale > 0f;
        if (speedButton.activeSelf != running) speedButton.SetActive(running);

        // Trận đầu tiên: vừa bấm Bắt đầu thì hiện bảng Cách chơi (ghép tế bào, Roll, bán)
        if (running && !HowToPlayUI.HasSeen && WaveManager.Instance != null && WaveManager.Instance.IsWaveStarted)
        {
            HowToPlayUI.Show();
            running = false;
        }

        bool canHelp = running;
        if (helpButton.activeSelf != canHelp) helpButton.SetActive(canHelp);

        bool canCallAll = running && WaveManager.Instance != null && WaveManager.Instance.HasReadyWaveSkip;
        if (callAllButton.activeSelf != canCallAll) callAllButton.SetActive(canCallAll);

        // Viền đỏ: chớp khi mất máu + nhấp nháy nhẹ khi máu thấp (thời gian thực, chạy cả khi x2)
        vignetteFlash = Mathf.Max(0f, vignetteFlash - Time.unscaledDeltaTime * 1.6f);
        float lowLifePulse = lowLife ? 0.18f + 0.12f * Mathf.Sin(Time.unscaledTime * 5f) : 0f;
        Color c = vignette.color;
        c.a = Mathf.Max(vignetteFlash, lowLifePulse);
        vignette.color = c;
    }

    // Các Manager có thể khởi tạo sau HUD -> móc sự kiện khi chúng sẵn sàng
    private void HookEvents()
    {
        if (!lifeHooked && LifeManager.Instance != null)
        {
            LifeManager.Instance.OnLifeChanged += HandleLifeChanged;
            lastLife = LifeManager.Instance.CurrentLife;
            lifeHooked = true;
        }

        if (!waveHooked && WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart += HandleWaveStart;
            waveHooked = true;
        }
    }

    private void HandleLifeChanged(int current, int max)
    {
        if (lastLife >= 0 && current < lastLife)
        {
            vignetteFlash = 0.55f;
            GameSfx.Play(GameSfx.Kind.LifeLost);
            StartCoroutine(CameraShake(0.25f, 0.12f));
            Toast("Mầm bệnh đã lọt tới Trái Tim! Hãy đặt thêm tế bào chặn làn đó.", WarningColor, "first_life_lost");
        }
        lastLife = current;

        bool wasLow = lowLife;
        lowLife = max > 0 && current > 0 && current <= Mathf.CeilToInt(max * 0.3f);
        if (lowLife && !wasLow)
        {
            Toast("Máu Trái Tim đang rất thấp! Cơ thể sắp gục ngã.", WarningColor, "low_life");
        }
    }

    private void HandleWaveStart(WaveConfig wave)
    {
        Toast("Mẹo: Hồng Cầu sinh ATP, nhặt đồng xu rơi ra để có tiền mua thêm tế bào!", TipColor, "tip_hongcau");
    }

    private const float ToastDuration = 2.4f;

    private static bool PlayerTapped()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
    }

    private IEnumerator ToastRoutine()
    {
        toastRunning = true;
        while (toastQueue.Count > 0)
        {
            var (message, color) = toastQueue.Dequeue();
            GameSfx.Play(color == WarningColor ? GameSfx.Kind.Error : GameSfx.Kind.Toast);
            toastText.text = message;
            toastText.color = color;

            for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
            {
                toastGroup.alpha = t / 0.2f;
                yield return null;
            }
            toastGroup.alpha = 1f;

            // Hiện tối đa 2.4s; người chơi nhấp/chạm vào màn hình thì tắt sớm để không che trận đấu
            for (float t = 0f; t < ToastDuration; t += Time.unscaledDeltaTime)
            {
                if (t > 0.15f && PlayerTapped()) break;
                yield return null;
            }

            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                toastGroup.alpha = 1f - t / 0.3f;
                yield return null;
            }
            toastGroup.alpha = 0f;
        }
        toastRunning = false;
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Vector3 origin = cam.transform.localPosition;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float fade = 1f - t / duration;
            cam.transform.localPosition = origin + (Vector3)(Random.insideUnitCircle * magnitude * fade);
            yield return null;
        }
        cam.transform.localPosition = origin;
    }

    /// <summary>Sprite viền: trong suốt ở giữa, đậm dần ra mép màn hình.</summary>
    private static Sprite CreateVignetteSprite()
    {
        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float edge = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, edge));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
