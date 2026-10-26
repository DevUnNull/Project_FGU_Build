using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nút ổ khóa ở góc trên-trái mỗi ô khay tế bào. Ô đã khóa giữ nguyên tế bào khi Roll (kể cả Roll miễn phí đầu wave),
/// để người chơi để dành con đang chờ ghép mà vẫn đổi được các ô còn lại.
/// Ô tự mở khóa khi tế bào trong đó đã được mua đi. ShopSpawner tự gắn component này lúc chạy.
/// </summary>
public class ShopSlotLocks : MonoBehaviour
{
    // Nút đặt phía trên vùng va chạm của tế bào (collider ~0.8 đơn vị) để không bắt nhầm cú kéo tế bào
    private const float ButtonWorldSize = 0.34f;
    private const int SortingOrder = 15; // tế bào = 10, quái = 12
    private static readonly Vector2 ButtonInset = new Vector2(0.22f, 0.16f); // tính từ góc trên-trái ô

    private static readonly Color OpenBadge = new Color(0f, 0f, 0f, 0.45f);
    private static readonly Color OpenIcon = new Color(1f, 1f, 1f, 0.85f);
    private static readonly Color LockedBadge = new Color(1f, 0.78f, 0.2f, 1f);
    private static readonly Color LockedIcon = new Color(0.35f, 0.2f, 0.05f, 1f);
    private static readonly Color FrameColor = new Color(1f, 0.82f, 0.25f, 0.95f);

    private static Sprite circleSprite, openSprite, closedSprite, frameSprite;

    private Transform[] slots;
    private bool[] locked;
    private RectTransform[] buttons;
    private Image[] badges;
    private Image[] icons;
    private RectTransform[] frames;
    private SlotMachineReelController reel;

    public void Init(Transform[] shopSlots)
    {
        slots = shopSlots;
        if (slots == null || slots.Length == 0) return;
        reel = GetComponent<SlotMachineReelController>();

        Canvas canvas = slots[0].GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Khung chứa riêng có GraphicRaycaster (Canvas nền chưa nhận click UI),
        // vẽ trên sprite tế bào (order 0) nhưng vẫn dưới CanvasUi (bảng tạm dừng, thắng/thua)
        RectTransform root = RuntimeUI.Stretch(canvas.transform, "ShopSlotLocks");
        Canvas lockCanvas = root.gameObject.AddComponent<Canvas>();
        lockCanvas.overrideSorting = true;
        lockCanvas.sortingLayerID = canvas.sortingLayerID;
        lockCanvas.sortingOrder = SortingOrder;
        root.gameObject.AddComponent<GraphicRaycaster>();

        locked = new bool[slots.Length];
        buttons = new RectTransform[slots.Length];
        badges = new Image[slots.Length];
        icons = new Image[slots.Length];
        frames = new RectTransform[slots.Length];
        for (int i = 0; i < slots.Length; i++) BuildSlot(root, i);
        Refresh();
    }

    public bool IsLocked(int index) => locked != null && index >= 0 && index < locked.Length && locked[index];

