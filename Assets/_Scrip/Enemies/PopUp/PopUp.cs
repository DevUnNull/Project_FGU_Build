using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    public string text_Value;
    
    [Header("Style")]
    public Color textColor = Color.red;
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float moveSpeed = 60f; // Tốc độ di chuyển lên trên (pixels/sec)

    private float timer = 0f;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (textMeshProUGUI == null)
        {
            textMeshProUGUI = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        UpdateDisplay();
        Destroy(gameObject, lifetime);
    }

    public void SetText(string text, Color color)
    {
        text_Value = text;
        textColor = color;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (textMeshProUGUI == null)
            textMeshProUGUI = GetComponent<TextMeshProUGUI>() ?? GetComponentInChildren<TextMeshProUGUI>();

        if (textMeshProUGUI != null)
        {
            textMeshProUGUI.text = text_Value;
            textMeshProUGUI.color = textColor;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Trôi lên trên
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += new Vector2(0f, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.up * (moveSpeed * 0.01f) * Time.deltaTime);
        }

        // Fade out mờ dần
        if (textMeshProUGUI != null && lifetime > 0f)
        {
            float alpha = Mathf.Clamp01(1f - (timer / lifetime));
            Color c = textColor;
            c.a = alpha;
            textMeshProUGUI.color = c;
        }
    }
}
