using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrainAStar : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    private Transform lastNearestWaypointForPlayer;

    [Header("Waypoints")]
    public List<Transform> waypoints = new();
    private List<Transform> waypointsOpen = new();
    private List<Transform> pathToFollow = new();
    public Transform target;
    private int currentIndice;

    [Header("Ennemi")]
    public float moveSpeed;
    public float rotationSpeed;
    private bool canMove = false;

    [Header("UI")]
    public GameObject retryWindow;

    void Start()
    {
        pathToFollow.Clear();

        StartCalculateTheNewNearestPath();

        canMove = true;
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            Move(target);
            LookForward(target);
        }
    }

    private void Move(Transform _cible)
    {
        Vector3 direction = _cible.position - transform.position;

        //If ennemy reach a waypoint he check if the player has moved since the last verification
        if (direction.magnitude <= 0.1f)
        {
            //If it's different, it calculate a new path
            if (ClosestWaypoint(player) != lastNearestWaypointForPlayer)
            {
                StartCalculateTheNewNearestPath();
            }
            //Else it determine the new path to follow
            else
            {
                target = NextWaypoint();
            }
        }

        //Move to reach the next waypoint
        Vector3 velocity = moveSpeed * Time.deltaTime * direction.normalized;
        transform.Translate(velocity, Space.World);
    }

    private void LookForward(Transform _cible)
    {
        //Look in the direction of the actual target
        float rotation;

        if (MathHelper.AngleBetween(transform.forward, _cible.position - transform.position) < 1f)
        {
            rotation = 0f;
        }
        else
        {
            switch (MathHelper.CrossProduct(_cible.position - transform.position, transform.forward).y)
            {
                case > 0f:
                    {
                        rotation = -Mathf.Abs(rotationSpeed);
                        break;
                    }
                case < 0f:
                    {
                        rotation = Mathf.Abs(rotationSpeed);
                        break;
                    }
                default:
                    {
                        rotation = Mathf.Abs(rotationSpeed);
                        break;
                    }
            }
        }

        float angularSpeed = rotation * Time.deltaTime;

        transform.rotation *= Quaternion.AngleAxis(angularSpeed, Vector3.up);
    }

    private Transform NextWaypoint()
    {
        //If the waypoint is the last waypoint and if the player has moved, it calculate a new path
        if (target == pathToFollow[pathToFollow.Count - 1] && ClosestWaypoint(player) != lastNearestWaypointForPlayer)
        {
            StartCalculateTheNewNearestPath();

            return pathToFollow[currentIndice];
        }
        //If the waypoint is the last waypoint and if the player hasn't moved, the new target is the player
        else if (target == pathToFollow[pathToFollow.Count - 1] && ClosestWaypoint(player) == lastNearestWaypointForPlayer)
        {
            return player.transform;
        }
        //Else target is the next waypoint in the path
        else
        {
            currentIndice++;

            return pathToFollow[currentIndice];
        }
    }

    private Transform ClosestWaypoint(Transform _entity)
    {
        //Return the closest waypoint for an entity given
        List<float> waypointDistances = new();

        foreach (Transform waypoint in waypoints)
        {
            waypointDistances.Add(MathHelper.VectorDistance(_entity.position, waypoint.position));
        }

        //Determine the closest and direct waypoint
        int closestDistanceIndice = 0;

        for (int i = 1; i < waypointDistances.Count - 1; i++)
        {
            if (waypointDistances[i] < waypointDistances[closestDistanceIndice] && !ThereIsAWallBetween(_entity, waypoints[i]))
            {
                closestDistanceIndice = i;
            }
        }

        return waypoints[closestDistanceIndice];
    }

    private bool ThereIsAWallBetween(Transform _entity, Transform _waypoint)
    {
        //Check if there is a wall between entity and waypoint
        RaycastHit hit;
        float maxDistance = MathHelper.VectorDistance(_entity.position, _waypoint.position);

        if (Physics.Raycast(_entity.position, _waypoint.position - _entity.position, out hit, maxDistance))
        {
            return hit.transform.CompareTag("Wall");
        }
        else
        {
            return false;
        }
    }

    public void StartCalculateTheNewNearestPath()
    {
        //Reset current indice and the list of open waypoints
        currentIndice = 0;
        waypointsOpen.Clear();

        //Set the last closest waypoint for the player as the actual
        Transform ClosestWaypointForPlayer = ClosestWaypoint(player);
        lastNearestWaypointForPlayer = ClosestWaypointForPlayer;

        //Start calculate a new path between ennemi and player
        NewNearestPath(ClosestWaypoint(gameObject.GetComponent<Transform>()), ClosestWaypointForPlayer);

        target = pathToFollow[currentIndice];
    }

    private void NewNearestPath(Transform _departure, Transform _arrival)
    {
        //If the departure is not the arrival, close it and open it linked and open waypoints
        if (_departure != _arrival)
        {
            //Remove the departure from open waypoints because it is closed
            if (waypointsOpen.Contains(_departure))
            {
                waypointsOpen.Remove(_departure);
            }
            //Set the bool isOpen to false in the waypoint's script
            Waypoint departure = _departure.GetComponent<Waypoint>();
            departure.isOpen = false;

            //If the departure has waypoints next to it...
            if (departure.directWaypoints.Count > 0)
            {
                //...foreach waypoint...
                foreach (Transform waypoint in departure.directWaypoints)
                {
                    //...if it is open then add this waypoint to the open waypoints list, calcul it distance and assign it previous waypoint to access it
                    if (waypoint.GetComponent<Waypoint>().isOpen)
                    {
                        waypointsOpen.Add(waypoint);
                        CalculDistanceBetween(_departure, waypoint);
                        waypoint.GetComponent<Waypoint>().previousWaypoint = _departure;
                    }
                }

                //Finally, perform again this action with the closest open waypoint as a departure
                NewNearestPath(ClosestOpenedWaypoint(), _arrival);
            }
            else
            {
                //If the departure doesn't have waypoints next to it, perform again this action with the other closest open waypoint as a departure
                NewNearestPath(ClosestOpenedWaypoint(), _arrival);
            }
        }
        else
        {
            //Clear the path to follow
            pathToFollow.Clear();

            //Set the new path to follow
            pathToFollow.Add(_arrival);
            ReturnNearestPath(_arrival);
            pathToFollow.Reverse();

            //Reset all values in waypoints after the calcul
            ResetAllWaypoints();
        }
    }

    private void CalculDistanceBetween(Transform _departure, Transform _waypoint)
    {
        Waypoint waypoint = _waypoint.GetComponent<Waypoint>();

        //Calcul the distance as the crow flies between departure and an open waypoint
        float h = MathHelper.VectorDistance(_departure.position, _waypoint.position);

        //g is equal to the number of waypoints between a waypoint and the departure
        if (waypoint.previousWaypoint != null)
        {
            waypoint.stepForAccess = waypoint.previousWaypoint.GetComponent<Waypoint>().stepForAccess + 1;
        }
        else
        {
            waypoint.stepForAccess = 0;
        }
        int g = waypoint.stepForAccess;

        //f is equal to h + g and represent a score for distance between the departure and this waypoint
        waypoint.f = h + g;
    }

    private Transform ClosestOpenedWaypoint()
    {
        //Return the closest open waypoint depending of it "distance score" (f)
        if (waypointsOpen.Count > 0)
        {
            int closestDistanceIndice = 0;

            for (int i = 1; i < waypointsOpen.Count - 1; i++)
            {
                if (waypointsOpen[i].GetComponent<Waypoint>().f < waypointsOpen[closestDistanceIndice].GetComponent<Waypoint>().f)
                {
                    closestDistanceIndice = i;
                }
            }

            return waypointsOpen[closestDistanceIndice];
        }
        else
        {
            return waypointsOpen[0];
        }
    }

    private void ReturnNearestPath(Transform _waypoint)
    {
        //Go back up the nearest path
        Transform previousWaypoint = _waypoint.GetComponent<Waypoint>().previousWaypoint;

        if (previousWaypoint != null)
        {
            pathToFollow.Add(previousWaypoint);
            ReturnNearestPath(previousWaypoint);
        }
        else
        {
            return;
        }
    }

    private void ResetAllWaypoints()
    {
        //Reset all values in waypoints after the calcul
        foreach (Transform transformWaypoint in waypoints)
        {
            Waypoint waypoint = transformWaypoint.GetComponent<Waypoint>();

            waypoint.isOpen = true;
            waypoint.stepForAccess = 0;
            waypoint.f = 0f;
            waypoint.previousWaypoint = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Destroy player if ennemi touch it
        if (other.CompareTag("Player"))
        {
            canMove = false;
            Destroy(other.gameObject);
        }

        retryWindow.SetActive(true);
    }
}