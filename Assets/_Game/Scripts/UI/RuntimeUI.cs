using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Bộ dựng UI bằng code (không cần Prefab/Scene) dùng chung cho các bảng thông tin:
/// thẻ kiến thức, bảng tổng kết, gợi ý trong trận, nút tốc độ...
/// Mọi hiệu ứng dùng unscaledDeltaTime để vẫn chạy khi game Pause hoặc tăng tốc.
/// </summary>
public static class RuntimeUI
{
    // Bảng màu chung
    public static readonly Color Backdrop = new Color(0f, 0f, 0f, 0.6f);
    public static readonly Color CardBg = new Color(1f, 0.98f, 0.92f, 1f);
    public static readonly Color HeaderGreen = new Color(0.22f, 0.62f, 0.32f, 1f);
    public static readonly Color HeaderRed = new Color(0.78f, 0.25f, 0.22f, 1f);
    public static readonly Color HeaderOrange = new Color(0.9f, 0.6f, 0.15f, 1f);
    public static readonly Color TextDark = new Color(0.18f, 0.16f, 0.14f, 1f);
    public static readonly Color PositiveText = new Color(0.12f, 0.55f, 0.2f, 1f);
    public static readonly Color NegativeText = new Color(0.78f, 0.2f, 0.16f, 1f);

    private static TMP_FontAsset cachedFont;

    public static TMP_FontAsset Font
    {
        get
        {
            if (cachedFont == null) cachedFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Baloo2-Bold SDF");
            return cachedFont;
        }
    }

