using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

[RequireComponent(typeof(FreeBody))]
public class ShipController : NetworkBehaviour, INetworkRunnerCallbacks
{
    public BurnDirection burnDirection;
    public float burnDuration;
    public float burnStrength;
    public FreeBody freeBody;

    private void Start()
    {
        freeBody = GetComponent<FreeBody>();
        if (Runner != null)
        {
            Debug.Log("Adding " + name + " to network callbacks");
            Runner.AddCallbacks(this);
        }
    }

    public void OnDisable()
    {
        if (Runner != null)
        {
            Debug.Log("Removing " + name + " to network callbacks");
            Runner.RemoveCallbacks(this);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<InputStruct>(out var input) == false) return;
        Debug.Log(name + " - " + input.burnDirection.Bits);
        Vector3 direction = Vector3.zero;
        if (input.burnDirection.IsSet(BurnDirection.nullDir)) return;
        else if (input.burnDirection.IsSet(BurnDirection.Prograde)) direction = transform.forward;
        else if (input.burnDirection.IsSet(BurnDirection.Retrograde)) direction = -transform.forward;
        else if (input.burnDirection.IsSet(BurnDirection.Normal)) direction = transform.up;
        else if (input.burnDirection.IsSet(BurnDirection.AntiNormal)) direction = -transform.up;
        else if (input.burnDirection.IsSet(BurnDirection.RadialOut)) direction = transform.right;
        else if (input.burnDirection.IsSet(BurnDirection.RadialIn)) direction = -transform.right;
        freeBody.AddForce(direction * input.burnStrength, input.burnDuration);
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        InputStruct newInput = new InputStruct();

        if (burnDirection != BurnDirection.nullDir)
        {
            newInput.burnDirection.Set(BurnDirection.Prograde, burnDirection == BurnDirection.Prograde);
            newInput.burnDirection.Set(BurnDirection.Retrograde, burnDirection == BurnDirection.Retrograde);
            newInput.burnDirection.Set(BurnDirection.Normal, burnDirection == BurnDirection.Normal);
            newInput.burnDirection.Set(BurnDirection.AntiNormal, burnDirection == BurnDirection.AntiNormal);
            newInput.burnDirection.Set(BurnDirection.RadialIn, burnDirection == BurnDirection.RadialIn);
            newInput.burnDirection.Set(BurnDirection.RadialOut, burnDirection == BurnDirection.RadialOut);
            newInput.burnDuration = burnDuration;
            newInput.burnStrength = burnStrength;
            newInput.burnStartTime = GravityManager.Instance.timestamp;
            burnDirection = BurnDirection.nullDir;
            input.Set(newInput);        
        }       
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {
        return;
    }
}
