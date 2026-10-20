using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StatImpactPopup : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI textUI;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float floatDistance = 100f;
    public float duration = 1.6f;
    public float fadeStartRatio = 0.5f;

    public void Setup(Sprite iconSprite, string statName, float value, Color positiveColor, Color negativeColor)
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Set Icon
        if (iconImage != null)
        {
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        // Format Text & Color
        if (textUI != null)
        {
            if (textUI.font == null || textUI.fontSharedMaterial == null)
            {
                TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/Baloo2-Bold SDF");
                if (fontAsset != null)
                {
                    textUI.font = fontAsset;
                    textUI.fontSharedMaterial = fontAsset.material;
                }
            }

            string sign = value > 0 ? "+" : "";
            textUI.text = $"{sign}{value} {statName}";
            textUI.color = value >= 0 ? positiveColor : negativeColor;
        }

        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 startPos = rect != null ? rect.anchoredPosition : (Vector2)transform.position;
        Vector2 endPos = startPos + new Vector2(0f, floatDistance);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Floating upward (Smooth Out Quad)
            float moveT = 1f - (1f - t) * (1f - t);
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, moveT);
            }
            else
            {
                transform.position = Vector3.Lerp(startPos, endPos, moveT);
            }

            // Fade Out
            if (t >= fadeStartRatio && canvasGroup != null)
            {
                float fadeT = (t - fadeStartRatio) / (1f - fadeStartRatio);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
