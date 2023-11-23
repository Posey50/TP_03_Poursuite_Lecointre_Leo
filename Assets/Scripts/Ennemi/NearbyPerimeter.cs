using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearbyPerimeter : MonoBehaviour
{
    public Transform player;
    EnemyBrainAStar enemyBrainAStar;

    private void Start()
    {
        enemyBrainAStar = GetComponentInParent<EnemyBrainAStar>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //If player enter in the nearby perimeter, it becomes the target
        if (other.CompareTag("Player"))
        {
            enemyBrainAStar.target = player;
        }
        else
        {
            return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //If player exit the nearby perimeter, ennemi recalculate a new path
        if (other.CompareTag("Player"))
        {
            enemyBrainAStar.StartCalculateTheNewNearestPath();
        }
        else
        {
            return;
        }
    }
}
