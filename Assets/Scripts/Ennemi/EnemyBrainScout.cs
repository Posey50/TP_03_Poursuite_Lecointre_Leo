using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrainScout : MonoBehaviour
{
    public List<Transform> waypoints = new();
    public Transform cible;
    private int currentIndice;

    public float moveSpeed;
    public float rotationSpeed;

    void Start()
    {
        currentIndice = 0;
        cible = waypoints[currentIndice];
    }

    void FixedUpdate()
    {
        Move(cible);
        LookForward(cible);
    }

    private Transform NextWaypoint()
    {
        if (cible == waypoints[waypoints.Count - 1])
        {
            currentIndice = 0;
        }
        else
        {
            currentIndice++;
        }

        return waypoints[currentIndice];
    }

    private void Move(Transform _cible)
    {
        Vector3 direction = _cible.position - transform.position;

        if (direction.magnitude <= 0.1f)
        {
            cible = NextWaypoint();
        }

        Vector3 velocity = moveSpeed * Time.deltaTime * direction.normalized;
        transform.Translate(velocity, Space.World);
    }

    private void LookForward(Transform _cible)
    {
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
}
