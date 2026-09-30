using System.Collections.Generic;
using UnityEngine;

public class ShopSpawner : MonoBehaviour
{
    public Transform[] slots;             // 4 vị trí tương ứng các ô
    public GameObject[] unitPrefabs;      // Các prefab tướng để random
    public int price;
    void Start()
    {
        SpawnUnitsInSlots();
        AutoSetupRollButton();
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

    // ✅ Hàm reset lại cửa hàng, nhưng giữ tướng đã mua
    public void ResetShop()
    {
        if (!GoldManager.Instance.HasEnoughGold(2))
        {
            Debug.Log("❌ Không đủ vàng để reset shop!");
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

        GoldManager.Instance.SpendGold(2);
        Debug.Log("🔁 Reset shop - Trừ 2 vàng");

        // Tự động tìm/gắn ReelController nếu chưa có
        SlotMachineReelController reelController = GetComponent<SlotMachineReelController>();
        if (reelController == null)
        {
            reelController = gameObject.AddComponent<SlotMachineReelController>();
        }

        // Đang quay reel thì bỏ qua không nhận click spam
        if (reelController.IsRolling) return;

        // Xoá tất cả tướng cũ chưa mua trước
        foreach (Transform slot in slots)
        {
            ClearUnboughtUnitsInSlot(slot);
        }

        // Chạy Reel Spin Slot Machine cho 4 ô (Dừng nối tiếp Trái -> Phải)
        reelController.StartReelRoll(slots, (slotIndex) =>
        {
            if (slotIndex >= 0 && slotIndex < slots.Length)
            {
                SpawnUnitInSlot(slots[slotIndex]);
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
