using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WayPointManager : MonoBehaviour
{
    // Biến để lưu trữ các waypoint đã được sắp xếp
    private static List<Transform> sortedWaypoints;

    private void Awake()
    {
        // Gọi hàm để tìm và sắp xếp các waypoint ngay khi scene được tải
        FindAndSortWaypoints();
    }

    private void FindAndSortWaypoints()
    {
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("waypoint");

        if (waypointObjects.Length > 0)
        {
            var tempSortedWaypoints = waypointObjects.OrderBy(go => GetWaypointNumber(go.name));
            sortedWaypoints = tempSortedWaypoints.Select(go => go.transform).ToList();
        }
        else
        {
            Debug.LogError("Không tìm thấy các đối tượng với tag 'waypoint'!");
        }
    }

    private int GetWaypointNumber(string name)
    {
        string[] parts = name.Split(' ');
        if (parts.Length > 1 && int.TryParse(parts[1], out int number))
        {
            return number;
        }
        return -1;
    }

    // Phương thức công khai để các class khác có thể lấy danh sách waypoint
    public static Transform[] GetWaypoints()
    {
        return sortedWaypoints?.ToArray();
    }
}
