using System.Linq;
using UnityEngine;

public class WaypointPath : MonoBehaviour //tạo mới, thay WayPointManager
{
    [SerializeField] private PathID pathID;
    public PathID PathID => pathID;

    private Transform[] waypoints;

    void Awake()
    {
        // Auto-sort children waypoints như cũ
        waypoints = GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("waypoint"))
            .OrderBy(go => GetWaypointNumber(go.name))
            .ToArray();
    }

    private int GetWaypointNumber(string name)
    {
        if (name.Split(' ').Length > 1 && int.TryParse(name.Split(' ')[1], out int num))
            return num;
        return -1;
    }

    public Transform[] GetWaypoints() => waypoints;
}