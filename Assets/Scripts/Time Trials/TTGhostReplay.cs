using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTGhostReplay : MonoBehaviour
{
    public TTGhost ghost;
    private float timeValue;
    private int index1;
    private int index2;
    public RaceManager raceManager;

    private void Awake()
    {
        timeValue = 0;
        StartCoroutine(CreateCar());
    }

    private void Update()
    {
        timeValue += Time.unscaledDeltaTime;

        if(ghost.isReplay)
        {
            GetIndex();
            SetTransform();
        }
    }


    private void GetIndex()
    {
        for (int i = 0; i < ghost.timeStamp.Count - 2; i++)
        {
            if(ghost.timeStamp[i] == timeValue)
            {
                index1 = i;
                index2 = i;
                return;
            }
            else if(ghost.timeStamp[i] < timeValue & timeValue < ghost.timeStamp[i + 1])
            {
                index1 = i;
                index2 = i + 1;
                return;
            }
        }

        index1 = ghost.timeStamp.Count - 1;
        index2 = ghost.timeStamp.Count - 1;
    }


    private void SetTransform()
    {
        if(index1 == index2)
        {
            this.transform.position = ghost.position[index1];
            this.transform.rotation = ghost.rotation[index1];
        }
        else
        {
            float interpolationFactor = (timeValue - ghost.timeStamp[index1])/ (ghost.timeStamp[index2] - ghost.timeStamp[index1]);
            
            this.transform.position = Vector3.Lerp(ghost.position[index1], ghost.position[index2], interpolationFactor);
            this.transform.rotation = Quaternion.Slerp(ghost.rotation[index1], ghost.rotation[index2], interpolationFactor);

        }
    }


    private IEnumerator CreateCar()
    {
        yield return new WaitForEndOfFrame();

        GameObject carToSpawn = ghost.car;
        GameObject spawnedcar = Instantiate(carToSpawn, transform.position, transform.rotation, transform);
        CarProfile profile = spawnedcar.GetComponent<CarProfile>();


        profile.carCollisionRB.gameObject.SetActive(false);
        profile.body.gameObject.layer = 10;
        if (profile.lfWheel != null)
            profile.lfWheel.gameObject.layer = 10;
        if (profile.rfWheel != null)
            profile.rfWheel.gameObject.layer = 10;
        if (profile.lbWheel != null)
            profile.lbWheel.gameObject.layer = 10;
        if (profile.rbWheel != null)
            profile.rbWheel.gameObject.layer = 10;
        if (profile.antenna != null)
            profile.antenna.gameObject.layer = 10;
        foreach (Transform child in profile.antenna.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = 10;
        }

        profile.skidMarkTrail.gameObject.SetActive(false);
        profile.airTrail.gameObject.SetActive(false);
        profile.offroadTrail.gameObject.SetActive(false);

        spawnedcar.transform.position += profile.spawnOffset;
    }
}
