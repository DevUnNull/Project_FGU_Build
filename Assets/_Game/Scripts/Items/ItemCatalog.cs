using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tên, biểu tượng và mô tả của các vật phẩm lối sống. Đặt tại Resources/ItemCatalog.asset.
/// </summary>
[CreateAssetMenu(fileName = "ItemCatalog", menuName = "Game/Item Catalog")]
public class ItemCatalog : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public LifestyleItem type;
        public string displayName;
        public Sprite icon;
        [Tooltip("Mô tả tác dụng trong trận, hiện khi nhận vật phẩm")]
        [TextArea(2, 4)]
        public string effectText;
    }

    public List<Entry> items = new List<Entry>();

    [Tooltip("Nền tròn của ô vật phẩm trên thanh vật phẩm trong trận")]
    public Sprite slotBackground;

    private static ItemCatalog instance;
    private static bool loaded;

    public static ItemCatalog Instance
    {
        get
        {
            if (!loaded)
            {
                instance = Resources.Load<ItemCatalog>("ItemCatalog");
                loaded = true;
            }
            return instance;
        }
    }

    public Entry Get(LifestyleItem type) => items.Find(e => e.type == type);
}
