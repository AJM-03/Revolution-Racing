using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTest3 : MonoBehaviour
{
    public Rigidbody sphereRB;
    public Rigidbody carColRB;
    private float moveInput;
    private float turnInput;
    public float accelSpeed;
    public float revSpeed;
    public float turnSpeed;
    private bool grounded;
    public LayerMask groundLayer;
    public float airDrag;
    public float groundDrag;


    void Start()
    {
        sphereRB.transform.parent = null;
        carColRB.transform.parent = null;
    }

    
    void Update()
    {
        moveInput = Input.GetAxisRaw("Vertical");
        turnInput = Input.GetAxisRaw("Horizontal");


        moveInput *= moveInput > 0 ? accelSpeed : revSpeed;

        transform.position = sphereRB.transform.position;

        float newRotation = turnInput * turnSpeed * Time.deltaTime * Input.GetAxisRaw("Vertical");
        transform.Rotate(0, newRotation, 0, Space.World);

        RaycastHit hit;
        grounded = Physics.Raycast(transform.position, -transform.up, out hit, groundLayer);

        transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        if (grounded)
            sphereRB.drag = groundDrag;
        else
            sphereRB.drag = airDrag;
    }


    private void FixedUpdate()
    {
        if (grounded)
            sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        else
            sphereRB.AddForce(transform.up * -38f);

        carColRB.MoveRotation(transform.rotation);
    }
}
