using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Hiệu ứng Sóng Axit Dạ Dày tràn qua 1 hàng của bàn cờ:
/// - Giai đoạn cảnh báo: dải axit nhấp nháy + chữ cảnh báo
/// - Giai đoạn gây sát thương: dải axit đặc lại và sóng sánh
/// - Kết thúc: mờ dần rồi tự hủy
/// Tạo hoàn toàn bằng code, không cần Prefab.
/// </summary>
public class GastricAcidWaveEffect : MonoBehaviour
{
    private static readonly Color AcidColor = new Color(0.55f, 0.95f, 0.15f, 1f);
    private const int SortingOrder = 6; // Trên ô Tile, dưới Tế bào (sortingOrder 10)

    private SpriteRenderer bandRenderer;
    private TextMeshPro warningText;

    public static GastricAcidWaveEffect Create(List<TileCell> rowTiles, float cellHeight, float warningTime, float duration)
    {
        GameObject go = new GameObject("GastricAcidWaveEffect");
        GastricAcidWaveEffect effect = go.AddComponent<GastricAcidWaveEffect>();
        effect.Build(CalculateRowBounds(rowTiles, cellHeight));
        effect.StartCoroutine(effect.PlayRoutine(warningTime, duration));
        return effect;
    }

    /// <summary>
    /// Tính vùng phủ của hàng từ vị trí các ô (ô Tile có thể vô hình nên không dùng bounds của SpriteRenderer).
    /// Chiều rộng 1 ô = khoảng cách trung bình giữa các cột, chiều cao = khoảng cách giữa các hàng.
    /// </summary>
    private static Bounds CalculateRowBounds(List<TileCell> rowTiles, float cellHeight)
    {
        float minX = float.MaxValue, maxX = float.MinValue, sumY = 0f;
        foreach (var tile in rowTiles)
        {
            minX = Mathf.Min(minX, tile.CenterPosition.x);
            maxX = Mathf.Max(maxX, tile.CenterPosition.x);
            sumY += tile.CenterPosition.y;
        }

        float cellWidth = rowTiles.Count > 1 ? (maxX - minX) / (rowTiles.Count - 1) : 1.5f;
        Vector3 center = new Vector3((minX + maxX) * 0.5f, sumY / rowTiles.Count, 0f);
        return new Bounds(center, new Vector3(maxX - minX + cellWidth, cellHeight, 0f));
    }

    private void Build(Bounds bounds)
    {
        transform.position = new Vector3(bounds.center.x, bounds.center.y, 0f);

        // Dải axit: sprite 1x1 trắng được kéo giãn phủ cả hàng
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        GameObject band = new GameObject("AcidBand");
        band.transform.SetParent(transform, false);
        band.transform.localScale = new Vector3(bounds.size.x, bounds.size.y * 0.9f, 1f);
        bandRenderer = band.AddComponent<SpriteRenderer>();
        bandRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        bandRenderer.sortingOrder = SortingOrder;
        bandRenderer.color = WithAlpha(AcidColor, 0f);

        // Chữ cảnh báo phía trên hàng
        GameObject textGo = new GameObject("AcidWarningText");
        textGo.transform.SetParent(transform, false);
        textGo.transform.localPosition = new Vector3(0f, bounds.size.y * 0.5f + 0.3f, 0f);
        warningText = textGo.AddComponent<TextMeshPro>();
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Baloo2-Bold SDF");
        if (font != null) warningText.font = font;
        warningText.text = "AXIT DẠ DÀY TRÀN!";
        warningText.fontSize = 5f;
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.color = new Color(1f, 0.95f, 0.3f, 1f);
        warningText.outlineWidth = 0.25f;
        warningText.outlineColor = new Color32(40, 60, 0, 255);
        warningText.sortingOrder = 30;
    }

    private IEnumerator PlayRoutine(float warningTime, float duration)
    {
        // 1. Cảnh báo: nhấp nháy nhanh
        for (float t = 0f; t < warningTime; t += Time.deltaTime)
        {
            float blink = Mathf.PingPong(t * 4f, 1f);
            bandRenderer.color = WithAlpha(AcidColor, Mathf.Lerp(0.1f, 0.4f, blink));
            warningText.alpha = Mathf.Lerp(0.4f, 1f, blink);
            yield return null;
        }

        // 2. Gây sát thương: axit đặc lại, sóng sánh nhẹ
        Vector3 baseScale = bandRenderer.transform.localScale;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float wave = Mathf.Sin(t * 6f);
            bandRenderer.color = WithAlpha(AcidColor, 0.5f + wave * 0.08f);
            bandRenderer.transform.localScale = new Vector3(baseScale.x, baseScale.y * (1f + wave * 0.04f), 1f);
            warningText.alpha = Mathf.Max(0f, 1f - t); // Chữ mờ dần trong 1s đầu
            yield return null;
        }

        // 3. Rút axit: mờ dần
        Color start = bandRenderer.color;
        for (float t = 0f; t < 0.6f; t += Time.deltaTime)
        {
            bandRenderer.color = WithAlpha(start, Mathf.Lerp(start.a, 0f, t / 0.6f));
            yield return null;
        }

        Destroy(bandRenderer.sprite.texture);
        Destroy(gameObject);
    }

    private static Color WithAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }
}
