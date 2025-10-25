using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PoolConfig", menuName = "Game/PoolConfig")]
public class PoolConfig : ScriptableObject
{
    [System.Serializable]
    public class PoolEntry
    {
        public EnemyType type;
        public GameObject prefab;
        public int initialSize = 5;
    }

    public List<PoolEntry> pools = new();
}