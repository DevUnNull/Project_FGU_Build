using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public WaypointPath assignedPath;  // Drag in Inspector
    // Hoặc auto: void Start() { assignedPath = PathManager.Instance.GetPath(PathID.Path1); }
}