using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class LobbyWrapper
{
    public static readonly string RelayJoinCodeKey = "RelayJoinCode";
    public static readonly string LobbyNameKey = "LobbyName";
    public static readonly string MaxPlayersKey = "MaxPlayers";
    public static readonly string IsLockedKey = "IsLocked";
    public static readonly string PasswordKey = "LobbyPassword";
    public static readonly string GameModeKey = "GameMode";
    public static readonly string MapKey = "Map";

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

    public string RelayJoinCode => Lobby?.Data[RelayJoinCodeKey]?.Value;
    public string Id => Lobby?.Id;

    public LobbyWrapper(Lobby lobby)
    {
        Lobby = lobby ?? throw new ArgumentNullException(nameof(lobby));
        LobbySetting = LobbySettingFromLobbyData(lobby.Data);
    }

    private static LobbySetting LobbySettingFromLobbyData(Dictionary<string, DataObject> data)
    {
        var setting = new LobbySetting();
        if (data != null)
        {
            if (data.TryGetValue(LobbyNameKey, out var nameObj))
                setting.LobbyName = nameObj.Value;
            if (data.TryGetValue(MaxPlayersKey, out var maxPlayersObj) && int.TryParse(maxPlayersObj.Value, out var maxPlayers))
                setting.MaxPlayers = maxPlayers;
            if (data.TryGetValue(IsLockedKey, out var isLockedObj) && bool.TryParse(isLockedObj.Value, out var isLocked))
                setting.IsLocked = isLocked;
            if (data.TryGetValue(PasswordKey, out var passwordObj))
                setting.Password = passwordObj.Value;
            if (data.TryGetValue(GameModeKey, out var gameModeObj))
                setting.GameMode = gameModeObj.Value;
            if (data.TryGetValue(MapKey, out var mapObj))
                setting.Map = mapObj.Value;
        }
        return setting;
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

