using UnityEngine;

/// <summary>
/// Ảnh khung dùng cho UI tạo bằng code (asset đặt tại Resources/UISkin).
/// Sprite gốc KHÔNG bị sửa import: các sprite 9-slice được cắt lúc chạy từ texture gốc.
/// Toạ độ cắt/viền tính theo pixel của ảnh gốc (đo trên file PNG), tự quy đổi khi Unity thu nhỏ texture.
/// </summary>
[CreateAssetMenu(fileName = "UISkin", menuName = "Game/UI Skin")]
public class UISkin : ScriptableObject
{
    [Tooltip("CanhBao_0: băng cảnh báo có tam giác \"!\" bên trái (ảnh gốc 2172x724)")]
    public Sprite warningBanner;

    [Tooltip("countWave_0: khung viên thuốc nền tối viền xanh (ảnh gốc 963x339)")]
    public Sprite pillButton;

    [Tooltip("Ngôi sao đã đạt (sao của mỗi ngày, bảng Nâng cấp)")]
    public Sprite starFilled;

    [Tooltip("Ngôi sao chưa đạt")]
    public Sprite starEmpty;

    [Tooltip("Nền huy hiệu số cấp của tế bào đã ghép")]
    public Sprite levelBadge;

    [Header("Bảng Cài đặt (Art/UI/Setting)")]
    [Tooltip("Thanh nền của thanh kéo âm lượng (hình viên thuốc, kéo giãn ngang giữ nguyên 2 đầu tròn)")]
    public Sprite sliderTrack;
    [Tooltip("Phần đã đầy của thanh kéo")]
    public Sprite sliderFill;
    [Tooltip("Núm kéo")]
    public Sprite sliderKnob;
    [Tooltip("Biểu tượng âm lượng tổng")]
    public Sprite musicIcon;
    [Tooltip("Biểu tượng âm lượng hiệu ứng")]
    public Sprite sfxIcon;

    [Header("Biểu tượng bảng Nâng cấp")]
    public Sprite heartIcon;
    public Sprite atpIcon;

    // ---- Số đo ảnh gốc CanhBao.png ----
    private const float BannerOriginalWidth = 2172f;
    // Dải chính (bỏ khoảng trong suốt trên/dưới), toạ độ từ đáy ảnh: y 134..634
    private const float BandBottom = 134f;
    private const float BandHeight = 500f;
    // Viền 9-slice của băng: trái chứa trọn tam giác, phải chứa đầu bo tròn + nét lấp lánh
    public static readonly Vector4 BannerBorder = new Vector4(560f, 70f, 182f, 130f); // (trái, dưới, phải, trên)
    public static readonly Vector4 BannerContentInset = new Vector4(590f, 85f, 210f, 145f);
    // Nửa phải của thân băng (x 1100..2172) dùng ghép khung lớn (nửa trái là bản lật)
    private const float PanelCropX = 1100f;
    public static readonly Vector4 PanelBorder = new Vector4(0f, 70f, 182f, 130f);
    public static readonly Vector4 PanelContentInset = new Vector4(0f, 85f, 210f, 145f);

    // ---- Số đo ảnh gốc countWave_0 ----
    private const float PillOriginalWidth = 963f;
    public static readonly Vector4 PillBorder = new Vector4(175f, 60f, 175f, 60f);
    public const float PillOriginalHeight = 339f;

    private static UISkin instance;
    private static bool loaded;
    private static Sprite bannerSliced, panelSliced, pillSliced;

    public static UISkin Instance
    {
        get
        {
            if (!loaded)
            {
                instance = Resources.Load<UISkin>("UISkin");
                loaded = true;
            }
            return instance;
        }
    }

    /// <summary>Hệ số quy đổi pixel ảnh gốc -> pixel texture đã import (Unity có thể thu nhỏ texture lớn).</summary>
    public static float BannerScale => Instance != null && Instance.warningBanner != null
        ? Instance.warningBanner.rect.width / BannerOriginalWidth : 1f;

    public static float PillScale => Instance != null && Instance.pillButton != null
        ? Instance.pillButton.rect.width / PillOriginalWidth : 1f;

    public static Sprite BannerSprite => bannerSliced != null ? bannerSliced
        : bannerSliced = Slice(Instance?.warningBanner, BannerScale, 0f, BannerOriginalWidth, BannerBorder);

    public static Sprite PanelSprite => panelSliced != null ? panelSliced
        : panelSliced = Slice(Instance?.warningBanner, BannerScale, PanelCropX, BannerOriginalWidth - PanelCropX, PanelBorder);

    public static Sprite PillSprite
    {
        get
        {
            if (pillSliced != null || Instance == null || Instance.pillButton == null) return pillSliced;
            Sprite src = Instance.pillButton;
            float s = PillScale;
            pillSliced = Sprite.Create(src.texture, src.rect, new Vector2(0.5f, 0.5f), src.pixelsPerUnit, 0,
                SpriteMeshType.FullRect, PillBorder * s);
            return pillSliced;
        }
    }

    public static float BandOriginalHeight => BandHeight;

    private static Sprite Slice(Sprite src, float s, float cropX, float cropWidth, Vector4 border)
    {
        if (src == null) return null;
        Rect r = src.rect;
        Rect crop = new Rect(r.x + cropX * s, r.y + BandBottom * s, cropWidth * s, BandHeight * s);
        return Sprite.Create(src.texture, crop, new Vector2(0.5f, 0.5f), src.pixelsPerUnit, 0,
            SpriteMeshType.FullRect, border * s);
    }
}
