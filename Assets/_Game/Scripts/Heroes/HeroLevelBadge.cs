using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Huy hiệu số cấp ở góc dưới phải tế bào (chỉ hiện từ cấp 2) + chữ "Cấp 2!" bay lên khi vừa ghép.
/// </summary>
public class HeroLevelBadge : MonoBehaviour
{
    private const float BadgeWorldSize = 0.6f;
    private const int SortingOrder = 30;
    private static readonly Color BadgeTextColor = new Color(0.45f, 0.22f, 0.02f, 1f);
    private static readonly Color PopupColor = new Color(1f, 0.85f, 0.2f, 1f);

    private Transform badge;
    private TextMeshPro badgeText;
    private GameObject popup;

    public static HeroLevelBadge GetOrAdd(GameObject go)
    {
        HeroLevelBadge b = go.GetComponent<HeroLevelBadge>();
        return b != null ? b : go.AddComponent<HeroLevelBadge>();
    }

    public void Show(int level, bool playLevelUp)
    {
        if (level <= 1)
        {
            if (badge != null) badge.gameObject.SetActive(false);
            return;
        }

        if (badge == null) CreateBadge();
        badge.gameObject.SetActive(true);
        badgeText.text = level.ToString();
        PlaceBadge();

        if (playLevelUp) StartCoroutine(LevelUpPopup(level));
    }

    private void CreateBadge()
    {
        badge = new GameObject("LevelBadge").transform;
        badge.SetParent(transform, false);

        // Khung (level_Charactor): co ảnh về đúng 1x1 đơn vị trong huy hiệu, bất kể kích thước ảnh gốc
        Sprite sprite = UISkin.Instance != null ? UISkin.Instance.levelBadge : null;
        if (sprite != null)
        {
            var bg = new GameObject("Frame");
            bg.transform.SetParent(badge, false);
            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = SortingOrder;
            float size = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            if (size > 0f) bg.transform.localScale = Vector3.one / size;
        }

        var textGo = new GameObject("Number", typeof(RectTransform));
        textGo.transform.SetParent(badge, false);
        badgeText = textGo.AddComponent<TextMeshPro>();
        if (RuntimeUI.Font != null) badgeText.font = RuntimeUI.Font;
        badgeText.alignment = TextAlignmentOptions.Center;
        badgeText.fontStyle = FontStyles.Bold;
        badgeText.color = sprite != null ? BadgeTextColor : PopupColor;
        badgeText.enableAutoSizing = true;
        badgeText.fontSizeMin = 1f;
        badgeText.fontSizeMax = 30f;
        badgeText.sortingOrder = SortingOrder + 1;
        badgeText.rectTransform.sizeDelta = new Vector2(0.45f, 0.45f); // vừa lòng tròn màu kem của khung
    }

    /// <summary>Đặt huy hiệu ở góc dưới phải hình tế bào, giữ kích thước thế giới cố định và không bị lật theo tế bào.</summary>
    private void PlaceBadge()
    {
        Bounds bounds = GetUnitBounds();
        Vector3 worldPos = new Vector3(bounds.max.x - BadgeWorldSize * 0.4f, bounds.min.y + BadgeWorldSize * 0.4f, transform.position.z - 0.1f);
        badge.position = worldPos;

        Vector3 lossy = transform.lossyScale;
        badge.localScale = new Vector3(BadgeWorldSize / NonZero(lossy.x), BadgeWorldSize / NonZero(lossy.y), 1f);
    }

    private Bounds GetUnitBounds()
    {
        Bounds bounds = new Bounds(transform.position, Vector3.one);
        bool found = false;
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (badge != null && sr.transform.IsChildOf(badge)) continue;
            if (!found) { bounds = sr.bounds; found = true; }
            else bounds.Encapsulate(sr.bounds);
        }
        return bounds;
    }

    private static float NonZero(float v) => Mathf.Abs(v) < 0.0001f ? 0.0001f : v;

    private IEnumerator LevelUpPopup(int level)
    {
        if (popup != null) Destroy(popup);
        var go = new GameObject("LevelUpText", typeof(RectTransform));
        popup = go;
        TextMeshPro text = go.AddComponent<TextMeshPro>();
        if (RuntimeUI.Font != null) text.font = RuntimeUI.Font;
        text.text = "Cấp " + level + "!";
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.color = PopupColor;
        text.outlineWidth = 0.25f;
        text.outlineColor = new Color(0.35f, 0.15f, 0f, 1f);
        text.enableAutoSizing = true;
        text.fontSizeMin = 1f;
        text.fontSizeMax = 30f;
        text.sortingOrder = SortingOrder + 2;
        text.rectTransform.sizeDelta = new Vector2(2.2f, 0.7f);

        Bounds bounds = GetUnitBounds();
        Vector3 start = new Vector3(bounds.center.x, bounds.max.y + 0.1f, transform.position.z - 0.2f);
        const float duration = 1.1f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = t / duration;
            go.transform.position = start + Vector3.up * (0.6f * k);
            go.transform.localScale = Vector3.one * (k < 0.15f ? Mathf.Lerp(0.6f, 1.15f, k / 0.15f) : 1f);
            Color c = text.color;
            c.a = k < 0.7f ? 1f : 1f - (k - 0.7f) / 0.3f;
            text.color = c;
            yield return null;
        }
        Destroy(go);
    }

    private void OnDestroy()
    {
        // Chữ bay lên không phải con của tế bào -> tế bào bị hủy giữa chừng thì dọn luôn
        if (popup != null) Destroy(popup);
    }
}
