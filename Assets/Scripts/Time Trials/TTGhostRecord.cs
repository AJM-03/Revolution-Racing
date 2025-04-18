using UnityEngine;

public class TTGhostRecord : MonoBehaviour
{
    public TTGhost ghost;
    public float timer;
    public float timeValue;


    private void Awake()
    {
        if(ghost.isRecord)
        {
            ghost.Clear();
            timeValue = 0;
            timer = 0;
        }
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        timeValue += Time.unscaledDeltaTime;

        if(ghost.isRecord & timer >= 1/ghost.recordFrequency)
        {
            ghost.timeStamp.Add(timeValue);
            ghost.position.Add(this.transform.position);
            ghost.rotation.Add(this.transform.rotation);

            timer = 0;
        }
    }

    public void SaveGhost(TTGhost saveTo, float newLapTime)
    {
        saveTo.Clear();

        saveTo.car = gameObject.GetComponent<CarController>().spawnedCarPrefab;
        saveTo.lapTime = newLapTime;

        for (int i = 0; i < ghost.timeStamp.Count; i++)
        {
            saveTo.timeStamp.Add(ghost.timeStamp[i]);
        }

        for (int i = 0; i < ghost.position.Count; i++)
        {
            saveTo.position.Add(ghost.position[i]);
        }

        for (int i = 0; i < ghost.rotation.Count; i++)
        {
            saveTo.rotation.Add(ghost.rotation[i]);
        }

        saveTo.timeStamp.Add(timeValue);
        saveTo.position.Add(this.transform.position);
        saveTo.rotation.Add(this.transform.rotation);
    }
}
