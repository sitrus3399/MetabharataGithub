using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

/// <summary>
/// Handles player-related operations for the lobby system.
/// </summary>
public class LobbyPlayerManager
{
    private readonly LobbySystem _lobbySystem;
    private readonly LobbyMessageHandler _messageHandler;

    public LobbyPlayerManager(LobbySystem lobbySystem, LobbyMessageHandler messageHandler)
    {
        _lobbySystem = lobbySystem;
        _messageHandler = messageHandler;
    }

    public async Task KickPlayerAsync(string playerId)
    {
        var currentLobby = _lobbySystem.CurrentLobby;
        if (currentLobby == null)
        {
            Debug.LogWarning("No lobby available.");
            return;
        }
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("playerId cannot be null or empty.");
            return;
        }

        try
        {
            ulong? clientId = null;
            foreach (var playerLobbyData in currentLobby.PlayerLobbyDataList)
            {
                if (playerLobbyData.PlayerId == playerId)
                {
                    clientId = playerLobbyData.LocalClientId;
                    break;
                }
            }

            if (clientId.HasValue)
                _messageHandler.SendKickPlayer(clientId.Value, "Kicked by host");

            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, playerId);
            Debug.Log($"Player {playerId} kicked from the lobby.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to kick player: {e}");
        }
    }

    public async Task SetReadyAsync(bool isReady)
    {
        var currentLobby = _lobbySystem.CurrentLobby;
        var currentPlayerId = _lobbySystem.CurrentPlayerId;
        var currentPlayerData = _lobbySystem.CurrentPlayerData;

        if (currentLobby == null)
        {
            Debug.LogWarning("No lobby available.");
            return;
        }
        if (string.IsNullOrEmpty(currentPlayerId))
        {
            Debug.LogWarning("Current player ID is not available.");
            return;
        }
        if (currentPlayerData == null)
        {
            Debug.LogWarning("Current player data is not available.");
            return;
        }

        try
        {
            var playerLobbyData = currentPlayerData.Data.GetPlayerLobbyData();
            playerLobbyData.IsReady = isReady;
            var playerData = new Dictionary<string, PlayerDataObject>
                {
                    { PlayerLobbyData.PlayerLobbyDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerLobbyData) }
                };

            var options = new UpdatePlayerOptions { Data = playerData };
            await LobbyService.Instance.UpdatePlayerAsync(currentLobby.Id, currentPlayerId, options);

            Debug.Log($"Set ready status to {isReady} for player {currentPlayerId}.");
            LobbyChangedEvent.Publish(new LobbyChangedEvent(currentLobby, _lobbySystem.CurrentJoinAllocation, _lobbySystem.IsHost, _lobbySystem.IsClient));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to set ready status...\nLog: {e}");
        }
    }
}
