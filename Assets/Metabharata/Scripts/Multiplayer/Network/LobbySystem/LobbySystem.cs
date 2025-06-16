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
    #region Fields & Properties

    public bool IsInitializing { get; set; }
    public bool IsInitialized { get; set; }
    public List<LobbyWrapper> LobbyList { get; set; }
    public LobbyWrapper CurrentLobby { get; set; }
    public JoinAllocation CurrentJoinAllocation { get; set; }
    public Player CurrentPlayerData;

    private readonly LobbyEventCallbacks _lobbyEventCallback = new();
    private ILobbyEvents _lobbyEvent;
    private string _userInputPassword;

    public string CurrentPlayerId =>
        NetworkServiceInitiator.Instance.IsInitialized ? AuthenticationService.Instance.PlayerId : string.Empty;

    public string CurrentPlayerName
    {
        get
        {
            if (!NetworkServiceInitiator.Instance.IsInitialized)
                return string.Empty;
            var playerName = AuthenticationService.Instance?.PlayerName;
            return string.IsNullOrWhiteSpace(playerName) ? "Guest" : playerName;
        }
    }

    public bool IsHost => CurrentLobby != null && CurrentLobby.Lobby.HostId == CurrentPlayerId;
    public bool IsClient => !IsHost;

    #endregion

    #region Public API

    public void InitializeSystem()
    {
        if (IsInitialized || IsInitializing)
        {
            Debug.LogWarning("Lobby system is already initialized or initializing.");
            return;
        }

        IsInitializing = true;
        LobbyList = new List<LobbyWrapper>();
        IsInitialized = true;
        IsInitializing = false;

        AddLobbyEventListener();

        EventMessenger.Main.Subscribe<LobbySystemEvent.PlayerJoinedEvent>(OnPlayerJoinedEvent);
        Debug.Log("Lobby system initialized.");
    }

    public void Shutdown()
    {
        if (CurrentLobby != null)
        {
            try
            {
                LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, CurrentPlayerId).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Error while leaving lobby during shutdown: {e}");
            }
            CurrentLobby = null;
        }

        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            NetworkManager.Singleton.Shutdown();

        CurrentJoinAllocation = null;
        LobbyList = null;
        IsInitialized = false;
        IsInitializing = false;
        _userInputPassword = null;

        RemoveLobbyEventListener();

        Debug.Log("Lobby system shutdown complete.");
    }

    public async Task<LobbyWrapper> CreateLobby(LobbySetting setting = null)
    {
        setting ??= new LobbySetting
        {
            LobbyName = "DefaultLobby",
            MaxPlayers = 2,
            IsLocked = false,
            Password = string.Empty,
            GameMode = "Casual",
            Map = "DefaultMap"
        };

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(setting.MaxPlayers);
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            var relayServerData = allocation.ToRelayServerData("dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.OnConnectionEvent -= HandleOnConnectionEvent;
            NetworkManager.Singleton.OnConnectionEvent += HandleOnConnectionEvent;
            NetworkManager.Singleton.OnPreShutdown -= PreShutdownHandle;
            NetworkManager.Singleton.OnPreShutdown += PreShutdownHandle;

            if (NetworkManager.Singleton.StartHost())
            {
                RegisterPasswordCheckHandler();
                Debug.Log("Host started...");
            }

            var hostLobbyData = new PlayerLobbyData
            {
                PlayerId = CurrentPlayerId,
                PlayerName = CurrentPlayerName,
                IsReady = false,
                IsHost = true,
                LocalClientId = NetworkManager.Singleton.LocalClientId
            };

            var playerLobbyData = new Dictionary<string, PlayerDataObject>
            {
                { PlayerLobbyData.PlayerLobbyDataKey, hostLobbyData }
            };

            var playerLobbyDataList = new List<PlayerLobbyData> { hostLobbyData };

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
                Player = new Player
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

            CurrentLobby = new LobbyWrapper(lobby);
            _lobbyEvent = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEventCallback);

            LobbySystemEvent.LobbyCreatedEvent.Publish(new LobbySystemEvent.LobbyCreatedEvent(CurrentLobby, CurrentJoinAllocation));
            Debug.Log($"Lobby '{setting.LobbyName}' created successfully with code: {lobby.LobbyCode}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create lobby...\nLog: {e}");
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
            IsReady = false,
            IsHost = false,
            LocalClientId = NetworkManager.Singleton.LocalClientId + 1
        };

        var joinLobbyOptions = new JoinLobbyByIdOptions
        {
            Player = new Player
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

        CurrentPlayerData = joinLobbyOptions.Player;

        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyOptions);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to lobby...\nLog: {e}");
            return;
        }

        try
        {
            _lobbyEvent = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEventCallback);
        }
        catch (LobbyServiceException e)
        {
            switch (e.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby:
                    Debug.LogWarning($"Already subscribed to lobby[{CurrentLobby.Id}]. Exception Message: {e.Message}");
                    break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                    Debug.LogError($"Subscription to lobby events was lost. Exception Message: {e.Message}");
                    throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError:
                    Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}");
                    throw;
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
            IsReady = false,
            IsHost = false,
            LocalClientId = NetworkManager.Singleton.LocalClientId + 1
        };

        var joinLobbyOption = new JoinLobbyByCodeOptions
        {
            Player = new Player
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

        CurrentPlayerData = joinLobbyOption.Player;

        try
        {
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, joinLobbyOption);
            SendCurrentLobbyDataToServer();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to connect to lobby...\nLog: {e}");
            return;
        }

        try
        {
            _lobbyEvent = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, _lobbyEventCallback);
        }
        catch (LobbyServiceException e)
        {
            switch (e.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby:
                    Debug.LogWarning($"Already subscribed to lobby[{CurrentLobby.Id}]. Exception Message: {e.Message}");
                    break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                    Debug.LogError($"Subscription to lobby events was lost. Exception Message: {e.Message}");
                    throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError:
                    Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}");
                    throw;
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
            Debug.Log($"Failed to Join Relay...\nLog: {e}");
            throw;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(CurrentJoinAllocation.ToRelayServerData("dtls"));

        NetworkManager.Singleton.OnConnectionEvent -= HandleOnConnectionEvent;
        NetworkManager.Singleton.OnConnectionEvent += HandleOnConnectionEvent;
        NetworkManager.Singleton.OnPreShutdown -= PreShutdownHandle;
        NetworkManager.Singleton.OnPreShutdown += PreShutdownHandle;

        try
        {
            if (NetworkManager.Singleton.StartClient())
                Debug.Log("Client started successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start client...\nLog:  {e}");
            throw;
        }
    }

    public async Task LeaveLobby(string playerId = null)
    {
        playerId ??= CurrentPlayerId;

        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby to leave.");
            return;
        }

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, playerId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to leave lobby: {e}");
            return;
        }

        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsConnectedClient)
            NetworkManager.Singleton.Shutdown();

        if (NetworkManager.Singleton.IsHost)
            NetworkManager.Singleton.Shutdown();

        CurrentLobby = null;
        CurrentJoinAllocation = null;

        LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, CurrentPlayerName));
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

            var options = new UpdateLobbyOptions { Data = data };
            var updatedLobby = await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);

            CurrentLobby = new LobbyWrapper(updatedLobby);

            LobbyWrapper.OnSettingChanged?.Invoke(setting);
            LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
            Debug.Log("Lobby settings updated successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update lobby settings: {e}");
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
            ulong? clientId = null;
            foreach (var playerLobbyData in CurrentLobby.PlayerLobbyDataList)
            {
                if (playerLobbyData.PlayerId == playerId)
                {
                    clientId = playerLobbyData.LocalClientId;
                    break;
                }
            }

            if (clientId.HasValue)
                SendKickPlayerMessage(clientId.Value, "Kicked by host");

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

            var options = new UpdatePlayerOptions { Data = playerData };
            await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, CurrentPlayerId, options);

            Debug.Log($"Set ready status to {isReady} for player {CurrentPlayerId}.");
            LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set ready status...\nLog: {e}");
        }
    }

    public async void StartGame()
    {
        if (CurrentLobby == null)
        {
            Debug.LogWarning("No lobby available.");
            return;
        }
        if (CurrentLobby.Lobby.HostId != CurrentPlayerId)
        {
            Debug.LogWarning("Only the host can start the game.");
            return;
        }

        try
        {
            var data = new Dictionary<string, DataObject>
            {
                { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, true.ToString()) }
            };

            var options = new UpdateLobbyOptions { Data = data };
            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);

            Debug.Log("Game started! Notified all players via lobby data.");
            LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start the game: {e}");
        }
    }

    public bool IsSystemReady() => IsInitialized;

    #endregion

    #region Event Handlers

    private void OnPlayerJoinedEvent(LobbySystemEvent.PlayerJoinedEvent obj)
    {
        // TODO: Kicked the player in here, not OnConnected NetworkManager
    }

    private void OnLobbyPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj)
    {
        SendCurrentLobbyDataToServer();
        Debug.Log("Lobby player data changed.");
        LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
    }

    private void OnLobbyDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
    {
        if (CurrentLobby?.Lobby?.Data != null)
        {
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
        SendCurrentLobbyDataToServer();
        Debug.Log("Lobby changed (general).");
        LobbySystemEvent.LobbyChangedEvent.Publish(new LobbySystemEvent.LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        Debug.Log($"Lobby event connection state changed: {state}");
        if (state == LobbyEventConnectionState.Unsynced)
            Debug.LogWarning("Lost connection to lobby events. You may need to rejoin or refresh.");

        switch (state)
        {
            case LobbyEventConnectionState.Subscribed: break;
            case LobbyEventConnectionState.Unsubscribed: RemoveLobbyEventListener(); break;
        }
    }

    private void OnPlayerKickedFromLobby()
    {
        Debug.LogWarning("You have been kicked from the lobby.");
        CurrentLobby = null;
        CurrentJoinAllocation = null;
        LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, CurrentPlayerName));
        _lobbyEvent.UnsubscribeAsync();
        _lobbyEvent = null;
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
        SendCurrentLobbyDataToServer();
        LobbySystemEvent.PlayerJoinedEvent.Publish(new LobbySystemEvent.PlayerJoinedEvent(CurrentPlayerId, CurrentPlayerName));
    }

    private void OnPlayerLeftLobby(List<int> playerIndex)
    {
        if (CurrentLobby is { PlayerLobbyDataList: not null })
        {
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

    #endregion

    #region Lobby Event Listener Registration

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

    #endregion

    #region Network Message Handlers

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

    private void HandleOnConnectionEvent(NetworkManager networkManager, ConnectionEventData eventData)
    {
        var isLocalClient = eventData.ClientId == networkManager.LocalClientId;
        var isClientConnected = eventData.EventType == ConnectionEvent.ClientConnected;
        var isClient = IsClient;

        if (!isLocalClient || !isClientConnected || !isClient) 
            return;

        RegisterLobbyDataHandler();
        RegisterKickPlayerHandler();
        SendPassword();
    }

    private void PreShutdownHandle()
    {
        if (NetworkManager.Singleton == null) 
            return;

        if (IsHost)
            UnregisterPasswordCheckHandler();

        if (IsClient)
        {
            UnregisterLobbyDataHandler();
            UnregisterKickPlayerHandler();
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

    private async void ReceiveKickPlayer(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out KickPlayerMessage msg);
        Debug.LogWarning($"You have been kicked from the lobby. Reason: {msg.Reason}");

        await LeaveLobby();

        CurrentLobby = null;
        CurrentJoinAllocation = null;
        LobbySystemEvent.PlayerLeftEvent.Publish(new LobbySystemEvent.PlayerLeftEvent(CurrentPlayerId, CurrentPlayerName));
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsConnectedClient)
            NetworkManager.Singleton.Shutdown();

        if (NetworkManager.Singleton.IsHost)
            NetworkManager.Singleton.Shutdown();
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
        var correctPassword = CurrentLobby?.LobbySetting.Password;

        if (string.IsNullOrEmpty(correctPassword) || msg.Password == correctPassword)
        {
            Debug.Log($"Client {clientNetworkId} joined with correct password.");
        }
        else
        {
            Debug.LogWarning($"Client {clientNetworkId} provided wrong password. Kicking...");
            var clientPlayerId = CurrentLobby.PlayerLobbyDataList[1].PlayerId;
            await KickPlayer(clientPlayerId);
        }
    }

    #endregion

    #region Helpers

    private byte[] SerializeLobbyDataModel(LobbyDataModel model) =>
        System.Text.Encoding.UTF8.GetBytes(model.ToJson());

    private void SendCurrentLobbyDataToServer()
    {
        if (!IsHost || CurrentLobby?.LobbyDataModel == null)
            return;

        var data = SerializeLobbyDataModel(CurrentLobby.LobbyDataModel);

        using var writer = new FastBufferWriter(data.Length, Unity.Collections.Allocator.Temp);
        writer.WriteBytesSafe(data, data.Length);

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId) continue;
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(ReceiveLobbyData), clientId, writer);
        }
    }

    #endregion
}

public struct JoinPasswordMessage : INetworkSerializable
{
    public string Password;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
        serializer.SerializeValue(ref Password);
}

public struct KickPlayerMessage : INetworkSerializable
{
    public string Reason;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
        serializer.SerializeValue(ref Reason);
}
