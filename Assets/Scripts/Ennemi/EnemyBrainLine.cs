using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyBrainLine : MonoBehaviour
{
    public Transform cible;
    public float moveSpeed;
    public float rotationSpeed;

    void FixedUpdate()
    {
        Move();
        LookForward();
    }

    private void Move()
    {
        Vector3 direction = cible.position - transform.position;

        if (direction.magnitude <= 0.2f)
        {
            return;
        }

        Vector3 velocity = moveSpeed * Time.deltaTime * direction.normalized;
        transform.Translate(velocity, Space.World);
    }

    private void LookForward()
    {
        float rotation;

        if (MathHelper.AngleBetween(transform.forward, cible.position - transform.position) < 1f)
        {
            rotation = 0f;
        }
        else
        {
            switch (MathHelper.CrossProduct(cible.position - transform.position, transform.forward).y)
            {
                case > 0f:
                    {
                        rotation = - Mathf.Abs(rotationSpeed);
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