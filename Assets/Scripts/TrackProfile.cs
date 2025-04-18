using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackProfile : MonoBehaviour
{
    public int laps;

    public AudioClip lap1Music, lap2Music;
    public float musicVolume;
    public float musicIntroTime;

    [Header("Time Trials")]
    public TTGhost trackGhost;
    public GameObject ttObjects;


    [Header("Cars")]
    public GameObject[] cars;
    [HideInInspector] public CarController playerCarController, car1Controller, car2Controller, car3Controller, car4Controller, car5Controller;


    [Header("Modes")]
    public GameObject raceObjects;
    public GameObject timeTrialObjects;

    private void Start()
    {
        playerCarController = cars[0].GetComponent<CarController>();
        car1Controller = cars[1].GetComponent<CarController>();
        car2Controller = cars[2].GetComponent<CarController>();
        car3Controller = cars[3].GetComponent<CarController>();
        car4Controller = cars[4].GetComponent<CarController>();
        car5Controller = cars[5].GetComponent<CarController>();
    }
}
