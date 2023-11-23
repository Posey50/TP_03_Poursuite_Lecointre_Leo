using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public List<Transform> directWaypoints;
    public Transform previousWaypoint;
    public int stepForAccess;
    public float f;

    public bool isOpen;

    private void Start()
    {
        isOpen = true;
        stepForAccess = 0;
    }
}
