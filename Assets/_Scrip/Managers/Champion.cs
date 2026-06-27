using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[Serializable]
public class Champion
{
    public string champName;
    public GameObject champPrefab;
    public int cost;

    public Champion(string name, GameObject prefab, int cost)
    {
        this.champName = name;
        this.champPrefab = prefab;
        this.cost = cost;
    }

}
