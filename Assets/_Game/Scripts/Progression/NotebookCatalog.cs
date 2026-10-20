using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nội dung trang Tế bào / Mầm bệnh của Sổ tay sức khỏe. Đặt tại Resources/NotebookCatalog.asset.
/// assetName = tên asset HeroData / EnemyData (VD: Cell_HongCau, VirusCumA).
/// </summary>
[CreateAssetMenu(fileName = "NotebookCatalog", menuName = "Game/Notebook Catalog")]
public class NotebookCatalog : ScriptableObject
{
    [System.Serializable]
    public class UnitEntry
    {
        [Tooltip("Tên asset HeroData / EnemyData")]
        public string assetName;
        public string displayName;
        public Sprite icon;
        [TextArea(2, 6)]
        public string description;
    }

    public List<UnitEntry> heroes = new List<UnitEntry>();
    public List<UnitEntry> enemies = new List<UnitEntry>();

    private static NotebookCatalog instance;
    private static bool loaded;

    public static NotebookCatalog Instance
    {
        get
        {
            if (!loaded)
            {
                instance = Resources.Load<NotebookCatalog>("NotebookCatalog");
                loaded = true;
            }
            return instance;
        }
    }

    public UnitEntry FindHero(string assetName) => heroes.Find(e => e.assetName == assetName);
    public UnitEntry FindEnemy(string assetName) => enemies.Find(e => e.assetName == assetName);
}
