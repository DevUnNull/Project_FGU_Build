using UnityEngine;
using System.Collections;

/// <summary>
/// Script xử lý tương tác khi đồng coin đã rơi xuống đất.
/// Khi nhấp vào: biến mất mượt và gọi TrashSellAnimation phát chuỗi coin bay về pointNhanTien.
/// </summary>
public class HongCauDroppedCoinItem : MonoBehaviour
{
    public int goldValue = 5;

    private bool isCollected = false;
    private Vector3 baseScale;
    private Coroutine idleRoutine;

    public void StartIdleAnimation(Vector3 initialScale)
    {
        baseScale = initialScale;
        if (idleRoutine != null) StopCoroutine(idleRoutine);
        idleRoutine = StartCoroutine(IdleFloatingRoutine());
    }

    /// <summary>
    /// Hiệu ứng dập dềnh nhấp nháy chờ nhặt
    /// </summary>
    private IEnumerator IdleFloatingRoutine()
    {
        Vector3 startPos = transform.position;
        float timer = 0f;

        while (!isCollected)
        {
            timer += Time.deltaTime * 3f;

            // Nhấp nhô lên xuống nhẹ nhàng
            float floatOffset = Mathf.Sin(timer) * 0.08f;
            transform.position = startPos + new Vector3(0f, floatOffset, 0f);

            // Co giãn nhẹ tạo hiệu ứng phát sáng lấp lánh
            float scalePulse = 1f + Mathf.Sin(timer * 2f) * 0.08f;
            transform.localScale = baseScale * scalePulse;

            yield return null;
        }
    }

    private void Update()
    {
        if (isCollected) return;

        // 📱 Chỉ khi NGƯỜI CHƠI ĐANG GIỮ CHUỘT / GIỮ TAY TRÊN MÀN HÌNH (GetMouseButton(0) hoặc Touch)
        // và vuốt qua gần vị trí đồng xu (bán kính 0.85f) thì mới nhặt!
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            if (Camera.main != null)
            {
                Vector3 inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                inputPos.z = 0f;

                Vector3 coinPos = transform.position;
                coinPos.z = 0f;

                if (Vector2.Distance(inputPos, coinPos) <= 0.85f)
                {
                    CollectCoin();
                }
            }
        }
    }

    private void OnMouseDown()
    {
        CollectCoin();
    }

    /// <summary>
    /// Thu thập coin khi người chơi bấm vào
    /// </summary>
    public void CollectCoin()
    {
        if (isCollected) return;
        isCollected = true;

        // Tắt collider ngay tránh click trùng
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (idleRoutine != null) StopCoroutine(idleRoutine);

        StartCoroutine(CollectAnimationRoutine());
    }

    private IEnumerator CollectAnimationRoutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 spawnPos = transform.position;
        float duration = 0.12f;
        float elapsed = 0f;

        // Biến mất mượt mà (co nhỏ dần)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        // Kích hoạt ngay lập tức TrashSellAnimation từ vị trí đồng xu vừa nhấp
        if (TrashSellAnimation.Instance != null)
        {
            TrashSellAnimation.Play(spawnPos, goldValue);
        }
        else if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(goldValue);
        }

        Destroy(gameObject);
    }
}
