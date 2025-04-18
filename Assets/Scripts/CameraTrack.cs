using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraTrack : MonoBehaviour
{
    public Transform car, cameraPoint;
    public float cameraMovementAmmount;
    public CinemachineVirtualCamera cinemachine;
    public float zoomAmmount;


    void Update()
    {
        transform.position = cameraPoint.position + (car.position - cameraPoint.position) / cameraMovementAmmount;  // Moves the camera

        if (cinemachine != null)  // Camera Zooming
        {
            float distance = Vector3.Distance(car.position, cameraPoint.position);
            distance = distance / zoomAmmount;
            distance = 60 - distance;

            if (distance < 30)  // Min camera distance
                distance = 30;
            if (distance > 600)  // Max camera distance
                distance = 600;

            cinemachine.m_Lens.FieldOfView = distance;
        }
    }
}