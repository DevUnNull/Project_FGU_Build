using System.Collections.Generic;
using UnityEngine;

public class ShopSpawner : MonoBehaviour
{
    public Transform[] slots;             // 4 vị trí tương ứng các ô
    public GameObject[] unitPrefabs;      // Các prefab tướng để random
    public int price;
    private ShopSlotLocks locks;

    void Start()
    {
        SpawnUnitsInSlots();
        AutoSetupRollButton();

        locks = GetComponent<ShopSlotLocks>();
        if (locks == null) locks = gameObject.AddComponent<ShopSlotLocks>();
        GetReelController(); // ShopSlotLocks cần reel để biết lúc đang quay
        locks.Init(slots);
    }

    // Các ô không bị khóa: chỉ những ô này được xóa và quay lại khi Roll
    private Transform[] UnlockedSlots()
    {
        int count = 0;
        for (int i = 0; i < slots.Length; i++) if (!locks.IsLocked(i)) count++;
        Transform[] result = new Transform[count];
        for (int i = 0, n = 0; i < slots.Length; i++) if (!locks.IsLocked(i)) result[n++] = slots[i];
        return result;
    }

    // Hàm sinh tướng mới ban đầu
    public void SpawnUnitsInSlots()
    {
        foreach (Transform slot in slots)
        {
            if (slot.childCount == 0)
            {
                SpawnUnitInSlot(slot);
            }
        }
    }

    private void AutoSetupRollButton()
    {
        GameObject rollObj = GameObject.Find("Roll");
        if (rollObj != null)
        {
            RollButtonAnimation anim = rollObj.GetComponent<RollButtonAnimation>();
            if (anim == null)
            {
                anim = rollObj.AddComponent<RollButtonAnimation>();
            }

            UnityEngine.UI.Button btn = rollObj.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(ResetShop);
                btn.onClick.AddListener(ResetShop);
            }
        }
    }

    // Lần cuối bấm Roll (MergeHintUI dùng để biết người chơi đang đứng yên)
    public static float LastRollTime { get; private set; }

    // ===== Roll miễn phí mỗi khi sang wave mới =====
    private int lastRoundSeen;
    private bool freeRollPending;

    private void Update()
    {
        if (WaveManager.Instance == null) return;

        // Lượt chung (có quái) tăng lên -> từ wave thứ 2 trở đi được đổi khay miễn phí 1 lần
        int round = WaveManager.Instance.GetCurrentGlobalWaveIndex();
        if (round > lastRoundSeen)
        {
            if (lastRoundSeen >= 1) freeRollPending = true;
            lastRoundSeen = round;
        }

        // Chờ người chơi thả tế bào đang kéo từ khay (không xóa mất con họ đang cầm)
        bool draggingFromShop = DragAndDrop.Dragging != null && !DragAndDrop.Dragging.isBuy;
        if (freeRollPending && !draggingFromShop && Time.timeScale > 0f && !GetReelController().IsRolling)
        {
            freeRollPending = false;
            Roll(true);
        }
    }

    private SlotMachineReelController GetReelController()
    {
        SlotMachineReelController reelController = GetComponent<SlotMachineReelController>();
        if (reelController == null) reelController = gameObject.AddComponent<SlotMachineReelController>();
        return reelController;
    }

    // ✅ Hàm reset lại cửa hàng, nhưng giữ tướng đã mua (nút Roll, tốn 2 ATP)
    public void ResetShop()
    {
        Roll(false);
    }

    private void Roll(bool free)
    {
        SlotMachineReelController reelController = GetReelController();

        // Đang quay reel thì bỏ qua click spam (kiểm tra TRƯỚC khi trừ tiền để không mất ATP oan)
        if (reelController.IsRolling) return;

        Transform[] rollSlots = UnlockedSlots();
        if (rollSlots.Length == 0)
        {
            if (!free) BattleHUD.Toast("Cả 4 ô đang khóa! Bấm ổ khóa để mở bớt rồi mới Roll.", BattleHUD.WarningColor);
            return;
        }

        if (!free && !GoldManager.Instance.HasEnoughGold(2))
        {
            BattleHUD.Toast("Cần 2 ATP để đổi tế bào mới!", BattleHUD.WarningColor);
            return;
        }

        // 🎯 Phát hiệu ứng animation quay 360 + lắc lư cho nút Roll
        GameObject rollObj = GameObject.Find("Roll");
        if (rollObj != null)
        {
            RollButtonAnimation anim = rollObj.GetComponent<RollButtonAnimation>();
            if (anim != null)
            {
                anim.PlaySpinAnimation();
            }
        }

        if (free)
        {
            BattleHUD.Toast("Wave mới! Khay tế bào được đổi MIỄN PHÍ. Muốn đổi thêm thì bấm Roll (2 ATP).", BattleHUD.TipColor, "free_roll_hint");
        }
        else
        {
            GoldManager.Instance.SpendGold(2);
            Debug.Log("🔁 Reset shop - Trừ 2 vàng");
        }
        LastRollTime = Time.unscaledTime;
        GameSfx.Play(GameSfx.Kind.RollSpin);

        // Xoá tướng cũ chưa mua ở các ô không khóa (ô khóa giữ nguyên)
        foreach (Transform slot in rollSlots)
        {
            ClearUnboughtUnitsInSlot(slot);
        }

        // Chạy Reel Spin Slot Machine cho các ô không khóa (Dừng nối tiếp Trái -> Phải)
        reelController.StartReelRoll(rollSlots, (slotIndex) =>
        {
            if (slotIndex >= 0 && slotIndex < rollSlots.Length)
            {
                SpawnUnitInSlot(rollSlots[slotIndex]);
            }
        });
    }

    private void ClearUnboughtUnitsInSlot(Transform slot)
    {
        List<GameObject> toDestroy = new List<GameObject>();

        foreach (Transform child in slot)
        {
            DragAndDrop drag = child.GetComponent<DragAndDrop>();
            if (drag != null && !drag.isBuy)
            {
                toDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject obj in toDestroy)
        {
            obj.transform.SetParent(null); // Gỡ khỏi parent ngay lập tức để childCount của slot về 0
            Destroy(obj); // Xoá GameObject cũ
        }
    }

    private void SpawnUnitInSlot(Transform slot)
    {
        int rand = Random.Range(0, unitPrefabs.Length);
        GameObject unit = Instantiate(unitPrefabs[rand], slot.position, Quaternion.identity, slot);

        // Lấy giá từ PricePlayer gắn trên tướng
        _Hero priceData = unit.GetComponent<_Hero>();
        if (priceData != null)
        {
            price = priceData.price;
            Debug.Log($"💰 Giá tướng mới: {price}");

            // Tìm script hiển thị giá trên slot
            PricePlayerInSlot priceDisplay = slot.GetComponent<PricePlayerInSlot>();
            if (priceDisplay != null)
            {
                priceDisplay.SetPrice(price); // ✅ Gửi giá qua slot UI
            }
        }

        // Thêm hoặc lấy component DragAndDrop
        DragAndDrop dragComponent = unit.GetComponent<DragAndDrop>();
        if (dragComponent == null)
        {
            dragComponent = unit.AddComponent<DragAndDrop>();
        }
        dragComponent.isBuy = false;  // ✅ Đánh dấu unit chưa được mua
    }





}
