using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct InputStruct : INetworkInput
{
    public NetworkButtons burnDirection;
    public float burnStrength;
    public float burnDuration;
    public float burnStartTime;

    public InputStruct(NetworkButtons _burnDirection, float _burnStrength, float _burnDuration, float _burnStartTime)
    {
        burnDirection = _burnDirection;
        burnStrength = _burnStrength;
        burnDuration = _burnDuration;
        burnStartTime = _burnStartTime;
    }
}