using Metabharata.Multiplayer.Network.LobbySystem;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles all custom network message registration and processing for the lobby system.
/// </summary>
public class LobbyMessageHandler
{
    private readonly LobbySystem _lobbySystem;

    public LobbyMessageHandler(LobbySystem lobbySystem)
    {
        _lobbySystem = lobbySystem;
    }

    #region Message Handler Registration

    /// <summary>
    /// Registers the handler for receiving lobby data.
    /// </summary>
    public void RegisterLobbyDataHandler()
    {
        var manager = NetworkManager.Singleton.CustomMessagingManager;
        manager.UnregisterNamedMessageHandler(nameof(OnReceiveLobbyData));
        manager.RegisterNamedMessageHandler(nameof(OnReceiveLobbyData), OnReceiveLobbyData);
    }

    public void UnregisterLobbyDataHandler()
    {
        if (NetworkManager.Singleton.CustomMessagingManager is null) return;

        var messageManager = NetworkManager.Singleton.CustomMessagingManager;
        messageManager.UnregisterNamedMessageHandler(nameof(OnReceiveLobbyData));
    }

    /// <summary>
    /// Registers the handler for receiving kick player messages.
    /// </summary>
    public void RegisterKickPlayerHandler()
    {
        var manager = NetworkManager.Singleton.CustomMessagingManager;
        manager.UnregisterNamedMessageHandler(nameof(OnReceiveKickPlayer));
        manager.RegisterNamedMessageHandler(nameof(OnReceiveKickPlayer), OnReceiveKickPlayer);
    }

    public void UnregisterKickPlayerHandler()
    {
        if (NetworkManager.Singleton.CustomMessagingManager is null) return;

        var messageManager = NetworkManager.Singleton.CustomMessagingManager;
        messageManager.UnregisterNamedMessageHandler(nameof(OnReceiveKickPlayer));
    }

    /// <summary>
    /// Registers the handler for join password check messages (host only).
    /// </summary>
    public void RegisterPasswordCheckHandler()
    {
        var manager = NetworkManager.Singleton.CustomMessagingManager;
        manager.UnregisterNamedMessageHandler(nameof(OnCheckJoinPassword));
        manager.RegisterNamedMessageHandler(nameof(OnCheckJoinPassword), OnCheckJoinPassword);
    }

    public void UnregisterPasswordCheckHandler()
    {
        if (NetworkManager.Singleton.CustomMessagingManager is null) return;

        var messageManager = NetworkManager.Singleton.CustomMessagingManager;
        messageManager.UnregisterNamedMessageHandler(nameof(OnCheckJoinPassword));
    }

    #endregion

    #region Connection Event Handling

    /// <summary>
    /// Handles connection events and registers necessary message handlers for clients.
    /// </summary>
    public void HandleConnectionEvent(NetworkManager networkManager, ConnectionEventData eventData)
    {
        var isLocalClient = eventData.ClientId == networkManager.LocalClientId;
        var isClientConnected = eventData.EventType == ConnectionEvent.ClientConnected;
        var isClient = _lobbySystem.IsClient;

        if (!isLocalClient || !isClientConnected || !isClient)
            return;

        RegisterLobbyDataHandler();
        RegisterKickPlayerHandler();
        SendJoinPassword();
    }

    /// <summary>
    /// Unregisters message handlers on shutdown.
    /// </summary>
    public void HandlePreShutdown()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (_lobbySystem.IsHost)
            UnregisterPasswordCheckHandler();

        if (_lobbySystem.IsClient)
        {
            UnregisterLobbyDataHandler();
            UnregisterKickPlayerHandler();
        }
    }

    #endregion

    #region Message Sending

    /// <summary>
    /// Sends the join password to the host for validation.
    /// </summary>
    public void SendJoinPassword()
    {
        var password = _lobbySystem.GetUserInputPassword();
        if (string.IsNullOrWhiteSpace(password)) return;

        var msg = new JoinPasswordMessage { Password = password };
        using var writer = new FastBufferWriter(sizeof(int) + (msg.Password?.Length ?? 0) * sizeof(char), Unity.Collections.Allocator.Temp);
        writer.WriteNetworkSerializable(msg);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(OnCheckJoinPassword), NetworkManager.ServerClientId, writer);
    }

    /// <summary>
    /// Sends a kick message to a client.
    /// </summary>
    public void SendKickPlayer(ulong clientId, string reason = "Kicked by host")
    {
        var msg = new KickPlayerMessage { Reason = reason };
        using var writer = new FastBufferWriter(sizeof(int) + (msg.Reason?.Length ?? 0) * sizeof(char), Unity.Collections.Allocator.Temp);
        writer.WriteNetworkSerializable(msg);
        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(nameof(OnReceiveKickPlayer), clientId, writer);
    }

    #endregion

    #region Message Handlers

    /// <summary>
    /// Handles receiving a kick message from the host.
    /// </summary>
    private async void OnReceiveKickPlayer(ulong senderClientId, FastBufferReader reader)
    {
        reader.ReadValueSafe(out KickPlayerMessage msg);
        Debug.LogWarning($"You have been kicked from the lobby. Reason: {msg.Reason}");

        await _lobbySystem.LeaveLobby();
    }

    /// <summary>
    /// Handles receiving updated lobby data from the host.
    /// </summary>
    public void OnReceiveLobbyData(ulong senderClientId, FastBufferReader reader)
    {
        var bytesToRead = reader.Length - reader.Position;
        var data = new byte[bytesToRead];
        reader.ReadBytesSafe(ref data, bytesToRead);
        var json = System.Text.Encoding.UTF8.GetString(data);
        var model = json.ToLobbyDataModel();

        if (_lobbySystem.CurrentLobby != null)
        {
            _lobbySystem.CurrentLobby.LobbyDataModel = model;
            _lobbySystem.CurrentLobby.PlayerLobbyDataList = model.PlayerLobbyDataList;
            _lobbySystem.CurrentLobby.LobbySetting = model.LobbySetting;
        }
    }

    /// <summary>
    /// Handles join password validation on the host.
    /// </summary>
    private async void OnCheckJoinPassword(ulong clientNetworkId, FastBufferReader reader)
    {
        var currentLobbySetting = _lobbySystem.CurrentLobby?.LobbySetting;
        var isNeedPassword = currentLobbySetting?.IsLocked ?? false;
        if (!isNeedPassword)
        {
            Debug.Log($"Client {clientNetworkId} joined without password requirement.");
            return;
        }

        reader.ReadValueSafe(out JoinPasswordMessage msg);
        var correctPassword = _lobbySystem.CurrentLobby?.LobbySetting.Password;
        
        if (_lobbySystem.PasswordHandler.ValidatePassword(msg.Password, correctPassword))
        {
            Debug.Log($"Client {clientNetworkId} joined with correct password.");
        }
        else
        {
            Debug.LogWarning($"Client {clientNetworkId} provided wrong password. Kicking...");
            var clientPlayerId = _lobbySystem.CurrentLobby.PlayerLobbyDataList[1].PlayerId;
            await _lobbySystem.KickPlayer(clientPlayerId);
        }
    }

    #endregion
}

/// <summary>
/// Message for sending a join password from client to host.
/// </summary>
public struct JoinPasswordMessage : INetworkSerializable
{
    public string Password;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
        serializer.SerializeValue(ref Password);
}

/// <summary>
/// Message for sending a kick reason from host to client.
/// </summary>
public struct KickPlayerMessage : INetworkSerializable
{
    public string Reason;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
        serializer.SerializeValue(ref Reason);
}
