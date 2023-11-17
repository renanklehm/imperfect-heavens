using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingBarycenter : MonoBehaviour
{
    public float timeWarp;
    public float orbitalPeriod;
    public Vector3 rotationAxis;

    private float radialSpeed;

    private void Update()
    {
        radialSpeed = 360f / orbitalPeriod;
        transform.Rotate(radialSpeed * Time.deltaTime * timeWarp * rotationAxis);
    }
}
