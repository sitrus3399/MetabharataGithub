using System;
using UnityEngine;

public class LobbySetting
{
    private int _maxPlayers = 2;

    public int MaxPlayers
    {
        get
        {
            if (_maxPlayers is > 0 and < 2)
            {
                return _maxPlayers;
            }

            Debug.LogWarning("MaxPlayers must be between 1 and 2. Returning default value of 2.");
            return 2; // Default value
        }
        set
        {
            if (value is > 0 and < 2)
            {
                _maxPlayers = value;
                return;
            }

            Debug.LogWarning("MaxPlayers must be between 1 and 2. Setting to default value of 2.");
            _maxPlayers = 2; // Default value
        }
    }

    public string LobbyName = "Default Lobby"; // Name of the lobby
    public bool IsLocked = false; // Indicates if the lobby is locked with a password
    public string Password = ""; // Password to enter when player joins the locked lobby
    public string GameMode;
    public string Map; // Map to be used in the game

    public static bool operator ==(LobbySetting a, LobbySetting b)
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;

        return (a.MaxPlayers, a.LobbyName, a.IsLocked, a.Password, a.GameMode, a.Map) ==
               (b.MaxPlayers, b.LobbyName, b.IsLocked, b.Password, b.GameMode, b.Map);

    }

    public static bool operator !=(LobbySetting a, LobbySetting b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        return this == obj as LobbySetting;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MaxPlayers, LobbyName, IsLocked, Password, GameMode, Map);
    }
}
