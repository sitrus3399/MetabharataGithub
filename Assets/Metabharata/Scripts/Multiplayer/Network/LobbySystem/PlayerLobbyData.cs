using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerLobbyData
{
    public static readonly string PlayerLobbyDataKey = "PlayerLobbyData";

    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public Sprite PlayerAvatar { get; set; }
    public bool IsReady { get; set; }
    public bool IsHost { get; set; }
    public ulong LocalClientId { get; set; }

    public static implicit operator PlayerProfile(PlayerLobbyData data)
    {
        var profile = new PlayerProfile(data.PlayerName);
        return profile;
    }

    public static implicit operator PlayerDataObject(PlayerLobbyData data)
    {
        var playerData = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, data);
        return playerData;
    }

    public static implicit operator string(PlayerLobbyData data)
    {
        return data.ToJson();
    }

    public static implicit operator PlayerLobbyData(string json)
    {
        return json.FromJson();
    }

    public static implicit operator DataObject(PlayerLobbyData data)
    {
        return new DataObject(DataObject.VisibilityOptions.Member, data.ToJson());
    }
}

public static class PlayerLobbyDataExtension
{
    public static PlayerLobbyData GetPlayerLobbyData(this Dictionary<string, PlayerDataObject> data)
    {
        var lobbyData = data.TryGetValue(PlayerLobbyData.PlayerLobbyDataKey, out var value);
        if (lobbyData && value != null)
        {
            return value.Value;
        }
        return new PlayerLobbyData();
    }

    public static string ToJson(this PlayerLobbyData data)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (data == null)
            return string.Empty;

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    public static PlayerLobbyData FromJson(this string json)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (string.IsNullOrEmpty(json))
            return new PlayerLobbyData();

        try
        {
            var data = JsonConvert.DeserializeObject<PlayerLobbyData>(json);
            return data ?? new PlayerLobbyData();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to deserialize PlayerLobbyData from JSON...\n" +
                           $"Returning default value...\n" +
                           $"Log: {e}");
            return new PlayerLobbyData();
        }
    }
}
