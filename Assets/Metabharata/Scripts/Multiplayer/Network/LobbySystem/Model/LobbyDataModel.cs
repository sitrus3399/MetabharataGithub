using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Metabharata.Multiplayer.Network.LobbySystem
{
    /// <summary>
    /// Data model for storing lobby settings and player lobby data list.
    /// </summary>
    public class LobbyDataModel
    {
        public static readonly string LobbyDataModelKey = "LobbyData";

        public LobbySetting LobbySetting { get; set; } = new();
        public List<PlayerLobbyData> PlayerLobbyDataList { get; set; } = new();
        public string RelayJoinCode { get; set; } = string.Empty;

        public static implicit operator string(LobbyDataModel model)
        {
            return model.ToJson();
        }

        public static implicit operator LobbyDataModel(string dataJson)
        {
            return dataJson.ToLobbyDataModel();
        }

        public static implicit operator DataObject(LobbyDataModel model)
        {
            return new DataObject(DataObject.VisibilityOptions.Member, model.ToJson());
        }
    }

    public static class LobbyDataModelExtension
    {
        public static LobbyDataModel GetLobbyDataModel(this Dictionary<string, DataObject> data)
        {
            var lobbyData = data.TryGetValue(LobbyDataModel.LobbyDataModelKey, out var value);
            if (lobbyData && value != null)
            {
                return value.Value.ToLobbyDataModel();
            }
            return new LobbyDataModel();
        }

        /// <summary>
        /// Serializes the LobbyDataModel to a JSON string.
        /// </summary>
        public static string ToJson(this LobbyDataModel data)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (data == null)
                return string.Empty;

            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }

        /// <summary>
        /// Deserializes a JSON string to a LobbyDataModel instance.
        /// Returns a new LobbyDataModel if input is null or invalid.
        /// </summary>
        public static LobbyDataModel ToLobbyDataModel(this string dataJson)
        {
            if (string.IsNullOrWhiteSpace(dataJson))
                return new LobbyDataModel();

            try
            {
                var model = JsonConvert.DeserializeObject<LobbyDataModel>(dataJson);
                return model ?? new LobbyDataModel();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize PlayerLobbyData from JSON...\n" +
                               $"Returning default value...\n" +
                               $"Log: {e}");
                return new LobbyDataModel();
            }
        }
    }
}