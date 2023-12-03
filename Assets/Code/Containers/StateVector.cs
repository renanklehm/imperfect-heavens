using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct StateVector : INetworkStruct
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector3 velocity;
    [SerializeField] public Vector3 acceleration;
    [SerializeField] public Vector3 prograde;
    [SerializeField] public Vector3 radialOut;
    [SerializeField] public Vector3 normal;
    public Vector3 gravityAcceleration;
    public Vector3 activeForce;
    public float timestamp;

    public StateVector(Vector3 _position = new Vector3())
    {
        position = _position;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        activeForce = Vector3.zero;
        gravityAcceleration = Vector3.zero;
        prograde = Vector3.forward;
        radialOut = Vector3.right;
        normal = Vector3.up;
        timestamp = 0f;
    }


    public StateVector(
        Vector3 _position,
        Vector3 _velocity,
        Vector3 _acceleration,
        Vector3 _prograde,
        Vector3 _radialOut,
        float _ticket,
        Vector3 _passiveForce,
        Vector3 _activeForce = new Vector3()
        )
    {
        position = _position;
        velocity = _velocity;
        acceleration = _acceleration;
        timestamp = _ticket;
        gravityAcceleration = _passiveForce;
        activeForce = _activeForce;
        prograde = _prograde;
        radialOut = _radialOut;
        normal = Vector3.Cross(prograde, radialOut);
    }

    public StateVector(StateVector source)
    {
        position = source.position;
        velocity = source.velocity;
        acceleration = source.acceleration;
        timestamp = source.timestamp;
        gravityAcceleration = source.gravityAcceleration;
        activeForce = source.activeForce;
        prograde = source.prograde;
        radialOut = source.radialOut;
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
        Vector3 _prograde = Vector3.Lerp(a.prograde, b.prograde, factor);
        Vector3 _radialOut = Vector3.Lerp(a.radialOut, b.radialOut, factor);
        float _ticket = Mathf.Lerp(a.timestamp, b.timestamp, factor);
        Vector3 _passiveForce = Vector3.Lerp(a.gravityAcceleration, b.gravityAcceleration, factor);
        Vector3 _activeForce = Vector3.Lerp(a.activeForce, b.activeForce, factor);

        return new StateVector(_position, _velocity, _acceleration, _prograde, _radialOut, _ticket, _passiveForce, _activeForce);
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
        returnMessage += "\t '-activeForce:\t" + activeForce + "\n";

        return returnMessage;
    }
}