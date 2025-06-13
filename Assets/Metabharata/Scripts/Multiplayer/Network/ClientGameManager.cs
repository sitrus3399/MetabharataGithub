using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metabharata.Network.Multiplayer.NetworkServiceSystem;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : MonoBehaviour
{
    public static ClientGameManager Instance { get; private set; }

    private JoinAllocation allocation;

    private const string MenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GamePlay";

    private string _userInputPassword;

    private Lobby _currentLobby;

    private LobbyEventCallbacks _lobbyEvent = new();
    private List<LobbyPlayerJoined> currentLobbyList;

    private Action<LobbyPlayerJoined> OnPlayerJoinedLobby;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Show error to user: "Wrong password or failed to join."
            Debug.LogWarning("Disconnected from server. Possibly due to wrong password.");
            GoToMenu();
        }
    }

    private void PlayerJoinedLobby(LobbyPlayerJoined obj)
    {
        if (_currentLobby.HostId != obj.Player.Id) return;
    }

    private void OnPlayerJoinLobby(List<LobbyPlayerJoined> obj)
    {
        if (currentLobbyList == null)
        {
            currentLobbyList = new List<LobbyPlayerJoined>(obj);
            foreach (var p in obj)
            {
                OnPlayerJoinedLobby?.Invoke(p);
            }
            return;
        }

        // Build a set of existing player IDs for fast lookup
        var existingIds = new HashSet<string>();
        foreach (var p in currentLobbyList)
        {
            if (p.Player != null && !string.IsNullOrEmpty(p.Player.Id))
                existingIds.Add(p.Player.Id);
        }

        // Find and invoke event for new players
        foreach (var p in obj)
        {
            if (p.Player != null && !string.IsNullOrEmpty(p.Player.Id) && !existingIds.Contains(p.Player.Id))
            {
                OnPlayerJoinedLobby?.Invoke(p);
            }
        }

        // Update the current lobby list to the latest
        currentLobbyList = new List<LobbyPlayerJoined>(obj);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task JoinLobby(string joinCode, string password = "")
    {
        _userInputPassword = password;

        // Example (in ClientGameManager or similar)
        var joinOptions = new JoinLobbyByCodeOptions();
        try
        {
            _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinOptions);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to lobby...\n" +
                           $"Error Info: {e}");
            return;
        }
        
        await LobbyService.Instance.SubscribeToLobbyEventsAsync(_currentLobby.Id, _lobbyEvent);
        await StartClientAsync(_currentLobby.Data["relay"].Value);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (System.Exception exception)
        {
            Debug.Log($"ERROR StartClient joinCode {joinCode} {exception}");
            throw;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

        // Subscribe to the connection event
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully.");
            // Do NOT call LoadScene here. The server will handle scene changes.
        }
        else
        {
            Debug.LogError("Failed to start client.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Only send password if this is the local client
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            //SendPassword();
        }
    }
}