using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour // thay WayPointManager static
{
    public static PathManager Instance { get; private set; }

    private Dictionary<PathID, WaypointPath> paths = new();

    void Awake()
    {
        Instance = this;

        // Find all paths in current scene
        paths.Clear();
        WaypointPath[] allPaths = FindObjectsOfType<WaypointPath>();
        foreach (var path in allPaths)
        {
            if (path != null)
                paths[path.PathID] = path;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public WaypointPath GetPath(PathID id) => (paths != null && paths.ContainsKey(id)) ? paths[id] : null;
}