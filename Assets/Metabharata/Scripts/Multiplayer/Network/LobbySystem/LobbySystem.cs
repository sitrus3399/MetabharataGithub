using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metabharata.Network.Multiplayer.NetworkServiceSystem;
using NyxMachina.Shared.EventFramework;
using NyxMachina.Shared.EventFramework.Core.Messenger;
using NyxMachina.Shared.EventFramework.Core.Payloads;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbySystem
{
    /// <summary>
    /// Indicates if the system is currently initializing.
    /// </summary>
    public bool IsInitializing { get; set; }

    /// <summary>
    /// Indicates if the system have been initialized.
    /// </summary>
    public bool IsInitialized { get; set; }

    public List<LobbyWrapper> LobbyList { get; set; }

    public LobbyWrapper CurrentLobby { get; set; }

    public JoinAllocation CurrentJoinAllocation { get; set; }

    private readonly LobbyEventCallbacks _lobbyEvent = new();

    private string _userInputPassword;

    public string CurrentPlayerId
    {
        get
        {
            if (!NetworkServiceInitiator.Instance.IsInitialized)
                return string.Empty;
            return AuthenticationService.Instance.PlayerId;
        }
    }

    public string CurrentPlayerName
    {
        get
        {
            if (!NetworkServiceInitiator.Instance.IsInitialized)
                return string.Empty;
            return AuthenticationService.Instance.PlayerName;
        }
    }

    public bool IsHost => CurrentLobby != null && CurrentLobby.Lobby.HostId == CurrentPlayerId;
    public bool IsClient => IsHost == false;

    public void InitializeSystem()
    {
        if (IsInitialized || IsInitializing)
        {
            Debug.LogWarning("Lobby system is already initialized or initializing.");
            return;
        }

        IsInitializing = true;

        // Initialize the lobby list
        LobbyList = new List<LobbyWrapper>();

        IsInitialized = true;
        IsInitializing = false;

        Debug.Log("Lobby system initialized.");
    }

    public void Shutdown()
    {
        // Leave the lobby if in one
        if (CurrentLobby != null)
        {
            try
            {
                // Synchronously wait for leave to complete (safe here since we're shutting down)
                LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, CurrentPlayerId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error while leaving lobby during shutdown: {e}");
            }
            CurrentLobby = null;
        }

        // Shutdown the network manager if running
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
        {
            NetworkManager.Singleton.Shutdown();
        }

        // Clear allocations and state
        CurrentJoinAllocation = null;
        LobbyList = null;
        IsInitialized = false;
        IsInitializing = false;
        _userInputPassword = null;

        Debug.Log("Lobby system shutdown complete.");
    }

    public async Task<LobbyWrapper> CreateLobby(LobbySetting setting = null)
    {
        if (setting == null)
        {
            setting = new LobbySetting
            {
                LobbyName = "DefaultLobby",
                MaxPlayers = 2,
                IsLocked = false,
                Password = string.Empty,
                GameMode = "Casual",
                Map = "DefaultMap"
            };
            Debug.LogWarning("LobbySetting is null, using default values.");
        }

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(setting.MaxPlayers);
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var data = new Dictionary<string, DataObject>
            {
                { LobbyWrapper.RelayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
                { LobbyWrapper.LobbyNameKey, new DataObject(DataObject.VisibilityOptions.Public, setting.LobbyName) },
                { LobbyWrapper.MaxPlayersKey, new DataObject(DataObject.VisibilityOptions.Public, setting.MaxPlayers.ToString()) },
                { LobbyWrapper.IsLockedKey, new DataObject(DataObject.VisibilityOptions.Public, setting.IsLocked.ToString()) },
                { LobbyWrapper.PasswordKey, new DataObject(DataObject.VisibilityOptions.Private, setting.Password) },
                { LobbyWrapper.GameModeKey, new DataObject(DataObject.VisibilityOptions.Public, setting.GameMode) },
                { LobbyWrapper.MapKey, new DataObject(DataObject.VisibilityOptions.Public, setting.Map) }
            };

            var lobbyOptions = new CreateLobbyOptions
            {
                Data = data
            };

            var lobby = await LobbyService.Instance.CreateLobbyAsync(setting.LobbyName, setting.MaxPlayers, lobbyOptions);

            CurrentLobby = new LobbyWrapper(lobby);

            LobbySystemEvent.LobbyCreatedEvent.Publish(new LobbySystemEvent.LobbyCreatedEvent(CurrentLobby, CurrentJoinAllocation));

            // Start host
            if (NetworkManager.Singleton.StartHost())
            {
                RegisterPasswordCheckHandler();
                Debug.Log("Host started successfully.");
            }

            Debug.Log($"Lobby '{setting.LobbyName}' created successfully with code: {lobby.LobbyCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create lobby...\n" +
                           $"Log: {e}");
            return null;
        }

        return CurrentLobby;
    }

    public async Task JoinLobbyByIdAsync(string lobbyId, string password = "")
    {
        if (string.IsNullOrWhiteSpace(lobbyId))
        {
            Debug.LogError("Lobby ID cannot be null or empty.");
            return;
        }
        _userInputPassword = password;
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to lobby...\n" +
                           $"Log: {e}");
            return;
        }

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEvent);
        await StartClientAsync(CurrentLobby.RelayJoinCode);

        LobbySystemEvent.PlayerJoinedEvent.Publish(new LobbySystemEvent.PlayerJoinedEvent(CurrentPlayerId, CurrentPlayerName));
    }

    public async Task JoinLobbyByCodeAsync(string joinCode, string password = "")
    {
        if (string.IsNullOrWhiteSpace(joinCode))
        {
            Debug.LogError("Join code cannot be null or empty.");
            return;
        }
        _userInputPassword = password;
        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to lobby...\n" +
                           $"Log: {e}");
            return;
        }

        await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEvent);
        await StartClientAsync(CurrentLobby.RelayJoinCode);

        LobbySystemEvent.PlayerJoinedEvent.Publish(new LobbySystemEvent.PlayerJoinedEvent(CurrentPlayerId, CurrentPlayerName));
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            CurrentJoinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.Log($"Failed to Join Relay...\n" +
                      $"Log: {e}");
            throw;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(CurrentJoinAllocation.ToRelayServerData("dtls"));

        // Subscribe to the connection event
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        try
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started successfully.");
                // Do NOT call LoadScene here. The server will handle scene changes.
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start client...\n" +
                           $"Log:  {e}");
            throw;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Only send password if this is the local client
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SendPassword();
        }
    }

    private void SendPassword()
    {
        if (string.IsNullOrWhiteSpace(_userInputPassword)) return;

        var msg = new JoinPasswordMessage { Password = _userInputPassword };

        using var writer = new FastBufferWriter(sizeof(int) + (msg.Password?.Length ?? 0) * sizeof(char), Unity.Collections.Allocator.Temp);
        writer.WriteNetworkSerializable(msg);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(CheckJoinPassword), NetworkManager.ServerClientId, writer);
    }

    public async Task LeaveLobby()
    {
        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby to leave.");
            return;
        }

        try
        {
            // Leave the lobby
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, CurrentPlayerId);

            // Stop the network client if running
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsConnectedClient)
            {
                NetworkManager.Singleton.Shutdown();
            }

            // Clear current lobby and allocation
            CurrentLobby = null;
            CurrentJoinAllocation = null;

            // Raise event
            LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, AuthenticationService.Instance.PlayerName));

            Debug.Log("Left the lobby successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to leave lobby..." +
                           $"Log: {e}");
        }
    }

    public async void SetLobbySettings(LobbySetting setting)
    {
        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby to update.");
            return;
        }
        if (setting == null)
        {
            Debug.LogWarning("LobbySetting is null.");
            return;
        }

        try
        {
            var data = new Dictionary<string, DataObject>
            {
                { LobbyWrapper.RelayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, CurrentLobby.RelayJoinCode) },
                { LobbyWrapper.LobbyNameKey, new DataObject(DataObject.VisibilityOptions.Public, setting.LobbyName) },
                { LobbyWrapper.MaxPlayersKey, new DataObject(DataObject.VisibilityOptions.Public, setting.MaxPlayers.ToString()) },
                { LobbyWrapper.IsLockedKey, new DataObject(DataObject.VisibilityOptions.Public, setting.IsLocked.ToString()) },
                { LobbyWrapper.PasswordKey, new DataObject(DataObject.VisibilityOptions.Private, setting.Password) },
                { LobbyWrapper.GameModeKey, new DataObject(DataObject.VisibilityOptions.Public, setting.GameMode) },
                { LobbyWrapper.MapKey, new DataObject(DataObject.VisibilityOptions.Public, setting.Map) }
            };

            var options = new UpdateLobbyOptions
            {
                Data = data
            };

            // Optionally update max players if it changed
            var updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);

            // Update the local wrapper
            CurrentLobby = new LobbyWrapper(updatedLobby);

            // Notify listeners
            LobbyWrapper.OnSettingChanged?.Invoke(setting);
            LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));

            Debug.Log("Lobby settings updated successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update lobby settings: {e}");
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby available.");
            return;
        }
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("playerId cannot be null or empty.");
            return;
        }

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
            Debug.Log($"Player {playerId} kicked from the lobby.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to kick player: {e}");
        }
    }

    public async void SetReady(bool isReady)
    {
        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby available.");
            return;
        }
        if (string.IsNullOrEmpty(CurrentPlayerId))
        {
            Debug.LogWarning("Current player ID is not available.");
            return;
        }

        try
        {
            var playerData = new Dictionary<string, PlayerDataObject>
            {
                { PlayerLobbyData.PlayerReadyKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, isReady.ToString()) }
            };

            var options = new UpdatePlayerOptions
            {
                Data = playerData
            };

            await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, CurrentPlayerId, options);

            Debug.Log($"Set ready status to {isReady} for player {CurrentPlayerId}.");
            LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set ready status...\n" +
                           $"Log: {e}");
        }
    }

    public async void StartGame()
    {
        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby available.");
            return;
        }

        // Only the host should be able to start the game
        if (CurrentLobby.Lobby.HostId != CurrentPlayerId)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        try
        {
            // Set a flag in the lobby data to indicate the game has started
            var data = new Dictionary<string, DataObject>
            {
                { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, true.ToString()) }
            };

            var options = new UpdateLobbyOptions
            {
                Data = data
            };

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);

            Debug.Log("Game started! Notified all players via lobby data.");

            // Optionally, trigger a local event or call game manager logic here
            // HostGameManager.Instance.StartGame(); // Uncomment if you have such logic

            LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start the game: {e}");
        }
    }

    private void RegisterPasswordCheckHandler()
    {
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(nameof(CheckJoinPassword), CheckJoinPassword);
    }

    private void CheckJoinPassword(ulong clientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out JoinPasswordMessage msg);

        // Get the password from the lobby's private data
        string correctPassword = null;
        if (CurrentLobby.Lobby is { Data: not null } && CurrentLobby.Lobby.Data.TryGetValue(LobbyWrapper.PasswordKey, out var pwObj))
        {
            correctPassword = pwObj.Value;
        }

        if (string.IsNullOrEmpty(correctPassword) || msg.Password == correctPassword)
        {
            // Password correct or not required: allow join
            Debug.Log($"Client {clientId} joined with correct password.");
        }
        else
        {
            // Password wrong: kick the client
            Debug.LogWarning($"Client {clientId} provided wrong password. Kicking...");
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }

    /// <summary>
    /// Checks if the system is initialized and authenticated.
    /// </summary>
    public bool IsSystemReady()
    {
        return IsInitialized;
    }
}

public struct JoinPasswordMessage : INetworkSerializable
{
    public string Password;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Password);
    }
}