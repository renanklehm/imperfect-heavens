using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GravityController : NetworkBehaviour
{
    [Networked] public float timeStamp { get; set; }

    public float timeWarp = 1;
    public SmoothCurve smoothCurve;

    [Range(1, 100)]
    public float maxTimestepMultiplier = 10f;
    [Range(0, 1)]
    public float dampFactor = 0.25f;

    public List<PhysicalBody> onRailsBodies;
    public List<PhysicalBody> freeBodies;
    public static GravityController Instance { get; set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Runner.Despawn(GetComponent<NetworkObject>());
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        smoothCurve = new SmoothCurve(maxTimestepMultiplier, dampFactor);
    }

    private void FixedUpdate()
    {
        timeStamp += Time.fixedDeltaTime * timeWarp;
    }

    public void RegisterBody(PhysicalBody body)
    {
        if (body.solverType == SolverType.OnRails)
        {
            onRailsBodies.Add(body);
        }
        else if (body.solverType == SolverType.FreeBody)
        {
            freeBodies.Add(body);
        }
    }

    public Vector3 GetNetForce(float smallBodyMass, Vector3 smallBodyPosition)
    {
        Vector3 netForce = Vector3.zero;
        foreach (PhysicalBody body in onRailsBodies)
        {
            netForce += CalculateForce(body, smallBodyMass, smallBodyPosition);
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
