using Metabharata.Multiplayer.Network.LobbySystem;
using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class LobbyWrapper
{
    public static Action<LobbySetting> OnSettingChanged;

    private LobbySetting _lobbySetting;

    public Lobby Lobby { get; }

    public LobbySetting LobbySetting
    {
        get => _lobbySetting;
        set
        {
            if (_lobbySetting == value) return;
            _lobbySetting = value ?? new LobbySetting();
            OnSettingChanged?.Invoke(_lobbySetting);
        }
    }

    public LobbyDataModel LobbyDataModel { get; internal set; }

    public List<PlayerLobbyData> PlayerLobbyDataList = new List<PlayerLobbyData>();

    public string RelayJoinCode => LobbyDataModel.RelayJoinCode;
    public string Id => Lobby?.Id;

    public LobbyWrapper(Lobby lobby)
    {
        Lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
        LobbyDataModel = lobby.Data.GetLobbyDataModel();
        LobbySetting = LobbyDataModel.LobbySetting ?? new LobbySetting();
        PlayerLobbyDataList = LobbyDataModel.PlayerLobbyDataList ?? new List<PlayerLobbyData>();
    }

    public void SetPlayerLobbyDataList(List<PlayerLobbyData> targetList)
    {
        LobbyDataModel.PlayerLobbyDataList = targetList;
    }

    public static implicit operator Lobby(LobbyWrapper wrapper)
    {
        return wrapper?.Lobby;
    }

    public static implicit operator LobbyWrapper(Lobby lobby)
    {
        return lobby != null ? new LobbyWrapper(lobby) : null;
    }
}

public static class LobbyWrapperExtension
{
    public static Player GetHostPlayerData(this Lobby lobby)
    {
        // Get the host player by comparing HostId with Player.Id in Lobby's Players list
        if (lobby?.HostId == null || lobby.Players == null)
            return null;
        foreach (var player in lobby.Players)
        {
            if (player.Id == lobby.HostId)
            {
                return player;
            }
        }
        return null;
    }

    public static Player GetNonHostPlayer(this Lobby lobby)
    {
        // Get the first player that is not the host
        if (lobby?.Players == null || lobby.Players.Count < 2)
            return null;
        foreach (var player in lobby.Players)
        {
            if (player.Id != lobby.HostId)
            {
                return player;
            }
        }
        return null;
    }

    public static Player GetPlayerById(this Lobby lobby, string playerId)
    {
        // Get the player by their ID
        if (lobby?.Players == null)
            return null;
        foreach (var player in lobby.Players)
        {
            if (player.Id == playerId)
            {
                return player;
            }
        }
        return null;
    }

    //public static string ToJson(this List<PlayerLobbyData> target)
    //{
    //    var json = Newtonsoft.Json.JsonConvert.SerializeObject(target);
    //    return json;
    //}

    //public static List<PlayerLobbyData> FromJson(this string json)
    //{
    //    if (string.IsNullOrEmpty(json)) return new List<PlayerLobbyData>();
    //    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PlayerLobbyData>>(json);
    //    return data ?? new List<PlayerLobbyData>();
    //}
}