using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour // thay WayPointManager static
{
    public static PathManager Instance { get; private set; }

    private Dictionary<PathID, WaypointPath> paths = new();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // Find all paths
        WaypointPath[] allPaths = FindObjectsOfType<WaypointPath>();
        foreach (var path in allPaths)
            paths[path.PathID] = path;
    }

    public WaypointPath GetPath(PathID id) => paths.ContainsKey(id) ? paths[id] : null;
}