using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsParent : MonoBehaviour
{
    public Waypoint[] waypoints { get; private set; }

    private void Awake() 
    {
        waypoints = GetComponentsInChildren<Waypoint>();
    }
}
