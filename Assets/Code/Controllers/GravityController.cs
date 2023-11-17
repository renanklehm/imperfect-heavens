using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityController : MonoBehaviour
{
    public float timeWarp;
    public float timeStamp;
    public SmoothCurve smoothCurve;
    public float maxTimestepMultiplier;
    public float dampFactor;

    private List<OnRailsBody> onRailsBodies;
    private List<FreeBody> freeBodies;
    public static GravityController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        onRailsBodies = new List<OnRailsBody>();
        foreach (OnRailsBody body in FindObjectsOfType<OnRailsBody>())
        {
            onRailsBodies.Add(body);
        }

        freeBodies = new List<FreeBody>();
        foreach (FreeBody body in FindObjectsOfType<FreeBody>())
        {
            freeBodies.Add(body);
        }

        smoothCurve = new SmoothCurve(maxTimestepMultiplier, dampFactor);
    }

    private void FixedUpdate()
    {
        timeStamp += Time.fixedDeltaTime * timeWarp;
    }

    public Vector3 GetNetForce(float smallBodyMass, Vector3 smallBodyPosition)
    {
        Vector3 netForce = Vector3.zero;
        foreach (OnRailsBody onRailBody in onRailsBodies)
        {
            netForce += CalculateForce(onRailBody.body, smallBodyMass, smallBodyPosition);
        }
        return netForce;
    }

    public static Vector3 CalculateForce(PhysicalBody B1, float smallBodyMass, Vector3 smallBodyPosition)
    {

        Vector3 _deltaDistance = B1.transform.position - smallBodyPosition;
        Vector3 _direction = _deltaDistance.normalized;
        float _distance = _deltaDistance.magnitude * Constants.DISTANCE_FACTOR;
        

        Vector3 resultingForce =  Constants.GRAVITATIONAL_CONSTANT * B1.mass * smallBodyMass / Mathf.Pow(_distance, 2) * _direction;
        return resultingForce;
    }
}
