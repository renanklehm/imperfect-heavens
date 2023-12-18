using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DEBUGER : MonoBehaviour
{
    public float magnitude;

    void Update()
    {
        Debug.Log(DynamicTimestep.Evaluate(magnitude));
    }
}
