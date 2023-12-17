using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct StateVector : INetworkStruct
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector3 velocity;
    [SerializeField] public Vector3 acceleration;
    public Vector3 prograde;
    public Vector3 radialOut;
    public Vector3 normal;
    public Vector3 gravityAcceleration;
    public Vector3 externalAcceleration;
    public float timestamp;

    public StateVector(Vector3 _position = new Vector3())
    {
        position = _position;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        externalAcceleration = Vector3.zero;
        gravityAcceleration = Vector3.zero;
        prograde = Vector3.forward;
        radialOut = Vector3.right;
        normal = Vector3.up;
        timestamp = 0f;
    }

    public StateVector(
        Vector3 position,
        Vector3 velocity,
        Vector3 acceleration,
        Vector3 prograde,
        Vector3 radialOut,
        Vector3 gravityAcceleration,
        float timestamp
        )
    {
        this.position = position;
        this.velocity = velocity;
        this.acceleration = acceleration;
        this.prograde = prograde.normalized;
        this.radialOut = radialOut.normalized;
        this.normal = Vector3.Cross(prograde, radialOut);
        this.timestamp = timestamp;
        this.gravityAcceleration = gravityAcceleration;
        this.externalAcceleration = acceleration - gravityAcceleration;
    }
    public StateVector(StateVector source)
    {
        position = source.position;
        velocity = source.velocity;
        acceleration = source.acceleration;
        timestamp = source.timestamp;
        gravityAcceleration = source.gravityAcceleration;
        externalAcceleration = source.externalAcceleration;
        prograde = source.prograde.normalized;
        radialOut = source.radialOut.normalized;
        normal = source.normal;
    }

    public static float ScoreDifference(StateVector a,StateVector b)
    {
        float score = 0;
        score += (a.position - b.position).sqrMagnitude;
        score += (a.velocity - b.velocity).sqrMagnitude;
        score += (a.acceleration - b.acceleration).sqrMagnitude;
        return score;
    }

    public static StateVector LerpVector(StateVector a, StateVector b, float factor)
    {
        Vector3 _position = Vector3.Lerp(a.position, b.position, factor);
        Vector3 _velocity = Vector3.Lerp(a.velocity, b.velocity, factor);
        Vector3 _acceleration = Vector3.Lerp(a.acceleration, b.acceleration, factor);
        Vector3 _prograde = Vector3.Lerp(a.prograde, b.prograde, factor).normalized;
        Vector3 _radialOut = Vector3.Lerp(a.radialOut, b.radialOut, factor).normalized;
        Vector3 _gravityAcceleration = Vector3.Lerp(a.gravityAcceleration, b.gravityAcceleration, factor);
        float _timestamp = Mathf.Lerp(a.timestamp, b.timestamp, factor);

        return new StateVector(_position, _velocity, _acceleration, _prograde, _radialOut, _gravityAcceleration, _timestamp);
    }

    public static float deltaPosition(StateVector a, StateVector b)
    {
        return (a.position - b.position).magnitude;
    }

    public override string ToString()
    {
        string returnMessage = "State Vector (" + timestamp.ToString("0.00") + "s)\n";
        returnMessage += "\t '-position:\t" + position + "\n";
        returnMessage += "\t '-velocity:\t" + velocity + "\n";
        returnMessage += "\t '-acceleration:\t" + acceleration + "\n";
        returnMessage += "\t '-activeForce:\t" + externalAcceleration + "\n";

        return returnMessage;
    }
}