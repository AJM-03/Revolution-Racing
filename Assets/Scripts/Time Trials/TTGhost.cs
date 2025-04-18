using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TTGhost : ScriptableObject
{
    public bool isRecord;
    public bool isReplay;
    public float recordFrequency;
    public GameObject car;
    public float lapTime;

    public List<float> timeStamp;
    public List<Vector3> position;
    public List<Quaternion> rotation;

    public void Clear()
    {
        timeStamp.Clear();
        position.Clear();
        rotation.Clear();
    }
}
