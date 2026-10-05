using TMPro;
using UnityEngine;

/// <summary>
/// Component hiển thị popup damage trên enemy và hero
/// </summary>
public class EnemyPopUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject PopUp_Prefab;
    [SerializeField] private GameObject GoldPopUp_Prefab; // prefab riêng cho vàng (optional)

    [Header("Anchors")]
    [Tooltip("Điểm neo popup damage (nếu null dùng transform unit)")]
    [SerializeField] private Transform damageAnchor;
    [Tooltip("Điểm neo popup vàng (nếu null dùng transform unit)")]
    [SerializeField] private Transform goldAnchor;
    [SerializeField] private Canvas targetCanvas; // Canvas dùng để hiển thị popup

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 30f, 0); // offset trong screen space (pixels)
    [SerializeField] private Vector3 goldOffset = new Vector3(0, 50f, 0); // offset cho popup vàng

    [Tooltip("Random offset để popup không stack đè lên nhau (pixels)")]
    [SerializeField] private float randomOffsetRange = 20f;

    private void Awake()
    {
        if (_camera == null) _camera = Camera.main;
        FindTargetCanvas();

        if (PopUp_Prefab == null)
        {
            PopUp_Prefab = Resources.Load<GameObject>("PopUp");
        }
    }

    private Canvas FindTargetCanvas()
    {
        if (targetCanvas != null) return targetCanvas;

        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.name == "CanvasUi" || canvas.name.Contains("CanvasUi"))
            {
                targetCanvas = canvas;
                return targetCanvas;
            }
        }

        foreach (Canvas canvas in allCanvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCanvas = canvas;
                return targetCanvas;
            }
        }

        if (allCanvases.Length > 0) targetCanvas = allCanvases[0];
        return targetCanvas;
    }

    /// <summary>
    /// Hiển thị popup damage khi unit nhận sát thương
    /// </summary>
    public void PopUpDame(int damage, Color? customColor = null)
    {
        if (PopUp_Prefab == null)
        {
            PopUp_Prefab = Resources.Load<GameObject>("PopUp");
        }

        if (PopUp_Prefab == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: PopUp_Prefab chưa được gán và không tìm thấy trong Resources!");
            return;
        }

        if (_camera == null) _camera = Camera.main;
        targetCanvas = FindTargetCanvas();

        string textDame = "-" + damage.ToString();
        Color colorToUse = customColor ?? Color.red;

        // Vị trí thế giới của unit (có nhích lên trên một chút để popup nằm trên đầu)
        Vector3 worldPos = (damageAnchor != null ? damageAnchor.position : transform.position);
        worldPos.y += 0.5f;

        Vector3 screenPos = (_camera != null) ? _camera.WorldToScreenPoint(worldPos) : Vector3.zero;

        // Random jitter để nhiều con số không đè lấp hoàn toàn
        Vector2 randomOffset = new Vector2(
            Random.Range(-randomOffsetRange, randomOffsetRange),
            Random.Range(0f, randomOffsetRange)
        );
        screenPos.x += offset.x + randomOffset.x;
        screenPos.y += offset.y + randomOffset.y;

        if (targetCanvas != null)
        {
            RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
            Vector2 localPoint;

            // Xử lý camera parameter đúng chuẩn Canvas ScreenSpaceOverlay vs ScreenSpaceCamera
            Camera uiCamera = (targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (targetCanvas.worldCamera ?? _camera);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                uiCamera,
                out localPoint))
            {
                GameObject popUpObject = Instantiate(PopUp_Prefab, targetCanvas.transform);

                RectTransform popupRect = popUpObject.GetComponent<RectTransform>();
                if (popupRect != null)
                {
                    popupRect.anchoredPosition = localPoint;
                    popupRect.localRotation = Quaternion.identity;
                    popupRect.localScale = Vector3.one;
                }
                else
                {
                    popUpObject.transform.localPosition = localPoint;
                }

                // Luôn hiển thị ở lớp trên cùng của Canvas
                popUpObject.transform.SetAsLastSibling();

                PopUp popUp = popUpObject.GetComponent<PopUp>();
                if (popUp != null)
                {
                    popUp.SetText(textDame, colorToUse);
                }
            }
        }
        else
        {
            // Fallback World space
            Vector3 spawnPos = _camera != null ? _camera.ScreenToWorldPoint(screenPos) : worldPos;
            spawnPos.z = 0f;

            GameObject popUpObject = Instantiate(PopUp_Prefab, spawnPos, Quaternion.identity);
            PopUp popUp = popUpObject.GetComponent<PopUp>();
            if (popUp != null)
            {
                popUp.SetText(textDame, colorToUse);
            }
        }
    }

    public void PopUpGold(int gold)
    {
    }

    public void PopUpGoldAt(int gold, Transform customAnchor)
    {
    }

    public void PopUpDameAt(int damage, Transform customAnchor, Color? customColor = null)
    {
        Transform prev = damageAnchor;
        damageAnchor = customAnchor;
        PopUpDame(damage, customColor);
        damageAnchor = prev;
    }
}
