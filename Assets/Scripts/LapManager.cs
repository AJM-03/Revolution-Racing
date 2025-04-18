using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapManager : MonoBehaviour
{
    public List<Checkpoint> keyCheckpoints;
    public int totalLaps;
    public TMPro.TextMeshProUGUI lapCount;
    public RaceManager manager;
    public GameObject nextWaypoints;

    private void Start()
    {
        totalLaps = manager.trackProfile.laps;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<CarCollider>())
        {
            CarController car = other.gameObject.GetComponent<CarCollider>().car;
            if (car.keyCheckpointIndex == keyCheckpoints.Count)
            {
                if (car.lapNumber == totalLaps && manager.currentMode == RaceManager.Mode.Race)
                {
                    car.finished = true;
                    if (car.aiControlled == false)
                    {
                        car.aiControlled = true;
                        manager.FinishRace();
                        lapCount.transform.parent.gameObject.SetActive(false);
                    }
                }

                else
                {
                    car.keyCheckpointIndex = 0;
                    car.checkpointIndex = 0;
                    car.lapNumber++;

                    if (manager.currentMode == RaceManager.Mode.TimeTrials)
                        manager.TTLapComplete();

                    if (!car.aiControlled)
                    {
                        lapCount.text = car.lapNumber.ToString();
                        manager.lapTime = 0;
                        manager.NextLapMusic();
                    }
                }
            }

            else if (manager.currentMode == RaceManager.Mode.TimeTrials && manager.ttGhostRecord.enabled == false)
            {
                manager.ttGhostRecord.enabled = true;
                manager.TTSpawnCar();
            }

            car.GetNextWaypoint(nextWaypoints, 10, false);
        }
    }
}
