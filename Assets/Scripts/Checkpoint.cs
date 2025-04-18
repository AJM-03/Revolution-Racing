using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool keyCheckpoint;
    public int index;
    public GameObject nextWaypoints;
    public GameObject respawnPoints;
    public float turningMultiplier = 10;
    public bool finish;

    private void OnTriggerEnter(Collider other)
    {
        if(!finish && other.gameObject.GetComponent<CarCollider>() && other.gameObject.GetComponent<CarCollider>().car != null)  // If a car passes through
        {
            CarController car = other.gameObject.GetComponent<CarCollider>().car;
            if (keyCheckpoint && car.keyCheckpointIndex == index - 1)  // If it is a key checkpoint
            {
                car.keyCheckpointIndex = index;
            }
            else if (!keyCheckpoint)
                int.TryParse(transform.name, out car.checkpointIndex);

            car.respawnPoints = respawnPoints;

            car.nextCheckpoint = nextWaypoints.transform.parent;

            car.GetNextWaypoint(nextWaypoints, turningMultiplier / 10, true);
        }




        else if (other.gameObject.GetComponent<Missile>() && other.gameObject.GetComponent<Missile>().homing == true)  // If a homing missile passes through
        {
            Missile missile = other.gameObject.GetComponent<Missile>();
            missile.GetNextWaypoint(nextWaypoints, turningMultiplier / 10);
        }
    }
}
