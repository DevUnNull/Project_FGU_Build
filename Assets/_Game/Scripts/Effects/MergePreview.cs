using TMPro;
using UnityEngine;

/// <summary>
/// Hiệu ứng báo "ghép được" khi đang kéo tế bào (DragAndDrop điều khiển):
/// - Candidate: viền sáng vàng nhấp nháy nhẹ + chữ "Ghép được" nhún nhảy -> biết thả vào đâu.
/// - Hover: đang kéo ngay trên con này -> viền sáng mạnh, nhịp nhanh + "Thả để lên cấp N!".
/// - Dragged: con đang được kéo, chỉ sáng viền (không có chữ).
/// Viền sáng = bản sao hình tế bào phóng to, tô vàng, vẽ ngay phía sau tế bào.
/// </summary>
public class MergePreview : MonoBehaviour
{
    public enum Mode { None, Candidate, Hover, Dragged }

    private static readonly Color GlowColor = new Color(1f, 0.85f, 0.15f, 1f);
    private static readonly Color LabelColor = new Color(1f, 0.9f, 0.25f, 1f);
    private static readonly Color LabelOutline = new Color(0.35f, 0.15f, 0f, 1f);
    private const float GlowScale = 1.18f;
    private const int LabelSortingOrder = 40;

    private Mode mode = Mode.None;
    private string hoverText;
    private SpriteRenderer source;
    private SpriteRenderer glow;
    private TextMeshPro label;

    public static MergePreview GetOrAdd(GameObject go)
    {
        MergePreview p = go.GetComponent<MergePreview>();
        return p != null ? p : go.AddComponent<MergePreview>();
    }

    /// <param name="nextLevel">Cấp sau khi ghép (hiện trong chữ ở chế độ Hover)</param>
    public void SetMode(Mode newMode, int nextLevel = 2)
    {
        mode = newMode;
        hoverText = $"Thả để lên cấp {nextLevel}!";
        if (mode == Mode.None)
        {
            if (glow != null) glow.enabled = false;
            if (label != null) label.gameObject.SetActive(false);
            return;
        }

        EnsureVisuals();
        glow.enabled = true;
        bool showLabel = mode == Mode.Candidate || mode == Mode.Hover;
        label.gameObject.SetActive(showLabel);
        label.text = mode == Mode.Hover ? hoverText : "Ghép được";
        label.fontSizeMax = mode == Mode.Hover ? 30f : 22f;
    }

    private void EnsureVisuals()
    {
        if (source == null) source = FindMainRenderer();
        if (glow == null && source != null)
        {
            var go = new GameObject("MergeGlow");
            go.transform.SetParent(source.transform, false);
            go.transform.localScale = Vector3.one * GlowScale;
            glow = go.AddComponent<SpriteRenderer>();
            glow.sortingLayerID = source.sortingLayerID;
            glow.sortingOrder = source.sortingOrder - 1;
        }

        if (label == null)
        {
            var go = new GameObject("MergeLabel", typeof(RectTransform));
            label = go.AddComponent<TextMeshPro>();
            if (RuntimeUI.Font != null) label.font = RuntimeUI.Font;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.color = LabelColor;
            label.outlineWidth = 0.3f;
            label.outlineColor = LabelOutline;
            label.enableAutoSizing = true;
            label.fontSizeMin = 1f;
            label.sortingOrder = LabelSortingOrder;
            label.rectTransform.sizeDelta = new Vector2(3f, 0.6f);
        }
    }

    // Hình chính của tế bào = SpriteRenderer lớn nhất (bỏ qua huy hiệu cấp, viền sáng)
    private SpriteRenderer FindMainRenderer()
    {
        SpriteRenderer best = null;
        float bestArea = 0f;
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.name == "MergeGlow" || sr.transform.parent != null && sr.transform.parent.name == "LevelBadge") continue;
            float area = sr.bounds.size.x * sr.bounds.size.y;
            if (area > bestArea) { best = sr; bestArea = area; }
        }
        return best;
    }

    private void LateUpdate()
    {
        if (mode == Mode.None) return;

        bool strong = mode != Mode.Candidate;
        float speed = strong ? 9f : 4.5f;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed);

        if (glow != null && source != null)
        {
            // Bám theo khung hình hiện tại của animation
            glow.sprite = source.sprite;
            glow.flipX = source.flipX;
            glow.flipY = source.flipY;
            Color c = GlowColor;
            c.a = strong ? Mathf.Lerp(0.55f, 0.95f, pulse) : Mathf.Lerp(0.25f, 0.6f, pulse);
            glow.color = c;
            float s = GlowScale + (strong ? 0.08f : 0.03f) * pulse;
            glow.transform.localScale = Vector3.one * s;
        }

        if (label != null && label.gameObject.activeSelf && source != null)
        {
            Bounds b = source.bounds;
            float bob = Mathf.Abs(Mathf.Sin(Time.unscaledTime * (strong ? 7f : 4f))) * 0.15f;
            label.transform.position = new Vector3(b.center.x, b.max.y + 0.3f + bob, transform.position.z - 0.3f);
            label.transform.localScale = Vector3.one * (strong ? 1f + 0.08f * pulse : 1f);
        }
    }

    private void OnDisable()
    {
        if (label != null) label.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Chữ không phải con của tế bào -> dọn theo
        if (label != null) Destroy(label.gameObject);
    }
}
