using Fusion;
using UnityEngine;

public struct Maneuver : INetworkStruct
{
    public float startTimestamp;
    public float burnDuration;
    public float throttle;
    public Vector3 direction;
}
