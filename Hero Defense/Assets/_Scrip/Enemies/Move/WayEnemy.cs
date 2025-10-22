using UnityEngine;

public class WayEnemy : MonoBehaviour
{
    public WaypointMovement WaypointMovement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WaypointMovement = GetComponent<WaypointMovement>();
        WaypointMovement.waypoints = WayPointManager.GetWaypoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (WaypointMovement != null)
        {
            
            WaypointMovement.UpdateMovement();
        }
    }
}
