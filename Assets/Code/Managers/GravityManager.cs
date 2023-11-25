using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GravityManager : NetworkBehaviour
{
    [Networked] 
    public float timeStamp { get; set; }

    [Range(1, 100)]
    public float maxTimestepMultiplier = 10f;
    [Range(0, 1)]
    public float dampFactor = 0.25f;


    public float timeWarp = 1;
    public SmoothCurve smoothCurve;
    public List<Body> onRailsBodies;
    public List<Body> freeBodies;
    public static GravityManager Instance { get; set; }
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
        smoothCurve = new SmoothCurve(maxTimestepMultiplier, dampFactor);
    }

    private void FixedUpdate()
    {
        if (HasStateAuthority)
        {
            timeStamp += Time.fixedDeltaTime * timeWarp;
        }
    }

    public void RegisterBody(Body body)
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

    public Vector3 GetNetForce(StateVector smallBodyStateVector, float smallBodyMass, float timestamp)
    {
        Vector3 netForce = Vector3.zero;
        foreach (Body body in onRailsBodies)
        {
            netForce += CalculateForce(body, smallBodyStateVector, smallBodyMass, timestamp);
        }
        return netForce;
    }

    public static Vector3 CalculateForce(Body B1, StateVector smallBodyStateVector, float smallBodyMass, float timestamp = 0f)
    {
        Vector3 _deltaDistance = Vector3.zero;
        if (timestamp == 0f)
        {
            _deltaDistance = B1.currentStateVector.position - smallBodyStateVector.position;
        }
        else if (timestamp == -1f)
        {
            _deltaDistance = B1.trajectory.newestStateVector.position - smallBodyStateVector.position;
        }
        else
        {
            _deltaDistance = B1.trajectory.Peek(timestamp).position - smallBodyStateVector.position;
        }
        
        Vector3 _direction = _deltaDistance.normalized;
        float _distance = _deltaDistance.magnitude * Constants.DISTANCE_FACTOR;
        
        Vector3 resultingForce =  Constants.GRAVITATIONAL_CONSTANT * B1.mass * smallBodyMass / Mathf.Pow(_distance, 2) * _direction;
        return resultingForce;
    }
}
