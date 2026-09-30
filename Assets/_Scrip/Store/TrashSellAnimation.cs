using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrashSellAnimation : MonoBehaviour
{
    private static TrashSellAnimation instance;
    public static TrashSellAnimation Instance
    {
        get
        {
            if (instance == null)
            {
                TrashSellAnimation existing = FindFirstObjectByType<TrashSellAnimation>();
                if (existing != null)
                {
                    instance = existing;
                }
                else
                {
                    GameObject obj = new GameObject("[TrashSellAnimation]");
                    instance = obj.AddComponent<TrashSellAnimation>();
                }
            }
            return instance;
        }
    }

    [Header("Target & References (Kéo thả điểm ĐÍCH tại đây)")]
    [Tooltip("Kéo đối tượng Thùng rác vào đây (Mặc định tự tìm 'Destroy')")]
    public Transform trashBinTransform;

    [Tooltip("🎯 VỊ TRÍ ĐÍCH: Kéo thả Object con (hoặc bất kỳ GameObject nào bạn tạo) vào đây để tiền bay về đúng vị trí đó!")]
    public Transform targetDestinationTransform;

    [Tooltip("Sprite đồng tiền D_HongCau")]
    public Sprite coinSprite;

    // Property alias để tương thích mã nguồn cũ
    public Transform buyFrameTransform
    {
        get => targetDestinationTransform;
        set => targetDestinationTransform = value;
    }

    [Header("Animation Settings")]
    [Tooltip("Kích thước tiền (đã giảm thêm 3 lần nữa = 0.033)")]
    public Vector3 coinBaseScale = new Vector3(0.033f, 0.033f, 0.033f);

    [Tooltip("Số tiền xu tối thiểu/tối đa sinh ra khi bán")]
    public int minCoins = 6;
    public int maxCoins = 12;

    [Tooltip("Bán kính nảy ra xung quanh thùng rác")]
    public float scatterRadius = 0.8f;

    [Tooltip("Thời gian nảy ra")]
    public float scatterDuration = 0.18f;

    [Tooltip("Thời gian tiền bay về đích")]
    public float flyDuration = 0.4f;

    [Tooltip("Độ trễ giữa mỗi đồng tiền")]
    public float staggeredDelay = 0.035f;

    private Sprite[] loadedCoinSprites;
    private Coroutine trashShakeRoutine;
    private Coroutine targetPunchRoutine;
    private Vector3? customWorldPositionTarget = null;

    // Lưu trữ scale ban đầu của các khung để không bao giờ bị méo hoặc phóng to dần
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        AutoSetupReferences();
    }

    private void Start()
    {
        AutoSetupReferences();
    }

    /// <summary>
    /// Tự động tìm kiếm các tham số nếu chưa gán trong Inspector
    /// </summary>
    public void AutoSetupReferences()
    {
        // 1. Tìm Thùng rác (Destroy)
        if (trashBinTransform == null)
        {
            GameObject trashObj = GameObject.Find("Destroy");
            if (trashObj != null) trashBinTransform = trashObj.transform;
        }

        if (trashBinTransform != null && !originalScales.ContainsKey(trashBinTransform))
        {
            originalScales[trashBinTransform] = trashBinTransform.localScale;
        }

        // 2. Tìm Vị trí Đích (Mặc định tìm Khung mua nếu chưa gán Object con)
        if (targetDestinationTransform == null)
        {
            GameObject shopObj = GameObject.Find("Khung mua");
            if (shopObj != null) targetDestinationTransform = shopObj.transform;
        }

        if (targetDestinationTransform != null && !originalScales.ContainsKey(targetDestinationTransform))
        {
            originalScales[targetDestinationTransform] = targetDestinationTransform.localScale;
        }

        // 3. Load Sprite D_HongCau từ Resources hoặc AssetDatabase
        if (coinSprite == null)
        {
            loadedCoinSprites = Resources.LoadAll<Sprite>("D_HongCau");
            if (loadedCoinSprites != null && loadedCoinSprites.Length > 0)
            {
                coinSprite = loadedCoinSprites[0];
            }
            else
            {
#if UNITY_EDITOR
                Sprite editorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ImageCallOurMyCell/Cell/D_HongCau.png");
                if (editorSprite != null) coinSprite = editorSprite;
#endif
            }
        }
    }

    /// <summary>
    /// Gán Object con (hoặc bất kỳ Transform nào) làm điểm đích bay về của tiền
    /// </summary>
    public void SetTargetDestination(Transform newTarget)
    {
        targetDestinationTransform = newTarget;
        customWorldPositionTarget = null;
        if (newTarget != null && !originalScales.ContainsKey(newTarget))
        {
            originalScales[newTarget] = newTarget.localScale;
        }
    }

    /// <summary>
    /// Thay đổi vị trí đích bay về thông qua Vector3 World Position
    /// </summary>
    public void SetTargetDestination(Vector3 worldPosition)
    {
        customWorldPositionTarget = worldPosition;
    }

    private Vector3 GetOriginalScale(Transform target)
    {
        if (target == null) return Vector3.one;

        if (!originalScales.TryGetValue(target, out Vector3 scale))
        {
            scale = target.localScale;
            originalScales[target] = scale;
        }
        return scale;
    }

    /// <summary>
    /// Gọi hiệu ứng bán tướng (Có thể truyền customTarget nếu muốn)
    /// </summary>
    public static void Play(Vector3 startPos, int totalGold, Transform customTarget = null)
    {
        Instance.PlaySellAnimation(startPos, totalGold, customTarget);
    }

    public void PlaySellAnimation(Vector3 startPos, int totalGold, Transform customTarget = null)
    {
        AutoSetupReferences();

        if (customTarget != null)
        {
            SetTargetDestination(customTarget);
        }

        // 1. Rung thùng rác
        if (trashBinTransform != null)
        {
            if (trashShakeRoutine != null) StopCoroutine(trashShakeRoutine);
            trashShakeRoutine = StartCoroutine(ShakeTrashBinRoutine(trashBinTransform));
        }

        // 2. Tính toán điểm bắt đầu và điểm kết thúc
        Vector3 spawnCenter = (trashBinTransform != null) ? trashBinTransform.position : startPos;
        spawnCenter.z = 0f;

        Vector3 targetPos = spawnCenter + new Vector3(3f, 3f, 0f);
        if (customWorldPositionTarget.HasValue)
        {
            targetPos = customWorldPositionTarget.Value;
        }
        else if (targetDestinationTransform != null)
        {
            targetPos = targetDestinationTransform.position;
        }
        targetPos.z = 0f;

        // 3. Bắn tiền
        int coinCount = Mathf.Clamp(totalGold, minCoins, maxCoins);
        StartCoroutine(SpawnAndFlyCoinsRoutine(spawnCenter, targetPos, totalGold, coinCount));
    }

    /// <summary>
    /// Coroutine tạo hiệu ứng rung nảy đàn hồi cho thùng rác
    /// </summary>
    private IEnumerator ShakeTrashBinRoutine(Transform trash)
    {
        Vector3 baseScale = GetOriginalScale(trash);
        Quaternion origRot = trash.localRotation;

        float duration = 0.32f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Wobble xoay qua lại
            float angle = Mathf.Sin(t * Mathf.PI * 6f) * (1f - t) * 12f;
            trash.localRotation = origRot * Quaternion.Euler(0, 0, angle);

            // Pop scale squash & stretch dựa trên scale gốc ban đầu chuẩn
            float scaleDamp = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t) * 0.15f;
            trash.localScale = baseScale * (1f + scaleDamp);

            yield return null;
        }

        trash.localRotation = origRot;
        trash.localScale = baseScale;
    }

    /// <summary>
    /// Coroutine sinh tiền, nảy ra và lượn về đích
    /// </summary>
    private IEnumerator SpawnAndFlyCoinsRoutine(Vector3 spawnCenter, Vector3 targetPos, int totalGold, int coinCount)
    {
        int goldPerCoin = totalGold / coinCount;
        int remainderGold = totalGold % coinCount;

        for (int i = 0; i < coinCount; i++)
        {
            int goldToAdd = goldPerCoin + (i == coinCount - 1 ? remainderGold : 0);
            StartCoroutine(SingleCoinRoutine(spawnCenter, targetPos, i, coinCount, goldToAdd));
            if (staggeredDelay > 0)
            {
                yield return new WaitForSeconds(staggeredDelay);
            }
        }
    }

    /// <summary>
    /// Quản lý vòng đời 1 đồng tiền
    /// </summary>
    private IEnumerator SingleCoinRoutine(Vector3 startPos, Vector3 targetPos, int index, int totalCoins, int goldValue)
    {
        // Tạo GameObject tiền
        GameObject coinObj = new GameObject($"Coin_{index}");
        coinObj.transform.position = startPos;
        coinObj.transform.localScale = Vector3.zero;

        SpriteRenderer sr = coinObj.AddComponent<SpriteRenderer>();
        if (coinSprite != null)
        {
            sr.sprite = coinSprite;
        }
        else if (loadedCoinSprites != null && loadedCoinSprites.Length > 0)
        {
            sr.sprite = loadedCoinSprites[Random.Range(0, loadedCoinSprites.Length)];
        }
        sr.sortingOrder = 9999; // Đảm bảo đè lên trên tất cả UI/Map

        // Kích thước tiền (đã giảm thêm 3 lần nữa = 0.033)
        Vector3 baseScale = coinBaseScale;

        // --- PHASE 1: Nảy ngẫu nhiên ra xung quanh ---
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(scatterRadius * 0.5f, scatterRadius * 1.1f);
        Vector3 scatterPos = startPos + (Vector3)(randomDir * randomDist);
        scatterPos.z = 0f;

        float elapsed = 0f;
        while (elapsed < scatterDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scatterDuration);
            float easePop = EaseOutBack(t);

            coinObj.transform.position = Vector3.Lerp(startPos, scatterPos, easePop);
            coinObj.transform.localScale = baseScale * Mathf.Clamp01(t * 1.2f);

            yield return null;
        }

        // --- PHASE 2: Bay theo đường cong Bezier về Đích ---
        Vector3 currentScatterPos = coinObj.transform.position;
        
        // Điểm điều khiển Bezier cong vồng nhẹ
        Vector3 dir = (targetPos - currentScatterPos).normalized;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0f);
        float curveSide = (index % 2 == 0) ? 1f : -1f;
        Vector3 controlPoint = (currentScatterPos + targetPos) * 0.5f + perpendicular * (1.2f * curveSide) + Vector3.up * 0.8f;

        elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyDuration);

            // Gia tốc bay (EaseInCubic) giúp tiền tăng tốc dần chui vào khung đích
            float flyT = EaseInCubic(t);

            // Bézier curve P(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            Vector3 currentPos = Mathf.Pow(1f - flyT, 2f) * currentScatterPos +
                                 2f * (1f - flyT) * flyT * controlPoint +
                                 Mathf.Pow(flyT, 2f) * targetPos;
            currentPos.z = 0f;
            coinObj.transform.position = currentPos;

            // Xoay nhẹ đồng tiền khi bay
            coinObj.transform.localRotation = Quaternion.Euler(0, 0, flyT * 360f * curveSide);

            // Thu nhỏ nhẹ khi gần tới mục tiêu
            if (t > 0.7f)
            {
                float shrinkT = (1f - t) / 0.3f;
                coinObj.transform.localScale = baseScale * shrinkT;
            }

            yield return null;
        }

        // --- PHASE 3: Đến nơi (Impact) ---
        Destroy(coinObj);

        // Nảy nhẹ Đích khi tiền nạp vào
        if (targetDestinationTransform != null)
        {
            TriggerTargetPunch(targetDestinationTransform);
        }

        // Cộng tiền vào GoldManager
        if (GoldManager.Instance != null && goldValue > 0)
        {
            GoldManager.Instance.AddGold(goldValue);
        }
    }

    private void TriggerTargetPunch(Transform target)
    {
        if (targetPunchRoutine != null) StopCoroutine(targetPunchRoutine);
        targetPunchRoutine = StartCoroutine(PunchScaleRoutine(target, 0.05f, 0.08f));
    }

    /// <summary>
    /// Hiệu ứng nảy scale (Punch / Bounce) an toàn cho Khung Đích
    /// </summary>
    private IEnumerator PunchScaleRoutine(Transform target, float punchAmount, float duration)
    {
        Vector3 baseScale = GetOriginalScale(target);
        float halfDuration = duration * 0.5f;

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            target.localScale = baseScale * (1f + punchAmount * Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            target.localScale = baseScale * (1f + punchAmount * (1f - t));
            yield return null;
        }

        target.localScale = baseScale;
    }

    private float EaseOutBack(float x)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private float EaseInCubic(float x)
    {
        return x * x * x;
    }
}
