using System;
using Fusion;
using UnityEngine;

[Serializable]
public struct StateVector : INetworkStruct
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector3 velocity;
    [SerializeField] public Vector3 acceleration;
    [SerializeField] public Vector3 activeForce;
    public float timestamp;

    public StateVector(Vector3 _position = new Vector3())
    {
        position = _position;
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        activeForce = Vector3.zero;
        timestamp = 0f;
    }


    public StateVector(Vector3 _position, Vector3 _velocity, Vector3 _acceleration, float _ticket, Vector3 _activeForce = new Vector3())
    {
        position = _position;
        velocity = _velocity;
        acceleration = _acceleration;
        timestamp = _ticket;
        activeForce = _activeForce;
    }

    public StateVector(StateVector source)
    {
        position = source.position;
        velocity = source.velocity;
        acceleration = source.acceleration;
        timestamp = source.timestamp;
        activeForce = source.activeForce;
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
        Vector3 position = Vector3.Lerp(a.position, b.position, factor);
        Vector3 velocity = Vector3.Lerp(a.velocity, b.velocity, factor);
        Vector3 acceleration = Vector3.Lerp(a.acceleration, b.acceleration, factor);
        float simulationTicket = GravityManager.Instance.timestamp;

        return new StateVector(position, velocity, acceleration, simulationTicket);
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