using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GravityManager : NetworkBehaviour
{
    [Networked] 
    public float timestamp { get; set; }

    [Range(1, 100)]
    public float maxTimestepMultiplier = 10f;
    [Range(0, 1)]
    public float dampFactor = 0.25f;

    public float timeWarp = 1;
    public SmoothCurve dynamicTimestamp;
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
        dynamicTimestamp = new SmoothCurve(maxTimestepMultiplier, dampFactor);
    }

    private void FixedUpdate()
    {
        if (HasStateAuthority)
        {
            timestamp += Time.fixedDeltaTime * timeWarp;
        }
    }

    public void RegisterBody(Body body)
    {
        if (body.solverType == SolverType.OnRails)
        {
            onRailsBodies.Add(body);
            float totalMass = 0f;
            Vector3 tempCenterOfGravity = Vector3.zero;
            foreach(Body x in onRailsBodies)
            {
                totalMass += x.mass;
                tempCenterOfGravity += body.transform.position * body.mass;
            }
        }
        else if (body.solverType == SolverType.FreeBody)
        {
            freeBodies.Add(body);
        }
    }

    public Dictionary<MotionVector, Vector3> GetMotionVectors(Vector3 position, Vector3 velocity, Vector3 acceleration)
    {
        Dictionary<MotionVector, Vector3> result = new Dictionary<MotionVector, Vector3>();
        result.Add(MotionVector.Prograde, velocity.normalized);
        Vector3 radialOutPlaneNormal = Vector3.Cross(position - acceleration, result[MotionVector.Prograde]);
        result.Add(MotionVector.RadialOut, Vector3.Cross(result[MotionVector.Prograde], radialOutPlaneNormal).normalized);
        result.Add(MotionVector.Normal, Vector3.Cross(result[MotionVector.Prograde], result[MotionVector.RadialOut]).normalized);

        return result;
    }

    public Dictionary<MotionVector, Vector3> GetMotionVectors(Body body)
    {
        return GetMotionVectors(body.currentStateVector.position, body.currentStateVector.velocity, body.currentStateVector.gravityAcceleration);
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
        Vector3 _deltaDistance;
        if (timestamp == 0f)
        {
            _deltaDistance = B1.currentStateVector.position - smallBodyStateVector.position;
        }
        else if (timestamp == -1f)
        {
            _deltaDistance = B1.mainTrajectory.newestStateVector.position - smallBodyStateVector.position;
        }
        else
        {
            _deltaDistance = B1.mainTrajectory.Peek(timestamp).position - smallBodyStateVector.position;
        }
        
        Vector3 _direction = _deltaDistance.normalized;
        float _distance = _deltaDistance.magnitude * Constants.DISTANCE_FACTOR;
        
        Vector3 resultingForce =  Constants.GRAVITATIONAL_CONSTANT * B1.mass * smallBodyMass / Mathf.Pow(_distance, 2) * _direction;
        return resultingForce;
    }
}
