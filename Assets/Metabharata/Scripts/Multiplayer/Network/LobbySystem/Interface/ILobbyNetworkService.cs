using System;
using System.Collections.Generic;

public interface ILobbyNetworkService
{
    public Action OnLobbyChanged { get; set; }
    public Action OnLobbyCreated { get; set; }
    public Action OnPlayerJoined { get; set; }
    public Action OnPlayerLeft { get; set; }

    public void CreateLobby(LobbySetting setting);
    public void JoinLobby(string lobbyId);
    public void LeaveLobby();
    public void SetLobbySettings(LobbySetting setting);
    public void KickPlayer(string playerId);
    public void SetReady(bool isReady);
    public List<IEnumerable<LobbyWrapper>> GetLobbyList();
}
