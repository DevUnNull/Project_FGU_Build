using UnityEditor;
using UnityEngine;

public class ShopSpawner : MonoBehaviour
{
    public Transform[] slots;             // 4 vị trí tương ứng các ô
    public GameObject[] unitPrefabs;      // Các prefab tướng để random
    public int price;
    void Start()
    {
        SpawnUnitsInSlots();
    }

    // Hàm sinh tướng mới ban đầu
    public void SpawnUnitsInSlots()
    {
        foreach (Transform slot in slots)
        {
            if (slot.childCount == 0)
            {
                SpawnUnitInSlot(slot); // ✅ dùng lại hàm chung
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

        GoldManager.Instance.SpendGold(2);
        Debug.Log("🔁 Reset shop - Trừ 2 vàng");

        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
            {
                Transform unit = slot.GetChild(0);
                DragAndDrop drag = unit.GetComponent<DragAndDrop>();

                if (drag != null && !drag.isBuy)
                {
                    Destroy(unit.gameObject);
                    SpawnUnitInSlot(slot); // ✅ gọi lại để tạo mới & cập nhật giá
                }
            }
            else
            {
                SpawnUnitInSlot(slot); // ✅ slot rỗng → tạo mới
            }
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
