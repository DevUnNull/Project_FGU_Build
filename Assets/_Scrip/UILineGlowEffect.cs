using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UILineGlowEffect : MonoBehaviour
{
    [Header("Aura Settings (Hào Quang)")]
    public Color auraColor = new Color(0.5f, 1f, 0.2f, 0.4f); // Vàng xanh nhẹ, hơi trong suốt
    
    [Range(0, 50)]
    public float auraSpread = 20f; // Độ tỏa ra (pixel)
    
    [Range(1, 10)]
    public int auraLayers = 4; // Số lớp hào quang (càng nhiều càng mịn)

    private Image parentImage;
    private GameObject[] auraLayersObjects;

    private void Start()
    {
        parentImage = GetComponent<Image>();
        CreateAura();
    }

    private void CreateAura()
    {
        // Xóa các aura cũ nếu có
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("AuraLayer_"))
            {
                Destroy(child.gameObject);
            }
        }

        auraLayersObjects = new GameObject[auraLayers];

        for (int i = 0; i < auraLayers; i++)
        {
            GameObject layerObj = new GameObject($"AuraLayer_{i}");
            layerObj.transform.SetParent(transform, false);
            layerObj.transform.SetAsFirstSibling(); // Đưa xuống dưới cùng

            Image layerImage = layerObj.AddComponent<Image>();
            layerImage.sprite = parentImage.sprite;
            layerImage.type = parentImage.type;
            
            // Chia alpha cho số layer để màu mềm mại hơn
            Color layerColor = auraColor;
            layerColor.a = auraColor.a / auraLayers;
            layerImage.color = layerColor;
            
            layerImage.raycastTarget = false;

            RectTransform rect = layerObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            
            // Size tăng dần từ trong ra ngoài để tạo độ mờ (gradient)
            float currentSpread = auraSpread * ((float)(i + 1) / auraLayers);
            rect.sizeDelta = new Vector2(currentSpread, currentSpread);
            
            auraLayersObjects[i] = layerObj;
        }
    }

    private void OnValidate()
    {
        // Tự động cập nhật khi bạn kéo thanh trượt trong Play Mode
        if (Application.isPlaying && auraLayersObjects != null && auraLayersObjects.Length == auraLayers)
        {
            for (int i = 0; i < auraLayers; i++)
            {
                if (auraLayersObjects[i] != null)
                {
                    Image img = auraLayersObjects[i].GetComponent<Image>();
                    Color layerColor = auraColor;
                    layerColor.a = auraColor.a / auraLayers;
                    img.color = layerColor;

                    RectTransform rect = auraLayersObjects[i].GetComponent<RectTransform>();
                    float currentSpread = auraSpread * ((float)(i + 1) / auraLayers);
                    rect.sizeDelta = new Vector2(currentSpread, currentSpread);
                }
            }
        }
        else if (Application.isPlaying && parentImage != null)
        {
            // Nếu số lượng layer thay đổi, tạo lại từ đầu
            CreateAura();
        }
    }
}
