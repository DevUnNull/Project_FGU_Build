using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TuneConfig", menuName = "Game/TuneConfig")]
public class TuneConfig : ScriptableObject
{
    public List<WaveConfig> waves = new();
}