    public static Canvas CreateOverlayCanvas(string name, int sortingOrder)
    {
        EnsureEventSystem();

        GameObject go = new GameObject(name, typeof(RectTransform));
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
        // Project dùng Input System mới nếu có module tương ứng, ngược lại dùng StandaloneInputModule
        Type inputSystemModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputSystemModule != null) es.AddComponent(inputSystemModule);
        else es.AddComponent<StandaloneInputModule>();
    }

    public static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        return rt;
    }

    public static RectTransform Stretch(Transform parent, string name) =>
        CreateRect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

    public static RectTransform CreateCentered(Transform parent, string name, Vector2 size)
    {
        RectTransform rt = CreateRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rt.sizeDelta = size;
        return rt;
    }

    public static Image AddImage(RectTransform rt, Color color, bool raycastTarget = true)
    {
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = raycastTarget;
        return img;
    }

    public static TextMeshProUGUI CreateText(Transform parent, string text, float fontSize, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        RectTransform rt = Stretch(parent, "Text");
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (Font != null) tmp.font = Font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = true;
        tmp.raycastTarget = false;
        return tmp;
    }

    /// <summary>
    /// Nút chung cho UI tạo bằng code: dùng khung viên thuốc countWave (UISkin).
    /// bg chỉ dùng khi thiếu UISkin (nút phẳng dự phòng).
    /// </summary>
    public static Button CreateButton(Transform parent, string label, Color bg, Vector2 size, Action onClick)
    {
        return UISkin.PillSprite != null
            ? CreatePillButton(parent, label, size, onClick)
            : CreateFlatButton(parent, label, bg, size, onClick);
    }

    private static Button CreateFlatButton(Transform parent, string label, Color bg, Vector2 size, Action onClick)
    {
        RectTransform rt = CreateCentered(parent, "Button_" + label, size);
        Image img = AddImage(rt, bg);
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        btn.colors = colors;

        CreateText(rt, label, 34f, Color.white);
        if (onClick != null) btn.onClick.AddListener(() => onClick());
        UIClickSounds.Attach(btn);
        return btn;
    }

    // Kích thước viền khung lớn: số đơn vị UI trên mỗi pixel ảnh gốc CanhBao
    private const float PanelUnitsPerPixel = 0.45f;
    private const float CardBannerHeight = 150f;

    private static Vector4 Scale(Vector4 v, float k) => new Vector4(v.x * k, v.y * k, v.z * k, v.w * k);

    /// <summary>Đặt khoảng cách viền (trái, dưới, phải, trên) cho rect con giãn theo cha.</summary>
    private static void SetInsets(RectTransform rt, Vector4 inset)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(inset.x, inset.y);
        rt.offsetMax = new Vector2(-inset.z, -inset.w);
    }

    /// <summary>
    /// Băng cảnh báo CanhBao (tam giác "!" bên trái) co giãn theo chiều ngang, tam giác giữ đúng tỉ lệ theo chiều cao.
    /// content = vùng ruột kem để đặt chữ. Không có UISkin thì trả về khung phẳng.
    /// </summary>
    public static RectTransform CreateWarningBanner(Transform parent, string name, Vector2 size, out RectTransform content)
    {
        RectTransform banner = CreateCentered(parent, name, size);
        Sprite sprite = UISkin.BannerSprite;

        if (sprite == null)
        {
            AddImage(banner, new Color(0.15f, 0.15f, 0.2f, 0.92f), false);
            content = Stretch(banner, "Content");
            SetInsets(content, new Vector4(24f, 6f, 24f, 6f));
            return banner;
        }

        // Quy đổi để chiều cao dải ảnh gốc (500px) vừa khít chiều cao băng
        float k = size.y / UISkin.BandOriginalHeight;
        Image img = AddImage(banner, Color.white, false);
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = UISkin.BannerScale / k;

        content = Stretch(banner, "Content");
        SetInsets(content, Scale(UISkin.BannerContentInset, k));
        return banner;
    }

    /// <summary>Nút khung viên thuốc countWave (chữ trắng). Không có UISkin thì dùng nút phẳng.</summary>
    public static Button CreatePillButton(Transform parent, string label, Vector2 size, Action onClick)
    {
        Sprite sprite = UISkin.PillSprite;
        if (sprite == null) return CreateFlatButton(parent, label, new Color(0.2f, 0.45f, 0.75f, 0.95f), size, onClick);

        RectTransform rt = CreateCentered(parent, "Button_" + label, size);
        Image img = AddImage(rt, Color.white);
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = UISkin.PillScale * UISkin.PillOriginalHeight / size.y;

        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        TextMeshProUGUI text = CreateText(rt, label, size.y * 0.42f, Color.white);
        text.fontStyle = FontStyles.Bold;
        if (onClick != null) btn.onClick.AddListener(() => onClick());
        UIClickSounds.Attach(btn);
        return btn;
    }

    /// <summary>
    /// Đổi một nút có sẵn trong Scene sang khung viên thuốc countWave (chữ trắng). Không có UISkin thì giữ nguyên.
    /// </summary>
    public static void ApplyPillStyle(Button button)
    {
        Sprite sprite = UISkin.PillSprite;
        if (button == null || sprite == null) return;

        Image img = button.targetGraphic as Image ?? button.GetComponent<Image>();
        if (img == null) img = button.gameObject.AddComponent<Image>();

        float height = Mathf.Max(1f, ((RectTransform)button.transform).rect.height);
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.preserveAspect = false;
        img.color = Color.white;
        img.pixelsPerUnitMultiplier = UISkin.PillScale * UISkin.PillOriginalHeight / height;

        // Ảnh đổi theo trạng thái (SpriteSwap) sẽ đè khung mới -> dùng đổi màu nhẹ
        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = img;

        foreach (var tmp in button.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            tmp.color = Color.white;
            tmp.fontStyle |= FontStyles.Bold;
        }
        foreach (var legacy in button.GetComponentsInChildren<Text>(true)) legacy.color = Color.white;
    }

    /// <summary>
    /// Thẻ lớn: thân khung ghép từ 2 nửa đối xứng của băng CanhBao + băng cảnh báo chứa tiêu đề đè lên mép trên.
    /// body = vùng nội dung bên trong. Không có UISkin thì dùng kiểu phẳng có thanh tiêu đề màu.
    /// </summary>
    public static RectTransform CreateCard(Transform parent, Vector2 size, string title, Color headerColor, out RectTransform body)
    {
        Sprite panelSprite = UISkin.PanelSprite;
        if (panelSprite == null) return CreateFlatCard(parent, size, title, headerColor, out body);

        RectTransform card = CreateCentered(parent, "Card", size);
        float multiplier = UISkin.BannerScale / PanelUnitsPerPixel;

        // Nửa phải nguyên bản, nửa trái là bản lật ngang -> khung đối xứng, co giãn tự do
        for (int side = 0; side < 2; side++)
        {
            RectTransform half = CreateRect(card, side == 0 ? "PanelLeft" : "PanelRight",
                new Vector2(side == 0 ? 0f : 0.5f, 0f), new Vector2(side == 0 ? 0.5f : 1f, 1f), Vector2.zero, Vector2.zero);
            if (side == 0) half.localScale = new Vector3(-1f, 1f, 1f);
            Image img = AddImage(half, Color.white);
            img.sprite = panelSprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = multiplier;
        }

        // Băng tiêu đề đè lên mép trên của thẻ
        float bannerWidth = Mathf.Min(size.x - 60f, 1000f);
        RectTransform banner = CreateWarningBanner(card, "TitleBanner", new Vector2(bannerWidth, CardBannerHeight), out RectTransform titleArea);
        banner.anchorMin = banner.anchorMax = new Vector2(0.5f, 1f);
        banner.anchoredPosition = new Vector2(0f, -CardBannerHeight * 0.2f);
        TextMeshProUGUI titleText = CreateText(titleArea, title, 40f, Darken(headerColor));
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 22f;
        titleText.fontSizeMax = 40f;

        // Vùng nội dung: dưới băng tiêu đề, trong viền khung
        Vector4 inset = Scale(UISkin.PanelContentInset, PanelUnitsPerPixel);
        body = Stretch(card, "Body");
        SetInsets(body, new Vector4(inset.z + 10f, inset.y + 10f, inset.z + 10f, CardBannerHeight * 0.8f + 10f));
        return card;
    }

    private static Color Darken(Color c) => new Color(c.r * 0.8f, c.g * 0.8f, c.b * 0.8f, 1f);

    /// <summary>Kiểu phẳng cũ (dùng khi thiếu UISkin): thanh tiêu đề màu + nền kem.</summary>
    private static RectTransform CreateFlatCard(Transform parent, Vector2 size, string title, Color headerColor, out RectTransform body)
    {
        RectTransform card = CreateCentered(parent, "Card", size);
        AddImage(card, CardBg);

        const float headerHeight = 80f;
        RectTransform header = CreateRect(card, "Header", new Vector2(0f, 1f), Vector2.one,
            new Vector2(0f, -headerHeight), Vector2.zero);
        AddImage(header, headerColor);
        CreateText(header, title, 40f, Color.white);

        body = CreateRect(card, "Body", Vector2.zero, Vector2.one,
            new Vector2(40f, 30f), new Vector2(-40f, -headerHeight - 20f));
        return card;
    }

    private static readonly Dictionary<Sprite, Sprite> pillSlices = new Dictionary<Sprite, Sprite>();

    /// <summary>
    /// Bản 9-slice của ảnh hình viên thuốc nằm ngang: 2 đầu (rộng = nửa chiều cao) giữ nguyên, phần giữa kéo giãn.
    /// Dùng với Image.type = Sliced và pixelsPerUnitMultiplier = chiều cao ảnh / chiều cao ô.
    /// </summary>
    public static Sprite SliceHorizontalPill(Sprite src)
    {
        if (src == null) return null;
        if (pillSlices.TryGetValue(src, out Sprite cached) && cached != null) return cached;
        float cap = src.rect.height * 0.5f;
        Sprite sliced = Sprite.Create(src.texture, src.rect, new Vector2(0.5f, 0.5f), src.pixelsPerUnit, 0,
            SpriteMeshType.FullRect, new Vector4(cap, 0f, cap, 0f));
        pillSlices[src] = sliced;
        return sliced;
    }

    /// <summary>Gán ảnh viên thuốc kéo giãn ngang cho Image có chiều cao height.</summary>
    public static void ApplyHorizontalPill(Image img, Sprite src, float height)
    {
        img.sprite = SliceHorizontalPill(src);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = src.rect.height / Mathf.Max(1f, height);
    }

    private static readonly Color SliderTrackColor = new Color(0.25f, 0.2f, 0.15f, 0.35f);
    private static readonly Color SliderFillColor = new Color(0.3f, 0.7f, 0.35f, 1f);

    /// <summary>
    /// Thanh kéo âm lượng dùng ảnh Art/UI/Setting (BackGroundBar / Fill / Knob) dựng vào root (giãn theo root, cao height);
    /// thiếu ảnh thì dùng khối màu. Dùng chung cho bảng Cài đặt ở menu và bảng tạm dừng trong trận.
    /// </summary>
    public static Slider CreateVolumeSlider(RectTransform root, float value, float height)
    {
        float knobSize = height * 1.55f;
        UISkin skin = UISkin.Instance;
        Image track = AddImage(root, SliderTrackColor);
        if (skin != null && skin.sliderTrack != null)
        {
            track.color = Color.white;
            ApplyHorizontalPill(track, skin.sliderTrack, height);
        }

        // Phần đầy nằm lọt trong lòng thanh nền
        float inset = height * 0.11f;
        RectTransform fillArea = CreateRect(root, "FillArea", Vector2.zero, Vector2.one, new Vector2(inset, inset), new Vector2(-inset, -inset));
        RectTransform fill = CreateRect(fillArea, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image fillImg = AddImage(fill, SliderFillColor, false);
        if (skin != null && skin.sliderFill != null)
        {
            fillImg.color = Color.white;
            ApplyHorizontalPill(fillImg, skin.sliderFill, height - inset * 2f);
        }

        RectTransform handleArea = CreateRect(root, "HandleArea", Vector2.zero, Vector2.one, new Vector2(knobSize * 0.4f, 0f), new Vector2(-knobSize * 0.4f, 0f));
        RectTransform handle = CreateRect(handleArea, "Handle", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
        handle.sizeDelta = new Vector2(knobSize, knobSize);
        Image handleImg = AddImage(handle, Color.white);
        Sprite knob = skin != null && skin.sliderKnob != null ? skin.sliderKnob : skin != null ? skin.levelBadge : null;
        if (knob != null)
        {
            handleImg.sprite = knob;
            handleImg.preserveAspect = true;
        }

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(value);
        return slider;
    }

    /// <summary>Hiệu ứng hiện ra: mờ -> rõ & phóng nhẹ (dùng thời gian thực).</summary>
    public static IEnumerator PopIn(CanvasGroup group, RectTransform target, float duration = 0.22f)
    {
        GameSfx.Play(GameSfx.Kind.PanelOpen);
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            float ease = 1f - Mathf.Pow(1f - k, 3f);
            group.alpha = k;
            target.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, ease);
            yield return null;
        }
        group.alpha = 1f;
        target.localScale = Vector3.one;
    }

    public static IEnumerator FadeOut(CanvasGroup group, float duration = 0.18f)
    {
        float start = group.alpha;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(start, 0f, t / duration);
            yield return null;
        }
        group.alpha = 0f;
    }
}