    private void BuildSlot(RectTransform root, int index)
    {
        // Viền vàng quanh ô đang khóa
        RectTransform frame = RuntimeUI.CreateRect(root, "LockFrame" + index, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        Image frameImg = RuntimeUI.AddImage(frame, FrameColor, false);
        frameImg.sprite = FrameSprite;
        frameImg.type = Image.Type.Sliced;
        frames[index] = frame;

        RectTransform button = RuntimeUI.CreateRect(root, "Lock" + index, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        badges[index] = RuntimeUI.AddImage(button, OpenBadge);
        badges[index].sprite = CircleSprite;
        RectTransform iconRt = RuntimeUI.CreateRect(button, "Icon", new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.85f), Vector2.zero, Vector2.zero);
        icons[index] = RuntimeUI.AddImage(iconRt, OpenIcon, false);
        icons[index].preserveAspect = true;
        Button btn = button.gameObject.AddComponent<Button>();
        btn.targetGraphic = badges[index];
        btn.onClick.AddListener(() => Toggle(index));
        buttons[index] = button;
    }

    private void Toggle(int index)
    {
        if (reel != null && reel.IsRolling) return;
        if (!locked[index] && !HasUnboughtUnit(slots[index]))
        {
            BattleHUD.Toast("Ô này đang trống, chưa có tế bào để giữ.", BattleHUD.WarningColor);
            return;
        }

        locked[index] = !locked[index];
        GameSfx.Play(locked[index] ? GameSfx.Kind.RollLock : GameSfx.Kind.UIClick);
        if (locked[index])
            BattleHUD.Toast("Đã khóa ô: Roll sẽ giữ nguyên tế bào này. Bấm ổ khóa lần nữa để mở.", BattleHUD.TipColor, "shop_lock_hint");
        StartCoroutine(Bounce(buttons[index]));
        Refresh();
    }

    private static System.Collections.IEnumerator Bounce(RectTransform target)
    {
        const float duration = 0.2f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.one * (1f + Mathf.Sin(t / duration * Mathf.PI) * 0.25f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    private void LateUpdate()
    {
        if (slots == null || locked == null) return;

        // Tế bào trong ô khóa đã được mua đi -> ô mở khóa để lần Roll sau có hàng mới
        bool changed = false;
        for (int i = 0; i < slots.Length; i++)
        {
            if (locked[i] && !HasUnboughtUnit(slots[i]))
            {
                locked[i] = false;
                changed = true;
            }
        }
        if (changed) Refresh();

        // Ô đang quay bị co giãn/dịch chuyển: giữ chỗ cũ tới khi dừng
        if (reel != null && reel.IsRolling) return;
        for (int i = 0; i < slots.Length; i++) Place(i);
    }

    private void Place(int index)
    {
        Transform slot = slots[index];
        if (slot == null) return;

        SpriteRenderer sr = slot.GetComponent<SpriteRenderer>();
        Bounds bounds = sr != null ? sr.bounds : new Bounds(slot.position, new Vector3(1.2f, 1.3f, 0f));
        float unitsPerWorld = 1f / Mathf.Max(0.0001f, buttons[index].parent.lossyScale.x);
        float z = buttons[index].parent.position.z;

        buttons[index].position = new Vector3(bounds.min.x + ButtonInset.x, bounds.max.y - ButtonInset.y, z);
        buttons[index].sizeDelta = Vector2.one * ButtonWorldSize * unitsPerWorld;

        frames[index].position = new Vector3(bounds.center.x, bounds.center.y, z);
        frames[index].sizeDelta = new Vector2(bounds.size.x, bounds.size.y) * unitsPerWorld;
    }

    private void Refresh()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            badges[i].color = locked[i] ? LockedBadge : OpenBadge;
            icons[i].color = locked[i] ? LockedIcon : OpenIcon;
            icons[i].sprite = locked[i] ? ClosedSprite : OpenSprite;
            frames[i].gameObject.SetActive(locked[i]);
        }
    }

    private static bool HasUnboughtUnit(Transform slot)
    {
        if (slot == null) return false;
        foreach (Transform child in slot)
        {
            DragAndDrop drag = child.GetComponent<DragAndDrop>();
            if (drag != null && !drag.isBuy) return true;
        }
        return false;
    }

    // ===== Hình vẽ bằng code (project chưa có ảnh ổ khóa) =====

    private static Sprite CircleSprite => circleSprite != null ? circleSprite : circleSprite = Draw(p => p.magnitude - 30f);
    private static Sprite ClosedSprite => closedSprite != null ? closedSprite : closedSprite = Draw(p => Padlock(p, false));
    private static Sprite OpenSprite => openSprite != null ? openSprite : openSprite = Draw(p => Padlock(p, true));

    private static Sprite FrameSprite
    {
        get
        {
            if (frameSprite != null) return frameSprite;
            // Viền bo góc 9-slice: dày 5px, bo 12px
            frameSprite = Draw(p => Mathf.Abs(RoundedBox(p, new Vector2(29f, 29f), 12f)) - 2.5f, new Vector4(20f, 20f, 20f, 20f));
            return frameSprite;
        }
    }

    // Ổ khóa: thân bo góc + quai hình chữ U (mở thì quai nhấc lên, chân phải tách khỏi thân) + lỗ khóa khoét rỗng
    private static float Padlock(Vector2 p, bool open)
    {
        float body = RoundedBox(p - new Vector2(0f, -10f), new Vector2(17f, 13f), 5f);

        float lift = open ? 8f : 0f;
        Vector2 c = new Vector2(0f, 6f + lift);
        Vector2 q = p - c;
        float arc = q.y >= 0f ? Mathf.Abs(q.magnitude - 11f) - 3f : 999f;
        float leftLeg = Box(p - new Vector2(-11f, (c.y + 2f) * 0.5f), new Vector2(3f, (c.y - 2f) * 0.5f + 0.01f));
        float rightBottom = open ? c.y - 4f : 2f;
        float rightLeg = Box(p - new Vector2(11f, (c.y + rightBottom) * 0.5f), new Vector2(3f, (c.y - rightBottom) * 0.5f + 0.01f));
        float shape = Mathf.Min(body, Mathf.Min(arc, Mathf.Min(leftLeg, rightLeg)));

        float keyhole = Mathf.Min((p - new Vector2(0f, -7f)).magnitude - 3.5f, Box(p - new Vector2(0f, -13f), new Vector2(1.6f, 5f)));
        return Mathf.Max(shape, -keyhole);
    }

    private static float Box(Vector2 p, Vector2 half)
    {
        Vector2 d = new Vector2(Mathf.Abs(p.x) - half.x, Mathf.Abs(p.y) - half.y);
        return new Vector2(Mathf.Max(d.x, 0f), Mathf.Max(d.y, 0f)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0f);
    }

    private static float RoundedBox(Vector2 p, Vector2 half, float radius) => Box(p, half - Vector2.one * radius) - radius;

    // Vẽ hình 64x64 từ hàm khoảng cách (âm = bên trong), khử răng cưa 1px; màu trắng để Image tô màu
    private static Sprite Draw(System.Func<Vector2, float> distance, Vector4 border = default)
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f - size * 0.5f, y + 0.5f - size * 0.5f);
                byte a = (byte)(Mathf.Clamp01(0.5f - distance(p)) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, a);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }
}
