using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BuildManager : MonoBehaviour
{
    public static BuildManager main;
    [Header("Reference")]
    //[SerializeField] private GameObject[] champPrefabs;
    [SerializeField] private Champion[] champs;
    private int selectedChamp = 0;
    private void Awake()
    {
        main = this;
    }
    public Champion GetSelectedChamp()
    {
        return champs[selectedChamp];
    }
    public void SetSelectedChamp(int _selectedChamp)
    {
        selectedChamp = _selectedChamp;
    }
    public void SetSelected()
    {

    }
}
