using System;
using UnityEngine;

[Serializable]
public struct StateVector
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector3 velocity;
    [SerializeField] public Vector3 acceleration;
    [SerializeField] public Vector3 activeForce;
    public float timestamp;

    public StateVector(Vector3 _position, Vector3 _velocity, Vector3 _acceleration, float _ticket, Vector3 _activeForce = new Vector3())
    {
        position = _position;
        velocity = _velocity;
        acceleration = _acceleration;
        timestamp = _ticket;
        activeForce = _activeForce;
    }

    public static StateVector LerpVector(StateVector a, StateVector b, float factor)
    {
        Vector3 position = Vector3.Lerp(a.position, b.position, factor);
        Vector3 velocity = Vector3.Lerp(a.velocity, b.velocity, factor);
        Vector3 acceleration = Vector3.Lerp(a.acceleration, b.acceleration, factor);
        float simulationTicket = Mathf.Lerp(a.timestamp, b.timestamp, factor);

        return new StateVector(position, velocity, acceleration, simulationTicket);
    }

    public static float deltaPosition(StateVector a, StateVector b)
    {
        return (a.position - b.position).magnitude;
    }
}