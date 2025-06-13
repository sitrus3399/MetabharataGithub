using NyxMachina.Shared.EventFramework;
using NyxMachina.Shared.EventFramework.Core.Payloads;
using Unity.Services.Relay.Models;

public class LobbySystemEvent
{
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

    public class PlayerJoinedEvent : IPayload
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public PlayerJoinedEvent(string playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
        public static void Publish(PlayerJoinedEvent payload)
        {
            EventMessenger.Main.Publish(payload);
        }
        public static PlayerJoinedEvent GetState()
        {
            return EventMessenger.Main.GetState<PlayerJoinedEvent>();
        }
    }

    public class PlayerLeftEvent : IPayload
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public PlayerLeftEvent(string playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }
        public static void Publish(PlayerLeftEvent payload)
        {
            EventMessenger.Main.Publish(payload);
        }
        public static PlayerLeftEvent GetState()
        {
            return EventMessenger.Main.GetState<PlayerLeftEvent>();
        }
    }
}
