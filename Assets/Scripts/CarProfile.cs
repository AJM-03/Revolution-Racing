using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarProfile : MonoBehaviour
{
    [Header("Variants")]
    public GameObject[] variants;
    public Color32 variantColour;
    public int variantNumber;

    [Header("Stats")]
    public float forwardSpeed;      // 8 - 12 [5-15]   (Lower is slower)
    public float reverseSpeed;      // 3 - 10          (Lower is slower)
    public float accelerationTime;  // 5 - 15          (Lower is faster)
    public float handling;          // 3 - 5           (Lower is slower)
    public float offroad;           // 5 - 15          (Lower is slower)


    [Header("Collision")]
    public Rigidbody carCollisionRB;


    [Header("Wheels")]
    public Transform lfWheel, rfWheel, lbWheel, rbWheel;
    public float maxWheelTurn;
    public bool flipLWheel, flipRWheel, dontTurnWheels;


    [Header("Effects")]
    public AudioClip motorSounds;


    [Header("Trails")]
    public TrailShow skidMarkTrail, airTrail, offroadTrail;


    [Header("Effects")]
    public GameObject[] boostFireEffect;


    [Header("Spawn Offset")]
    public Vector3 spawnOffset;

    [Header("Time Trials")]
    public GameObject body;
    public GameObject antenna;
}
