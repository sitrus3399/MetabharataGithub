using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerNetworkPrefab;
    private const string GAMEPLAY_SCENE_NAME = "GamePlay";
    private bool playerSpawned;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        NetworkManager.OnConnectionEvent += OnConnectionEvent;
        NetworkManager.SceneManager.OnLoadComplete += SceneManager_OnLoadComplete;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkManager.OnConnectionEvent -= OnConnectionEvent;
        NetworkManager.SceneManager.OnLoadComplete -= SceneManager_OnLoadComplete;
    }

    private void SceneManager_OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        HandlePlayerSpawn();
    }

    private void HandlePlayerSpawn()
    {
        if (SceneManager.GetActiveScene().name != GAMEPLAY_SCENE_NAME || NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.IsHost)
        {
            if (playerSpawned) return;
            SpawnPlayerNetworkPrefab(NetworkManager.Singleton.LocalClientId);
            playerSpawned = true;
        }
        else if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (playerSpawned) return;
            RequestPlayerSpawnServerRpc();
            playerSpawned = true;
        }
    }

    private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData eventData)
    {
        if (eventData.EventType == ConnectionEvent.ClientConnected)
        {
            HandlePlayerSpawn();
        }
        else if (eventData.EventType == ConnectionEvent.ClientDisconnected)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            var ownedObjects = NetworkManager.Singleton.SpawnManager.GetClientOwnedObjects(eventData.ClientId);
            foreach (var netObj in ownedObjects)
            {
                if (netObj == null || !netObj.IsSpawned) continue;
                netObj.Despawn();
                Debug.Log($"Cleaned up objects for disconnected client {eventData.ClientId}.");
            }
        }
    }

    private void SpawnPlayerNetworkPrefab(ulong clientId)
    {
        if (playerNetworkPrefab == null)
        {
            Debug.LogWarning("Player Network Prefab is not assigned in PlayerSpawner.");
            return;
        }

        var existingPlayer = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (existingPlayer != null && existingPlayer.IsSpawned)
        {
            Debug.LogWarning($"Player object for client {clientId} already exists.");
            return;
        }

        var player = Instantiate(playerNetworkPrefab, Vector3.zero, Quaternion.identity);
        var netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("Player prefab does not have a NetworkObject component.");
            Destroy(player);
            return;
        }
        netObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"Spawned player for client {clientId} as SERVER.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestPlayerSpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("RequestPlayerSpawnServerRpc called on non-server instance.");
            return;
        }
        if (SceneManager.GetActiveScene().name != GAMEPLAY_SCENE_NAME)
        {
            Debug.LogWarning("RequestPlayerSpawnServerRpc called in wrong scene.");
            return;
        }
        SpawnPlayerNetworkPrefab(clientId);
    }
}