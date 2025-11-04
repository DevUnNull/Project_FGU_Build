using TMPro;
using UnityEngine;

/// <summary>
/// Component hiển thị popup damage trên enemy
/// </summary>
public class EnemyPopUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject PopUp_Prefab;
    [SerializeField] private GameObject GoldPopUp_Prefab; // prefab riêng cho vàng (optional)
    [Header("Anchors")]
    [Tooltip("Điểm neo popup damage (nếu null dùng transform enemy)")]
    [SerializeField] private Transform damageAnchor;
    [Tooltip("Điểm neo popup vàng (nếu null dùng transform enemy)")]
    [SerializeField] private Transform goldAnchor;
    [SerializeField] private Canvas targetCanvas; // Canvas dùng để hiển thị popup (Screen Space - Camera)

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 30f, 0); // offset trong screen space (pixels)
    [SerializeField] private Vector3 goldOffset = new Vector3(0, 50f, 0); // offset cho popup vàng

    [Tooltip("Random offset để popup không stack đè lên nhau (pixels)")]
    [SerializeField] private float randomOffsetRange = 20f;

    private void Awake()
    {
        // Tự động lấy camera nếu chưa gán
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        // Tự động tìm Canvas nếu chưa gán
        if (targetCanvas == null)
        {
            // Tìm Canvas có tên "CanvasUi" trước (ưu tiên)
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.name == "CanvasUi" || canvas.name.Contains("CanvasUi"))
                {
                    targetCanvas = canvas;
                    Debug.Log($"✅ Tìm thấy CanvasUi: {canvas.name}");
                    break;
                }
            }

            // Nếu không tìm thấy "CanvasUi", tìm Canvas có render mode Screen Space
            if (targetCanvas == null)
            {
                foreach (Canvas canvas in allCanvases)
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceCamera ||
                        canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        targetCanvas = canvas;
                        Debug.LogWarning($"⚠️ Không tìm thấy CanvasUi, dùng Canvas: {canvas.name}");
                        break;
                    }
                }
            }

            // Nếu vẫn không tìm thấy, lấy Canvas đầu tiên
            if (targetCanvas == null && allCanvases.Length > 0)
            {
                targetCanvas = allCanvases[0];
                Debug.LogWarning($"⚠️ Không tìm thấy CanvasUi, dùng Canvas đầu tiên: {targetCanvas.name}");
            }
        }

        // Kiểm tra prefab
        if (PopUp_Prefab == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: PopUp_Prefab chưa được gán trong Inspector!");
        }
    }

    /// <summary>
    /// Hiển thị popup damage khi enemy nhận sát thương
    /// </summary>
    /// <param name="damage">Số lượng damage</param>
    public void PopUpDame(int damage)
    {
        // Kiểm tra prefab
        if (PopUp_Prefab == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: Không thể hiển thị popup vì PopUp_Prefab chưa được gán!");
            return;
        }

        if (_camera == null)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: Camera chưa được gán!");
            return;
        }

        // Tạo text hiển thị
        string textDame = "-" + damage.ToString();

        // Lấy điểm neo (chính xác vào enemy) rồi convert sang screen position
        Vector3 worldPos = (damageAnchor != null ? damageAnchor.position : transform.position);
        Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);

        // Random offset trong screen space (pixels)
        Vector2 randomOffset = new Vector2(
            Random.Range(-randomOffsetRange, randomOffsetRange),
            Random.Range(0f, randomOffsetRange)
        );

        // Áp dụng offset trong screen space
        screenPos.x += offset.x + randomOffset.x;
        screenPos.y += offset.y + randomOffset.y;

        // Nếu có Canvas (Screen Space - Camera)
        if (targetCanvas != null)
        {
            // Convert screen position sang local position trong Canvas
            RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                targetCanvas.worldCamera ?? _camera,
                out localPoint))
            {
                // Tạo popup trong Canvas
                GameObject popUpObject = Instantiate(PopUp_Prefab, targetCanvas.transform);

                // Set local position
                RectTransform popupRect = popUpObject.GetComponent<RectTransform>();
                if (popupRect != null)
                {
                    popupRect.localPosition = localPoint;
                    popupRect.localRotation = Quaternion.identity;
                    popupRect.localScale = Vector3.one;
                }
                else
                {
                    popUpObject.transform.localPosition = localPoint;
                }

                // Set Sorting Order = -1 cho popup
                SetPopupSortingOrder(popUpObject, -1);

                // Set text
                PopUp popUp = popUpObject.GetComponent<PopUp>();
                if (popUp != null)
                {
                    popUp.text_Value = textDame;
                    popUp.textColor = Color.red;
                }
                else
                {
                    Debug.LogError($"❌ {gameObject.name}: PopUp component không tìm thấy trên prefab!");
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Không thể convert screen point sang canvas local point!");
            }
        }
        else
        {
            // Fallback: Tạo popup trong world space (nếu không có Canvas)
            Vector3 spawnPos = _camera.ScreenToWorldPoint(screenPos);
            spawnPos.z = 0f;

            GameObject popUpObject = Instantiate(PopUp_Prefab, spawnPos, Quaternion.identity);

            PopUp popUp = popUpObject.GetComponent<PopUp>();
            if (popUp != null)
            {
                popUp.text_Value = textDame;
                popUp.textColor = Color.red;
            }
        }
    }

    /// <summary>
    /// Hiển thị popup vàng khi thưởng gold sau khi enemy chết
    /// </summary>
    public void PopUpGold(int gold)
    {
        // if (gold <= 0) return;
        // GameObject prefab = GoldPopUp_Prefab != null ? GoldPopUp_Prefab : PopUp_Prefab;
        // if (prefab == null)
        // {
        //     Debug.LogWarning($"⚠️ {gameObject.name}: Không thể hiển thị popup vàng vì chưa gán prefab!");
        //     return;
        // }

        // if (_camera == null)
        // {
        //     _camera = Camera.main;
        //     if (_camera == null) return;
        // }

        // string text = "+" + gold.ToString();
        // Vector3 worldPos = (goldAnchor != null ? goldAnchor.position : transform.position);
        // Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);
        // Vector2 randomOffset = new Vector2(
        //     Random.Range(-randomOffsetRange, randomOffsetRange),
        //     Random.Range(0f, randomOffsetRange)
        // );
        // screenPos.x += goldOffset.x + randomOffset.x;
        // screenPos.y += goldOffset.y + randomOffset.y;

        // if (targetCanvas != null)
        // {
        //     RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
        //     Vector2 localPoint;
        //     if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //         canvasRect, screenPos, targetCanvas.worldCamera ?? _camera, out localPoint))
        //     {
        //         GameObject obj = Instantiate(prefab, targetCanvas.transform);
        //         RectTransform rect = obj.GetComponent<RectTransform>();
        //         if (rect != null) rect.localPosition = localPoint; else obj.transform.localPosition = localPoint;
        //         SetPopupSortingOrder(obj, -1);
        //         PopUp p = obj.GetComponent<PopUp>();
        //         if (p != null)
        //         {
        //             p.text_Value = text;
        //             p.textColor = new Color(1f, 0.9f, 0.2f); // vàng nhạt
        //         }
        //     }
        // }
        // else
        // {
        //     Vector3 spawnPos = _camera.ScreenToWorldPoint(screenPos);
        //     spawnPos.z = 0f;
        //     GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        //     PopUp p = obj.GetComponent<PopUp>();
        //     if (p != null)
        //     {
        //         p.text_Value = text;
        //         p.textColor = new Color(1f, 0.9f, 0.2f);
        //     }
        // }
    }

    // API chỉ định Transform anchor tùy ý cho popup vàng
    public void PopUpGoldAt(int gold, Transform customAnchor)
    {
        // Tạm thời thay goldAnchor theo custom trong 1 lần gọi
        Transform prev = goldAnchor;
        goldAnchor = customAnchor;
        PopUpGold(gold);
        goldAnchor = prev;
    }

    // API chỉ định Transform anchor tùy ý cho popup damage
    public void PopUpDameAt(int damage, Transform customAnchor)
    {
        Transform prev = damageAnchor;
        damageAnchor = customAnchor;
        PopUpDame(damage);
        damageAnchor = prev;
    }

    /// <summary>
    /// Set sorting order cho popup (để hiển thị phía sau hoặc phía trước)
    /// </summary>
    private void SetPopupSortingOrder(GameObject popup, int sortingOrder)
    {
        // Cách 1: Nếu popup có Canvas component (Additional Canvas)
        Canvas popupCanvas = popup.GetComponent<Canvas>();
        if (popupCanvas != null)
        {
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = sortingOrder;
        }
        else
        {
            // Cách 2: Thêm Canvas component với sorting order = -1
            popupCanvas = popup.AddComponent<Canvas>();
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = sortingOrder;

            // Thêm GraphicRaycaster để vẫn có thể tương tác (nếu cần)
            // popup.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }
}
