using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerBehaviour : MonoBehaviour
{
    public float sizeFactor;

    void Start()
    {
        
    }


    void Update()
    {
        float newSize = Solver.GetHandleSize( transform.position, Camera.main, sizeFactor);
        transform.localScale = Vector3.one * newSize;
    }
}
