using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Game/WaveConfig")]
public class WaveConfig : ScriptableObject
{
    [System.Serializable]
    public class SpawnGroup
    {
        public EnemyType type;
        public int count;
        public float interval = 1f;
    }

    public PathID pathID;
    public List<SpawnGroup> groups = new();  // Path n�y spawn c�c group n�y
}