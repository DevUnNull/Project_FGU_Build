using UnityEngine;
using System.Collections;

/// <summary>
/// Script gán cho tướng Hồng Cầu (CellHongCau).
/// Cứ mỗi 5s sau khi ra sàn đấu, Hồng Cầu sẽ phát animation Spawn và nảy ra 1 đồng tiền rơi ngẫu nhiên xung quanh.
/// khi click vào đồng tiền đó, nó biến mất mượt và kích hoạt TrashSellAnimation bắn tiền về pointNhanTien.
/// </summary>
public class HongCauCoinSpawner : MonoBehaviour
{
    [Header("Coin Spawn Settings")]
    [Tooltip("Thời gian giữa các lần sinh tiền (Mặc định 5s)")]
    public float spawnInterval = 5f;

    [Tooltip("Giá trị tiền thu được khi nhặt đồng xu này")]
    public int goldPerCoin = 5;

    [Tooltip("Kích thước đồng coin rơi trên sàn")]
    public Vector3 droppedCoinScale = new Vector3(0.35f, 0.35f, 0.35f);

    [Tooltip("Bán kính rơi ngẫu nhiên tối thiểu/tối đa xung quanh con Hồng Cầu")]
    public float dropRadiusMin = 0.7f;
    public float dropRadiusMax = 1.4f;

    [Tooltip("Bán kính vùng chạm (Collider) để bấm nhặt tiền dễ dàng không bị trượt")]
    public float clickColliderRadius = 3.5f;

    [Tooltip("Sprite đồng coin rơi (Mặc định tự nạp từ Resources/HongCauDroppedCoin)")]
    public Sprite customCoinSprite;

    private float timer = 0f;
    private DragAndDrop dragComponent;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        dragComponent = GetComponent<DragAndDrop>();
        if (customCoinSprite == null)
        {
            customCoinSprite = Resources.Load<Sprite>("HongCauDroppedCoin");
        }
    }

    private void Update()
    {
        // 1. Chỉ sinh tiền khi đã mua và được đặt trên sàn đấu
        if (dragComponent != null && !dragComponent.isBuy)
        {
            return;
        }

        // 2. Chỉ sinh tiền khi người chơi đã ấn nút GỌI WAVE (StartWave)
        if (WaveManager.Instance != null && !WaveManager.Instance.IsWaveStarted)
        {
            return;
        }

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TriggerSpawnCoinAnimation();
        }
    }

    /// <summary>
    /// Kích hoạt animation Spawn. 
    /// Mốc 0.35s của animation Spawn có Animation Event tự động gọi SpawnRandomCoin().
    /// </summary>
    public void TriggerSpawnCoinAnimation()
    {
        if (animator == null) animator = GetComponent<Animator>();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.Play("Spawn", 0, 0f);
        }
        else
        {
            // Fallback nếu không có animator hoặc controller
            SpawnRandomCoin();
        }
    }

    /// <summary>
    /// Sinh 1 đồng coin nảy ra ngẫu nhiên từ người con Hồng Cầu (Được gọi bởi Animation Event)
    /// </summary>
    public void SpawnRandomCoin()
    {
        Vector3 startPos = transform.position;
        startPos.z = -5f; // Ưu tiên #1: Nằm sát phía trước Camera để nhận Raycast bấm chuột trước tiên

        // Tính vị trí rơi ngẫu nhiên xung quanh Hồng Cầu (Góc & Bán kính ngẫu nhiên)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomRadius = Random.Range(dropRadiusMin, dropRadiusMax);
        Vector3 offset = new Vector3(Mathf.Cos(randomAngle) * randomRadius, Mathf.Sin(randomAngle) * randomRadius, 0f);
        Vector3 targetGroundPos = startPos + offset;
        targetGroundPos.z = -5f;

        // Tạo GameObject Coin
        GameObject coinObj = new GameObject("HongCau_DroppedCoin");
        coinObj.transform.position = startPos;
        coinObj.transform.localScale = Vector3.zero;

        // Gán SpriteRenderer
        SpriteRenderer sr = coinObj.AddComponent<SpriteRenderer>();
        if (customCoinSprite != null)
        {
            sr.sprite = customCoinSprite;
        }
        else
        {
            // Fallback load sprite
            sr.sprite = Resources.Load<Sprite>("HongCauDroppedCoin");
        }
        sr.sortingOrder = 5000;

        // Gán CircleCollider2D rộng rãi để dễ bấm không bị trượt
        CircleCollider2D collider = coinObj.AddComponent<CircleCollider2D>();
        collider.radius = clickColliderRadius;
        collider.isTrigger = true;

        // Gán script quản lý tương tác Click
        HongCauDroppedCoinItem coinItem = coinObj.AddComponent<HongCauDroppedCoinItem>();
        coinItem.goldValue = goldPerCoin;

        // Chạy Coroutine nảy coin ra khỏi người mượt mà (Parabolic Jump Arc)
        float arcHeight = Random.Range(0.8f, 1.3f);
        StartCoroutine(PopOutAndDropRoutine(coinObj.transform, startPos, targetGroundPos, arcHeight, coinItem));
    }

    /// <summary>
    /// Coroutine tạo hiệu ứng đồng coin nảy ra khỏi người Hồng Cầu và rớt xuống đất mượt mà
    /// </summary>
    private IEnumerator PopOutAndDropRoutine(Transform coinTransform, Vector3 startPos, Vector3 endPos, float arcHeight, HongCauDroppedCoinItem coinItem)
    {
        float duration = 0.42f;
        float elapsed = 0f;

        Vector3 targetScale = droppedCoinScale;

        while (elapsed < duration)
        {
            if (coinTransform == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Nảy theo đường cong Parabol
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;
            currentPos.y += heightOffset;

            coinTransform.position = currentPos;

            // Co giãn scale khi nảy ra
            coinTransform.localScale = targetScale * Mathf.Clamp01(t * 1.3f);

            // Xoay nhẹ đồng xu khi nảy
            coinTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2f) * 20f);

            yield return null;
        }

        if (coinTransform == null) yield break;

        coinTransform.position = endPos;
        coinTransform.localRotation = Quaternion.identity;

        // Hiệu ứng nảy đàn hồi (Squash & Stretch Bounce) khi chạm đất
        float bounceTime = 0.15f;
        elapsed = 0f;
        while (elapsed < bounceTime)
        {
            if (coinTransform == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / bounceTime;
            float bounceScale = 1f + Mathf.Sin(t * Mathf.PI) * 0.25f;
            coinTransform.localScale = new Vector3(targetScale.x * bounceScale, targetScale.y * (2f - bounceScale), targetScale.z);
            yield return null;
        }

        if (coinTransform == null) yield break;

        coinTransform.localScale = targetScale;

        // Đã rơi xuống đất xong -> Bắt đầu hiệu ứng dập dềnh nhấp nháy chờ player click
        if (coinItem != null)
        {
            coinItem.StartIdleAnimation(targetScale);
        }
    }
}
