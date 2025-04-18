using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinObject : MonoBehaviour
{
    public Vector3 spinAmmount;

    void Update()
    {
        transform.Rotate(spinAmmount);
    }
}
