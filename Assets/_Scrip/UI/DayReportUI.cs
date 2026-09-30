using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DayReportUI : MonoBehaviour
{
    private GameObject reportPanel;
    private TextMeshProUGUI reportText;
    private Button closeButton;

    private void Start()
    {
        // Nếu không có dữ liệu StomachDayData, không cần hiện báo cáo
        if (StomachDayData.Instance == null)
        {
            return;
        }

        // 1. Tự động khởi tạo toàn bộ giao diện (Canvas, Panel, Text, Button) bằng code
        CreateUI();

        // 2. Viết nội dung báo cáo
        string report = "<b><size=120%>BÁO CÁO TÌNH TRẠNG CƠ THỂ</size></b>\n\n";

        // Chỉ số có lợi (Buff / Debuff)
        float atp = StomachDayData.Instance.atpRecoveryMultiplier;
        report += FormatStat("- Tốc độ hồi ATP", atp);

        float hp = StomachDayData.Instance.mucosaHpMultiplier;
        report += FormatStat("- Sinh lực (HP) Tế bào", hp);

        float dmg = StomachDayData.Instance.cellDamageMultiplier;
        report += FormatStat("- Sát thương Tế bào", dmg);

        float atkSpeed = StomachDayData.Instance.cellAttackSpeedMultiplier;
        report += FormatStat("- Tốc độ đánh", atkSpeed);

        report += "\n<b><size=120%>ẢNH HƯỞNG TIÊU CỰC</size></b>\n\n";
        bool hasNegative = false;

        // Chỉ số bất lợi (Event)
        float acid = StomachDayData.Instance.gastricAcidLevel;
        if (acid > 0)
        {
            report += $"- <color=#ff4444>Mức Axit dạ dày tăng mạnh (+{acid})!</color>\n";
            hasNegative = true;
        }

        int toxins = StomachDayData.Instance.toxinObstaclesCount;
        if (toxins > 0)
        {
            report += $"- <color=#ff4444>Có {toxins} khối độc tố cản đường!</color>\n";
            hasNegative = true;
        }

        if (!hasNegative)
        {
            report += "- <color=#44ff44>Không có ảnh hưởng xấu nào.</color>\n";
        }

        reportText.text = report;
        closeButton.onClick.AddListener(CloseReport);
    }

    private void CreateUI()
    {
        // Tạo Canvas độc lập cho báo cáo (nằm đè lên trên cùng)
        GameObject canvasObj = new GameObject("DayReportCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Lớp trên cùng
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // Đảm bảo có EventSystem để bấm được nút
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();
        }

        // Tạo màn nền đen mờ
        reportPanel = new GameObject("ReportPanel");
        reportPanel.transform.SetParent(canvasObj.transform, false);
        Image panelImage = reportPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);
        RectTransform panelRect = reportPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // Tạo hộp thoại (Dialog Box) ở giữa
        GameObject dialogBox = new GameObject("DialogBox");
        dialogBox.transform.SetParent(reportPanel.transform, false);
        Image dialogImage = dialogBox.AddComponent<Image>();
        dialogImage.color = new Color(0.15f, 0.15f, 0.2f, 1f); // Màu xám xanh tối
        RectTransform dialogRect = dialogBox.GetComponent<RectTransform>();
        dialogRect.sizeDelta = new Vector2(800, 600);

        // Tạo Text hiển thị báo cáo
        GameObject textObj = new GameObject("ReportText");
        textObj.transform.SetParent(dialogBox.transform, false);
        reportText = textObj.AddComponent<TextMeshProUGUI>();
        reportText.color = Color.white;
        reportText.fontSize = 32;
        reportText.alignment = TextAlignmentOptions.TopLeft;
        reportText.richText = true;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(50, 100); 
        textRect.offsetMax = new Vector2(-50, -50);

        // Tạo Nút Đóng (Xác nhận)
        GameObject btnObj = new GameObject("CloseButton");
        btnObj.transform.SetParent(dialogBox.transform, false);
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.7f, 0.2f, 1f); // Màu xanh lá
        closeButton = btnObj.AddComponent<Button>();
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.sizeDelta = new Vector2(250, 60);
        btnRect.anchoredPosition = new Vector2(0, 50);

        // Chữ trong nút Đóng
        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "Xác nhận & Bắt đầu";
        btnText.color = Color.white;
        btnText.fontSize = 28;
        btnText.alignment = TextAlignmentOptions.Center;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
    }

    private string FormatStat(string statName, float multiplier)
    {
        float percent = multiplier * 100f;
        if (multiplier > 1.0f)
        {
            return $"{statName}: <color=#44ff44>{percent}% (Tăng cường)</color>\n";
        }
        else if (multiplier < 1.0f)
        {
            return $"{statName}: <color=#ff4444>{percent}% (Suy giảm)</color>\n";
        }
        else
        {
            return $"{statName}: <color=white>{percent}% (Bình thường)</color>\n";
        }
    }

    private void CloseReport()
    {
        // Khi bấm nút, xoá toàn bộ Canvas chứa giao diện báo cáo này
        Destroy(reportPanel.transform.parent.gameObject);
    }
}
