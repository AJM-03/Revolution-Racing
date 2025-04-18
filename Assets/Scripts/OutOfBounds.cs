using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    // Custom particles/sounds?

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CarCollider>() && other.gameObject.GetComponent<CarCollider>().car != null)
        {
            CarController car = other.gameObject.GetComponent<CarCollider>().car;
            StartCoroutine(car.RespawnCar());
        }
    }
}
