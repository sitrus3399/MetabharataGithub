using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;

namespace Metabharata.Multiplayer.Network.LobbySystem
{
    /// <summary>
    /// Encapsulates the current lobby data details, including settings and player data.
    /// </summary>
    public class LobbyContext
    {
        public LobbyWrapper CurrentLobby { get; private set; }
        public List<LobbyWrapper> LobbyList { get; private set; } = new List<LobbyWrapper>();
        public Player CurrentPlayerData { get; set; }
        public JoinAllocation CurrentJoinAllocation { get; set; }

        public void SetCurrentLobby(LobbyWrapper lobby)
        {
            CurrentLobby = lobby;
        }

        public void ClearCurrentLobby()
        {
            CurrentLobby = null;
            CurrentJoinAllocation = null;
        }

        public void SetLobbyList(List<LobbyWrapper> lobbies)
        {
            LobbyList = lobbies ?? new List<LobbyWrapper>();
        }
    }
}