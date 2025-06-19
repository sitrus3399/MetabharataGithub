using System.Collections.Generic;
using NyxMachina.Shared.EventFramework;
using NyxMachina.Shared.EventFramework.Core.Payloads;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
// ReSharper disable CheckNamespace

public class LobbyChangedEvent : IPayload
{
    public LobbyWrapper CurrentLobby { get; set; }
    public JoinAllocation CurrentJoinAllocation { get; set; }
    public bool IsHost { get; set; }
    public bool IsClient { get; set; }
    public LobbyChangedEvent(LobbyWrapper currentLobby, JoinAllocation currentAllocation, bool isHost, bool isClient)
    {
        CurrentLobby = currentLobby;
        CurrentJoinAllocation = currentAllocation;
        IsHost = isHost;
        IsClient = isClient;
    }

    public static void Publish(LobbyChangedEvent payload)
    {
        EventMessenger.Main.Publish(payload);
    }
    public static LobbyChangedEvent GetState()
    {
        return EventMessenger.Main.GetState<LobbyChangedEvent>();
    }
}

public class LobbyCreatedEvent : IPayload
{
    public LobbyWrapper CurrentLobby { get; set; }
    public JoinAllocation CurrentJoinAllocation { get; set; }
    public LobbyCreatedEvent(LobbyWrapper currentLobby, JoinAllocation currentAllocation)
    {
        CurrentLobby = currentLobby;
        CurrentJoinAllocation = currentAllocation;
    }
    public static void Publish(LobbyCreatedEvent payload)
    {
        EventMessenger.Main.Publish(payload);
    }
    public static LobbyCreatedEvent GetState()
    {
        return EventMessenger.Main.GetState<LobbyCreatedEvent>();
    }
}

public class PlayerJoinedLobbyEvent : IPayload
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public LobbyWrapper JoinedLobby { get; set; }

    public PlayerJoinedLobbyEvent(string playerId, string playerName, LobbyWrapper joinedLobby)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        JoinedLobby = joinedLobby;
    }
    public static void Publish(PlayerJoinedLobbyEvent payload)
    {
        EventMessenger.Main.Publish(payload);
    }
    public static PlayerJoinedLobbyEvent GetState()
    {
        return EventMessenger.Main.GetState<PlayerJoinedLobbyEvent>();
    }
}

public class PlayerLeftLobbyEvent : IPayload
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public LobbyWrapper LastJoinedLobby { get; set; }

    public PlayerLeftLobbyEvent(string playerId, string playerName, LobbyWrapper lastJoinedLobby)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        LastJoinedLobby = lastJoinedLobby;
    }
    public static void Publish(PlayerLeftLobbyEvent payload)
    {
        EventMessenger.Main.Publish(payload);
    }
    public static PlayerLeftLobbyEvent GetState()
    {
        return EventMessenger.Main.GetState<PlayerLeftLobbyEvent>();
    }
}

public class LobbyPlayerDataChangedEvent : IPayload
{
    public LobbyWrapper CurrentLobby { get; }
    public Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> Changes { get; }

    public LobbyPlayerDataChangedEvent(LobbyWrapper currentLobby, Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> changes)
    {
        CurrentLobby = currentLobby;
        Changes = changes;
    }

    public static void Publish(LobbyPlayerDataChangedEvent payload)
    {
        EventMessenger.Main.Publish(payload);
    }

    public static LobbyPlayerDataChangedEvent GetState()
    {
        return EventMessenger.Main.GetState<LobbyPlayerDataChangedEvent>();
    }
}

public class LobbyDataChangedEvent : IPayload
{
    public LobbyWrapper CurrentLobby { get; }
    public Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> Changes { get; }

    public LobbyDataChangedEvent(LobbyWrapper currentLobby, Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> changes)
    {
        CurrentLobby = currentLobby;
        Changes = changes;
    }

    public static void Publish(LobbyDataChangedEvent payload)
    {
        EventMessenger.Main.Publish(payload);
    }

    public static LobbyDataChangedEvent GetState()
    {
        return EventMessenger.Main.GetState<LobbyDataChangedEvent>();
    }
}

public class LobbyConnectionStateChangedEvent : IPayload
{
    public LobbyEventConnectionState State { get; }

    public LobbyConnectionStateChangedEvent(LobbyEventConnectionState state)
    {
        State = state;
    }

    public static void Publish(LobbyConnectionStateChangedEvent payload)
    {
        EventMessenger.Main.Publish(payload);
    }

    public static LobbyConnectionStateChangedEvent GetState()
    {
        return EventMessenger.Main.GetState<LobbyConnectionStateChangedEvent>();
    }
}