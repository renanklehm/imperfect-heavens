using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef _gravityControllerPrefab;
    [SerializeField] private NetworkPrefabRef _gravitationSystemPrefab;
    [SerializeField] private NetworkPrefabRef _playerPrefab;
    [SerializeField] float _initialOrbit;
    private GravitationalSystem _gravitationSystem;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private NetworkRunner _runner;

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
        }
    }

    async void StartGame(GameMode mode)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (_runner.IsServer)
        {
            _runner.Spawn(_gravityControllerPrefab);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            StartCoroutine(SafeSpawnPlayer(player));
        }
    }

    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_runner.IsServer)
        {
            _runner.Despawn(_spawnedCharacters[player]);
        }
    }

    IEnumerator SafeSpawnPlayer(PlayerRef player)
    {
        while (GravityManager.Instance == null)
        {
            yield return new WaitForEndOfFrame();
        }

        if (_gravitationSystem == null)
        {
            _gravitationSystem = _runner.Spawn(_gravitationSystemPrefab).GetComponent<GravitationalSystem>();
        }

        Vector3 spawnPosition = UnityEngine.Random.insideUnitSphere.normalized * _initialOrbit;
        Vector3 initialVelocity = Vector3.Cross(spawnPosition, Vector3.up).normalized * Solver.GetOrbitalSpeed(_initialOrbit, _gravitationSystem.centralBody);
        NetworkObject newPlayer = _runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
        newPlayer.name = "Player (ID: " + player.PlayerId.ToString("00") + ")";
        newPlayer.GetComponent<Body>().initialVelocity = initialVelocity;
        _spawnedCharacters.Add(player, newPlayer);
        _runner.SetPlayerObject(player, newPlayer);
    }

    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
    }

    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("Connection failed");
    }

    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("Connection request");
    }

    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log("Custom auth response");
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("Disconnected from server");
    }

    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("Migrating host to " + hostMigrationToken);
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        Debug.Log("Reliable data received");
    }

    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load done");
    }

    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Scene started");
    }

    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Scene loaded");
    }

    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Shutdown msg: " + shutdownReason);
    }

    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug.Log("Simulation msg: " + message);
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        return;
    }

    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        return;
    }
}