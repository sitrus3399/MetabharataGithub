using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metabharata.Multiplayer.Network.LobbySystem;
using Metabharata.Network.Multiplayer.NetworkServiceSystem;
using NyxMachina.Shared.EventFramework;
using NyxMachina.Shared.EventFramework.Core.Messenger;
using NyxMachina.Shared.EventFramework.Core.Payloads;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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

    private readonly LobbyEventCallbacks _lobbyEventCallback = new();
    private ILobbyEvents _lobbyEvent;

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
            var playerName = AuthenticationService.Instance?.PlayerName;
            if (string.IsNullOrWhiteSpace(playerName))
                playerName = "Guest";
            return playerName;
        }
    }

    public Player CurrentPlayerData;

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

        
        
        
        // Subscribe to PlayerJoinedEvent
        EventMessenger.Main.Subscribe<LobbySystemEvent.PlayerJoinedEvent>(OnPlayerJoinedEvent);

        Debug.Log("Lobby system initialized.");
    }

    private void OnPlayerJoinedEvent(LobbySystemEvent.PlayerJoinedEvent obj)
    {
        // TODO: Kicked the player in here, not OnConnected NetworkManager
        //throw new NotImplementedException();
    }

    private void AddLobbyEventListener()
    {
        _lobbyEventCallback.PlayerJoined += OnPlayerJoinLobby;
        _lobbyEventCallback.PlayerLeft += OnPlayerLeftLobby;
        _lobbyEventCallback.KickedFromLobby += OnPlayerKickedFromLobby;
        _lobbyEventCallback.LobbyChanged += OnLobbyChanged;
        _lobbyEventCallback.DataAdded += OnLobbyDataChanged;
        _lobbyEventCallback.DataChanged += OnLobbyDataChanged;
        _lobbyEventCallback.DataRemoved += OnLobbyDataChanged;
        _lobbyEventCallback.PlayerDataAdded += OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataChanged += OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataRemoved += OnLobbyPlayerDataChanged;
        _lobbyEventCallback.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
    }

    private void RemoveLobbyEventListener()
    {
        _lobbyEventCallback.PlayerJoined -= OnPlayerJoinLobby;
        _lobbyEventCallback.PlayerLeft -= OnPlayerLeftLobby;
        _lobbyEventCallback.KickedFromLobby -= OnPlayerKickedFromLobby;
        _lobbyEventCallback.LobbyChanged -= OnLobbyChanged;
        _lobbyEventCallback.DataAdded -= OnLobbyDataChanged;
        _lobbyEventCallback.DataChanged -= OnLobbyDataChanged;
        _lobbyEventCallback.DataRemoved -= OnLobbyDataChanged;
        _lobbyEventCallback.PlayerDataAdded -= OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataChanged -= OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataRemoved -= OnLobbyPlayerDataChanged;
        _lobbyEventCallback.LobbyEventConnectionStateChanged -= OnLobbyEventConnectionStateChanged;
    }

    private void OnLobbyPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj)
    {
        // Update player data in the current lobby
        SendCurrentLobbyDataToServer();
        Debug.Log("Lobby player data changed.");
        LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
    }

    private void OnLobbyDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
    {
        // Update lobby data (settings, custom properties, etc.)
        if (CurrentLobby?.Lobby?.Data != null)
        {
            // Optionally update local lobby settings if changed
            if (CurrentLobby.Lobby.Data.TryGetValue(LobbyDataModel.LobbyDataModelKey, out var lobbyDataObj) &&
                !string.IsNullOrWhiteSpace(lobbyDataObj.Value))
            {
                var model = lobbyDataObj.Value.ToLobbyDataModel();
                if (model != null)
                {
                    CurrentLobby.PlayerLobbyDataList = model.PlayerLobbyDataList;
                    CurrentLobby.LobbySetting = model.LobbySetting;
                }
            }
        }
        Debug.Log("Lobby data changed.");
        LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
    }

    private void OnLobbyChanged(ILobbyChanges obj)
    {
        // General lobby change event (fallback)
        SendCurrentLobbyDataToServer();
        Debug.Log("Lobby changed (general).");
        LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        // Handle connection state changes (e.g., lost connection, reconnected)
        Debug.Log($"Lobby event connection state changed: {state}");
        if (state == LobbyEventConnectionState.Unsynced)
        {
            Debug.LogWarning("Lost connection to lobby events. You may need to rejoin or refresh.");
            // Optionally, notify the user or attempt reconnection logic here
        }

        switch (state)
        {
            case LobbyEventConnectionState.Subscribed:
                break;
            case LobbyEventConnectionState.Unsubscribed:
                RemoveLobbyEventListener();
                break;
        }
    }

    private void OnPlayerKickedFromLobby()
    {
        Debug.LogWarning("You have been kicked from the lobby.");
        // Leave the lobby and clean up
        CurrentLobby = null;
        CurrentJoinAllocation = null;
        LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, CurrentPlayerName));
        _lobbyEvent = null; // Clear the event handler to avoid memory leaks
        // Optionally, notify the UI or redirect the player to a safe screen
    }

    private void OnPlayerJoinLobby(List<LobbyPlayerJoined> players)
    {
        foreach (var player in players)
        {
            var playerObject = player.Player;
            playerObject.Data.TryGetValue(PlayerLobbyData.PlayerLobbyDataKey, out var playerDataObject);
            PlayerLobbyData playerData = playerDataObject?.Value.FromJson();

            Debug.Log($"Player joined: {playerObject.Id} ({playerData?.PlayerName})");
            CurrentLobby.PlayerLobbyDataList.Add(playerData);
        }

        // Send updated lobby data to the server
        SendCurrentLobbyDataToServer();

        LobbySystemEvent.PlayerJoinedEvent.Publish(new LobbySystemEvent.PlayerJoinedEvent(CurrentPlayerId, CurrentPlayerName));
    }

    private void OnPlayerLeftLobby(List<int> playerIndex)
    {
        if (CurrentLobby is { PlayerLobbyDataList: not null })
        {
            // Remove players by index (descending order to avoid shifting indices)
            playerIndex.Sort((a, b) => b.CompareTo(a));
            foreach (var index in playerIndex)
            {
                if (index >= 0 && index < CurrentLobby.PlayerLobbyDataList.Count)
                {
                    var leftPlayer = CurrentLobby.PlayerLobbyDataList[index];
                    Debug.Log($"Player left: {leftPlayer.PlayerId} ({leftPlayer.PlayerName})");
                    CurrentLobby.PlayerLobbyDataList.RemoveAt(index);
                }
                else
                {
                    Debug.LogWarning($"Invalid player index {index} in OnPlayerLeftLobby.");
                }
            }
        }

        SendCurrentLobbyDataToServer();
        LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, CurrentPlayerName));
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
            // allocation must be used immediately after creation to avoid timeout
            var allocation = await RelayService.Instance.CreateAllocationAsync(setting.MaxPlayers);
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            var relayServerData = allocation.ToRelayServerData("dtls");

            // Set the relay server data in the Unity Transport component
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.OnConnectionEvent -= HandleOnConnectionEvent;
            NetworkManager.Singleton.OnConnectionEvent += HandleOnConnectionEvent;
            NetworkManager.Singleton.OnPreShutdown -= PreShutdownHandle;
            NetworkManager.Singleton.OnPreShutdown += PreShutdownHandle;

            // Start host 
            if (NetworkManager.Singleton.StartHost())
            {
                RegisterPasswordCheckHandler();
                Debug.Log($"Host started...");
            }

            var hostLobbyData = new PlayerLobbyData
            {
                PlayerId = CurrentPlayerId,
                PlayerName = CurrentPlayerName,
                IsReady = false, // Default to not ready
                IsHost = true,
                LocalClientId = NetworkManager.Singleton.LocalClientId
            };

            var playerLobbyData = new Dictionary<string, PlayerDataObject>()
            {
                { PlayerLobbyData.PlayerLobbyDataKey, hostLobbyData }
            };

            var playerLobbyDataList = new List<PlayerLobbyData>
            {
                hostLobbyData
            };

            var lobbyDataModel = new LobbyDataModel
            {
                PlayerLobbyDataList = playerLobbyDataList,
                LobbySetting = setting,
                RelayJoinCode = relayJoinCode
            };

            var roomData = new Dictionary<string, DataObject>
            {
                { LobbyDataModel.LobbyDataModelKey, lobbyDataModel }
            };

            var lobbyOptions = new CreateLobbyOptions
            {
                Data = roomData,
                Player = new Player()
                {
                    Profile = new PlayerProfile(CurrentPlayerName),
                    Data = playerLobbyData,
                    Joined = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                },
                IsLocked = false,
                IsPrivate = false
            };

            var lobby = await LobbyService.Instance.CreateLobbyAsync(setting.LobbyName, setting.MaxPlayers, lobbyOptions);

            RemoveLobbyEventListener();
            AddLobbyEventListener();

            CurrentLobby = new LobbyWrapper(lobby);
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEventCallback);

            LobbySystemEvent.LobbyCreatedEvent.Publish(new LobbySystemEvent.LobbyCreatedEvent(CurrentLobby, CurrentJoinAllocation));

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

        var clientLobbyData = new PlayerLobbyData
        {
            PlayerId = CurrentPlayerId,
            PlayerName = CurrentPlayerName,
            IsReady = false, // Default to not ready
            IsHost = false,
            LocalClientId = NetworkManager.Singleton.LocalClientId + 1
        };

        var joinLobbyOptions = new JoinLobbyByIdOptions()
        {
            Player = new Player()
            {
                Profile = new PlayerProfile(CurrentPlayerName),
                Joined = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { PlayerLobbyData.PlayerLobbyDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, clientLobbyData) }
                }
            }
        };

        RemoveLobbyEventListener();
        AddLobbyEventListener();

        CurrentPlayerData = joinLobbyOptions.Player;

        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyOptions);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to lobby...\n" +
                           $"Log: {e}");
            return;
        }

        try
        {
            _lobbyEvent = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEventCallback);
        }
        catch (LobbyServiceException e)
        {
            switch (e.Reason) {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{CurrentLobby.Id}]. We did not need to try and subscribe again. Exception Message: {e.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {e.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}"); throw;
                default: throw;
            }
        }

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

        var clientLobbyData = new PlayerLobbyData
        {
            PlayerId = CurrentPlayerId,
            PlayerName = CurrentPlayerName,
            IsReady = false, // Default to not ready
            IsHost = false,
            LocalClientId = NetworkManager.Singleton.LocalClientId + 1
        };

        var joinLobbyOption = new JoinLobbyByCodeOptions()
        {
            Player = new Player()
            {
                Profile = new PlayerProfile(CurrentPlayerName),
                Joined = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { PlayerLobbyData.PlayerLobbyDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, clientLobbyData) }
                }
            }
        };

        RemoveLobbyEventListener();
        AddLobbyEventListener();

        CurrentPlayerData = joinLobbyOption.Player;

        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinLobbyOption);
            // Update PlayerLobbyDataList after join
            SendCurrentLobbyDataToServer();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to lobby...\n" +
                           $"Log: {e}");
            return;
        }
        
        try
        {
            _lobbyEvent = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEventCallback);
        }
        catch (LobbyServiceException e)
        {
            switch (e.Reason) {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{CurrentLobby.Id}]. We did not need to try and subscribe again. Exception Message: {e.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {e.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}"); throw;
                default: throw;
            }
        }

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
        NetworkManager.Singleton.OnConnectionEvent -= HandleOnConnectionEvent;
        NetworkManager.Singleton.OnConnectionEvent += HandleOnConnectionEvent;

        NetworkManager.Singleton.OnPreShutdown -= PreShutdownHandle;
        NetworkManager.Singleton.OnPreShutdown += PreShutdownHandle;

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

    private void SendPassword()
    {
        if (string.IsNullOrWhiteSpace(_userInputPassword)) return;

        var msg = new JoinPasswordMessage { Password = _userInputPassword };

        using var writer = new FastBufferWriter(sizeof(int) + (msg.Password?.Length ?? 0) * sizeof(char), Unity.Collections.Allocator.Temp);
        writer.WriteNetworkSerializable(msg);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(CheckJoinPassword), NetworkManager.ServerClientId, writer);
    }

    public async Task LeaveLobby(string playerId = null)
    {
        if (string.IsNullOrWhiteSpace(playerId))
            playerId = CurrentPlayerId;

        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby to leave.");
            return;
        }

        try
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to leave lobby: {e}");
                return;
            }

            // Stop the network client if running
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsConnectedClient)
            {
                NetworkManager.Singleton.Shutdown();
            }

            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown();
            }

            // Clear current lobby and allocation
            CurrentLobby = null;
            CurrentJoinAllocation = null;

            // Raise event
            LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, CurrentPlayerName));
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
            var lobbyDataModel = new LobbyDataModel
            {
                LobbySetting = setting,
                RelayJoinCode = CurrentLobby.RelayJoinCode,
                PlayerLobbyDataList = CurrentLobby.PlayerLobbyDataList
            };

            var data = new Dictionary<string, DataObject>
            {
                { LobbyDataModel.LobbyDataModelKey, new DataObject(DataObject.VisibilityOptions.Member, lobbyDataModel) }
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

    private byte[] SerializeLobbyDataModel(LobbyDataModel model)
    {
        return System.Text.Encoding.UTF8.GetBytes(model.ToJson());
    }

    private void SendCurrentLobbyDataToServer()
    {
        if (!IsHost || CurrentLobby?.LobbyDataModel == null)
            return;

        var data = SerializeLobbyDataModel(CurrentLobby.LobbyDataModel);

        using var writer = new FastBufferWriter(data.Length, Unity.Collections.Allocator.Temp);
        writer.WriteBytesSafe(data, data.Length);

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) continue; // skip host
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(ReceiveLobbyData), clientId, writer);
        }
    }

    public async Task KickPlayer(string playerId)
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
            // Find the clientId for the playerId
            ulong? clientId = null;
            foreach (var playerLobbyData in CurrentLobby.PlayerLobbyDataList)
            {
                if (playerLobbyData.PlayerId != playerId) continue;
                clientId = playerLobbyData.LocalClientId;
                break;
            }

            if (clientId.HasValue)
            {
                SendKickPlayerMessage(clientId.Value, "Kicked by host");
            }

            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
            Debug.Log($"Player {playerId} kicked from the lobby.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to kick player: {e}");
            return;
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
            var playerDataInstance = CurrentPlayerData;
            if (playerDataInstance == null)
            {
                Debug.LogWarning("Current player data is not available.");
                return;
            }

            var playerLobbyData = playerDataInstance.Data.GetPlayerLobbyData();
            playerLobbyData.IsReady = isReady;
            var playerData = new Dictionary<string, PlayerDataObject>
            {
                { PlayerLobbyData.PlayerLobbyDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerLobbyData) }
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

    #region Event Handler Registration and Unregistration

    private void RegisterLobbyDataHandler()
    {
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(nameof(ReceiveLobbyData));
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(nameof(ReceiveLobbyData), ReceiveLobbyData);
    }

    private void UnregisterLobbyDataHandler()
    {
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(nameof(ReceiveLobbyData));
    }

    private void RegisterKickPlayerHandler()
    {
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(nameof(ReceiveKickPlayer));
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(nameof(ReceiveKickPlayer), ReceiveKickPlayer);
    }

    private void UnregisterKickPlayerHandler()
    {
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(nameof(ReceiveKickPlayer));
    }

    private void RegisterPasswordCheckHandler()
    {
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(nameof(CheckJoinPassword));
        NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(nameof(CheckJoinPassword), CheckJoinPassword);
    }

    private void UnregisterPasswordCheckHandler()
    {
        NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(nameof(CheckJoinPassword));
    }

    #endregion

    /// <summary>
    /// Handles network connection events for the local client.
    /// Registers necessary handlers and sends the join password after a successful connection.
    /// </summary>
    /// <param name="networkManager">The network manager instance.</param>
    /// <param name="eventData">The connection event data.</param>
    private void HandleOnConnectionEvent(NetworkManager networkManager, ConnectionEventData eventData)
    {
        // Only process if this is the local client, the event is a client connection, and this instance is a client (not host)
        var isLocalClient = eventData.ClientId == networkManager.LocalClientId;
        var isClientConnected = eventData.EventType == ConnectionEvent.ClientConnected;
        var isClient = IsClient;

        if (!isLocalClient && !isClientConnected && !isClient)
            return;

        // Register handlers for lobby data and kick messages
        RegisterLobbyDataHandler();
        RegisterKickPlayerHandler();

        // Send the password to the host for validation
        SendPassword();

        // Note: Unregistering is handled in PreShutdown because NetworkManager.Singleton may be null on disconnect.
    }

    private void PreShutdownHandle()
    {
        if (IsHost)
        {
            UnregisterPasswordCheckHandler();
        }

        if (IsClient)
        {
            UnregisterLobbyDataHandler();
            UnregisterKickPlayerHandler();
        }
    }

    private async void ReceiveKickPlayer(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out KickPlayerMessage msg);
        Debug.LogWarning($"You have been kicked from the lobby. Reason: {msg.Reason}");

        // Optionally show a UI message here

        await LeaveLobby();

        // Optionally, leave the lobby and clean up
        // Stop the network client if running
        
        CurrentLobby = null;
        CurrentJoinAllocation = null;
        LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, CurrentPlayerName));
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void SendKickPlayerMessage(ulong clientId, string reason = "Kicked by host")
    {
        var msg = new KickPlayerMessage { Reason = reason };
        using var writer = new FastBufferWriter(sizeof(int) + (msg.Reason?.Length ?? 0) * sizeof(char), Unity.Collections.Allocator.Temp);
        writer.WriteNetworkSerializable(msg);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(ReceiveKickPlayer), clientId, writer);
    }

    private void ReceiveLobbyData(ulong senderClientId, FastBufferReader reader)
    {
        var bytesToRead = reader.Length - reader.Position;
        var data = new byte[bytesToRead];
        reader.ReadBytesSafe(ref data, bytesToRead);
        var json = System.Text.Encoding.UTF8.GetString(data);
        var model = json.ToLobbyDataModel();

        // Update local state/UI
        if (CurrentLobby != null)
        {
            CurrentLobby.LobbyDataModel = model;
            CurrentLobby.PlayerLobbyDataList = model.PlayerLobbyDataList;
            CurrentLobby.LobbySetting = model.LobbySetting;
        }
    }

    private async void CheckJoinPassword(ulong clientNetworkId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out JoinPasswordMessage msg);

        // Get the password from the lobby's private data
        var correctPassword = CurrentLobby?.LobbySetting.Password;

        if (string.IsNullOrEmpty(correctPassword) || msg.Password == correctPassword)
        {
            // Password correct or not required: allow join
            Debug.Log($"Client {clientNetworkId} joined with correct password.");
        }
        else
        {
            // Password wrong: kick the client
            Debug.LogWarning($"Client {clientNetworkId} provided wrong password. Kicking...");
            var clientPlayerId = CurrentLobby.PlayerLobbyDataList[1].PlayerId;
            await KickPlayer(clientPlayerId);
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

public struct KickPlayerMessage : INetworkSerializable
{
    public string Reason;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Reason);
    }
}
