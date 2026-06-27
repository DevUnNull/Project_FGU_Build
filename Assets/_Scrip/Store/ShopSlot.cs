using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public Image iconImage;
    private GameObject currentUnit; // instance trong slot

    // Set unit vào slot
    public void SetSlot(GameObject unitPrefab, Sprite icon)
    {
        currentUnit = Instantiate(unitPrefab, transform.position, Quaternion.identity, this.transform);
        iconImage.sprite = icon;

        // Nếu DragAndDrop chưa có thì thêm vào
        if (!currentUnit.TryGetComponent<DragAndDrop>(out _))
        {
            currentUnit.AddComponent<DragAndDrop>();
        }
    }

    // Kiểm tra unit đã mua chưa
    public bool HasBought()
    {
        if (currentUnit == null) return false;
        DragAndDrop drag = currentUnit.GetComponent<DragAndDrop>();
        return drag != null && drag.isBuy;
    }

    // Xoá unit trong slot
    public void ClearSlot()
    {
        if (currentUnit != null)
        {
            Destroy(currentUnit);
            currentUnit = null;
        }
    }
}
