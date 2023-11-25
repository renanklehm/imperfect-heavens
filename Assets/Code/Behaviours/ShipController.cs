using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

[RequireComponent(typeof(FreeBody))]
public class ShipController : NetworkBehaviour, INetworkRunnerCallbacks
{
    public Vector3 burnDirection;
    public float burnDuration;
    public float burnStrength;

    private FreeBody freeBody;
    private InputStruct inputStruct;

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
        freeBody.AddForce(input.burnDirection * input.burnStrength, input.burnDuration);
        burnDirection = Vector3.zero;
    }

    private bool IsInputReady()
    {
        return burnDirection != Vector3.zero && burnStrength > 0;
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (IsInputReady())
        {
            burnDuration = burnDuration == 0 ? Time.fixedDeltaTime : burnDuration;
            inputStruct = new InputStruct(burnDirection, burnStrength, burnDuration, GravityManager.Instance.timeStamp);
            input.Set(inputStruct);
        }
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug.Log("Player: " + player.PlayerId + " missed " + input);
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
