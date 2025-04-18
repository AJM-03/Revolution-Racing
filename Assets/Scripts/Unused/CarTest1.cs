using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTest1 : MonoBehaviour
{
    public float moveSpeed;  // Accelleration
    public float maxSpeed;  // Max Speed
    public float drag;  // Drag
    public float steerAngle;  // Handling
    public float traction = 1;  // Traction

    private Vector3 moveForce;  // Speed


    void Update()
    {
        // --- Accellerate --- //
        moveForce += transform.forward * moveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
        transform.position += moveForce * Time.deltaTime;


        // --- Steering --- //
        float steerInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * steerInput * moveForce.magnitude * steerAngle * Time.deltaTime);


        // --- Drag --- //
        moveForce *= drag;
        moveForce = Vector3.ClampMagnitude(moveForce, maxSpeed);


        // --- Traction --- //
        Debug.DrawRay(transform.position, moveForce.normalized * 3);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.blue);
        moveForce = Vector3.Lerp(moveForce.normalized, transform.forward, traction * Time.deltaTime) * moveForce.magnitude;
    }
}
