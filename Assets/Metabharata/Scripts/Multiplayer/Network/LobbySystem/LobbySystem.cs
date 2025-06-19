using Metabharata.Multiplayer.Network.LobbySystem;
using Metabharata.Network.Multiplayer.NetworkServiceSystem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    #region Fields & Properties

    public bool IsInitializing { get; set; }
    public bool IsInitialized { get; set; }

    public LobbyWrapper CurrentLobby
    {
        get => _lobbyContext.CurrentLobby;
        set => _lobbyContext.SetCurrentLobby(value);
    }
    public List<LobbyWrapper> LobbyList
    {
        get => _lobbyContext.LobbyList;
        set => _lobbyContext.SetLobbyList(value);
    }
    public Player CurrentPlayerData
    {
        get => _lobbyContext.CurrentPlayerData;
        set => _lobbyContext.CurrentPlayerData = value;
    }
    public JoinAllocation CurrentJoinAllocation
    {
        get => _lobbyContext.CurrentJoinAllocation;
        set => _lobbyContext.CurrentJoinAllocation = value;
    }

    private readonly LobbyEventCallbacks _lobbyEventCallback = new();
    private ILobbyEvents _lobbyEvent;

    private LobbyEventManager _eventManager;
    private LobbyMessageHandler _messageHandler;
    private LobbyDataSyncService _lobbyDataSyncService;
    private readonly LobbyContext _lobbyContext = new();
    private LobbyPlayerManager _playerManager;
    private NetworkRelaySetupService _networkRelaySetupService;
    public readonly LobbyPasswordHandler PasswordHandler = new();

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

    public async void InitializeSystem()
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

        _eventManager = new LobbyEventManager(this, _lobbyEventCallback);
        _eventManager.AddLobbyEventListener();

        _messageHandler = new LobbyMessageHandler(this);
        _playerManager = new LobbyPlayerManager(this, _messageHandler);

        await NetworkServiceInitiator.Instance.WaitUntilInitializationFinished();

        _lobbyDataSyncService = new LobbyDataSyncService(NetworkManager.Singleton);
        _networkRelaySetupService = new NetworkRelaySetupService(NetworkManager.Singleton, _messageHandler);

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
        PasswordHandler.ClearUserInputPassword();

        _eventManager?.RemoveLobbyEventListener();

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
            var (allocation, relayJoinCode) = await _networkRelaySetupService.CreateRelayAllocationAsync(setting.MaxPlayers);
            _networkRelaySetupService.SetupHostTransport(allocation);
            if (_networkRelaySetupService.StartHost())
            {
                _messageHandler.RegisterPasswordCheckHandler();
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

            LobbyCreatedEvent.Publish(new LobbyCreatedEvent(CurrentLobby, CurrentJoinAllocation));
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
        PasswordHandler.SetUserInputPassword(password);

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
            CurrentLobby.PlayerLobbyDataList.Add(clientLobbyData);
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
        PlayerJoinedLobbyEvent.Publish(new PlayerJoinedLobbyEvent(CurrentPlayerId, CurrentPlayerName, CurrentLobby));
    }

    public async Task JoinLobbyByCodeAsync(string joinCode, string password = "")
    {
        if (string.IsNullOrWhiteSpace(joinCode))
        {
            Debug.LogError("Join code cannot be null or empty.");
            return;
        }
        PasswordHandler.SetUserInputPassword(password);

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
            CurrentLobby.PlayerLobbyDataList.Add(clientLobbyData);
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
        PlayerJoinedLobbyEvent.Publish(new PlayerJoinedLobbyEvent(CurrentPlayerId, CurrentPlayerName, CurrentLobby));
    }

    public async Task StartClientAsync(string joinCode)
    {
        CurrentJoinAllocation = await _networkRelaySetupService.JoinRelayAsync(joinCode);
        _networkRelaySetupService.SetupClientTransport(CurrentJoinAllocation);
        if (_networkRelaySetupService.StartClient())
            Debug.Log("Client started successfully.");
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

        var lastLobby = CurrentLobby;
        CurrentLobby = null;
        CurrentJoinAllocation = null;

        PlayerLeftLobbyEvent.Publish(new PlayerLeftLobbyEvent(CurrentPlayerId, CurrentPlayerName, lastLobby));
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
            LobbyChangedEvent.Publish(new LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
            Debug.Log("Lobby settings updated successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to update lobby settings: {e}");
        }
    }

    public Task KickPlayer(string playerId) => _playerManager.KickPlayerAsync(playerId);

    public Task SetReady(bool isReady) => _playerManager.SetReadyAsync(isReady);

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
            LobbyChangedEvent.Publish(new LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start the game: {e}");
        }
    }

    #endregion

    #region Event Handlers (called by LobbyEventManager)

    internal void OnPlayerJoinedEvent(PlayerJoinedLobbyEvent obj)
    {
        // TODO: Kicked the player in here, not OnConnected NetworkManager
    }

    internal void OnLobbyPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj)
    {
        SendCurrentLobbyDataToServer();
        Debug.Log("Lobby player data changed.");

        LobbyPlayerDataChangedEvent.Publish(new LobbyPlayerDataChangedEvent(CurrentLobby, obj));

        LobbyChangedEvent.Publish(new LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient));
    }

    // Replace the original check in OnLobbyDataChanged with the new method
    internal void OnLobbyDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
    {
        if (HasValidLobbyDataModel())
        {
            var lobbyDataObj = CurrentLobby.Lobby.Data[LobbyDataModel.LobbyDataModelKey];
            var model = _lobbyDataSyncService.DeserializeLobbyData(lobbyDataObj.Value);
            if (model != null)
            {
                CurrentLobby.PlayerLobbyDataList = model.PlayerLobbyDataList;
                CurrentLobby.LobbySetting = model.LobbySetting;
            }
        }
        Debug.Log("Lobby data changed.");

        LobbyDataChangedEvent.Publish(
            new LobbyDataChangedEvent(CurrentLobby, obj)
        );

        LobbyChangedEvent.Publish(
            new LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient)
        );
        return;
    }

    internal void OnLobbyChanged(ILobbyChanges obj)
    {
        SendCurrentLobbyDataToServer();
        Debug.Log("Lobby changed (general).");

        LobbyChangedEvent.Publish(
            new LobbyChangedEvent(CurrentLobby, CurrentJoinAllocation, IsHost, IsClient)
        );
    }

    internal void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        Debug.Log($"Lobby event connection state changed: {state}");
        if (state == LobbyEventConnectionState.Unsynced)
            Debug.LogWarning("Lost connection to lobby events. You may need to rejoin or refresh.");

        LobbyConnectionStateChangedEvent.Publish(
            new LobbyConnectionStateChangedEvent(state)
        );

        switch (state)
        {
            case LobbyEventConnectionState.Subscribed: break;
            case LobbyEventConnectionState.Unsubscribed: break;
        }
    }

    internal void OnPlayerKickedFromLobby()
    {
        Debug.LogWarning("You have been kicked from the lobby.");

        var lastLobby = CurrentLobby;
        CurrentLobby = null;
        CurrentJoinAllocation = null;
        _lobbyEvent?.UnsubscribeAsync();
        _lobbyEvent = null;
        //PlayerLeftLobbyEvent.Publish(new PlayerLeftLobbyEvent(CurrentPlayerId, CurrentPlayerName, lastLobby));
    }

    internal void OnPlayerJoinLobby(List<LobbyPlayerJoined> players)
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
        PlayerJoinedLobbyEvent.Publish(new PlayerJoinedLobbyEvent(CurrentPlayerId, CurrentPlayerName, CurrentLobby));
    }

    internal void OnPlayerLeftLobby(List<int> playerIndex)
    {
        var lastLobby = CurrentLobby;
        var leftPlayerId = string.Empty;
        var leftPlayerName = string.Empty;
        if (CurrentLobby is { PlayerLobbyDataList: not null })
        {
            playerIndex.Sort((a, b) => b.CompareTo(a));
            foreach (var index in playerIndex)
            {
                if (index >= 0 && index < CurrentLobby.PlayerLobbyDataList.Count)
                {
                    var leftPlayer = CurrentLobby.PlayerLobbyDataList[index];
                    leftPlayerId = leftPlayer.PlayerId;
                    leftPlayerName = leftPlayer.PlayerName;
                    CurrentLobby.PlayerLobbyDataList.RemoveAt(index);
                }
                else
                {
                    Debug.LogWarning($"Invalid player index {index} in OnPlayerLeftLobby.");
                }
            }
        }
        SendCurrentLobbyDataToServer();
        PlayerLeftLobbyEvent.Publish(new PlayerLeftLobbyEvent(leftPlayerId, leftPlayerName, lastLobby));
        Debug.Log($"Player left: \nId:'{leftPlayerId}' \nName:'{leftPlayerName}'");
    }

    #endregion

    #region Helpers

    private void SendCurrentLobbyDataToServer()
    {
        if (!IsHost || CurrentLobby?.LobbyDataModel == null)
            return;

        _lobbyDataSyncService.SendLobbyDataToClients(
            CurrentLobby.LobbyDataModel,
            NetworkManager.Singleton.LocalClientId,
            nameof(LobbyMessageHandler.OnReceiveLobbyData)
        );
    }

    internal string GetUserInputPassword() => PasswordHandler.GetUserInputPassword();

    private bool HasValidLobbyDataModel()
    {
        return CurrentLobby is { Lobby: { Data: not null } } &&
               CurrentLobby.Lobby.Data.TryGetValue(LobbyDataModel.LobbyDataModelKey, out var lobbyDataObj) &&
               !string.IsNullOrWhiteSpace(lobbyDataObj.Value);
    }

    #endregion
}