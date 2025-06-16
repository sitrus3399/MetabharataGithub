using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using NyxMachina.Shared.EventFramework;
using NyxMachina.Shared.EventFramework.Core.Messenger;
using Unity.Services.Lobbies;

public class LobbyEventManager
{
    private readonly LobbySystem _lobbySystem;
    private readonly LobbyEventCallbacks _lobbyEventCallback;

    public LobbyEventManager(LobbySystem lobbySystem, LobbyEventCallbacks lobbyEventCallback)
    {
        _lobbySystem = lobbySystem;
        _lobbyEventCallback = lobbyEventCallback;
    }

    public void AddLobbyEventListener()
    {
        _lobbyEventCallback.PlayerJoined += OnPlayerJoinLobby;
        _lobbyEventCallback.PlayerLeft += OnPlayerLeftLobby;
        _lobbyEventCallback.KickedFromLobby += OnPlayerKickedFromLobby;
        _lobbyEventCallback.LobbyChanged += OnLobbyChanged;
        _lobbyEventCallback.DataAdded += OnLobbyDataChanged;
        _lobbyEventCallback.DataChanged += OnLobbyDataChanged;
        _lobbyEventCallback.DataRemoved += OnLobbyDataChanged;
        _lobbyEventCallback.PlayerDataAdded += OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataChanged += OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataRemoved += OnLobbyPlayerDataChanged;
        _lobbyEventCallback.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

        EventMessenger.Main.Subscribe<PlayerJoinedLobbyEvent>(OnPlayerJoinedEvent);
    }

    public void RemoveLobbyEventListener()
    {
        _lobbyEventCallback.PlayerJoined -= OnPlayerJoinLobby;
        _lobbyEventCallback.PlayerLeft -= OnPlayerLeftLobby;
        _lobbyEventCallback.KickedFromLobby -= OnPlayerKickedFromLobby;
        _lobbyEventCallback.LobbyChanged -= OnLobbyChanged;
        _lobbyEventCallback.DataAdded -= OnLobbyDataChanged;
        _lobbyEventCallback.DataChanged -= OnLobbyDataChanged;
        _lobbyEventCallback.DataRemoved -= OnLobbyDataChanged;
        _lobbyEventCallback.PlayerDataAdded -= OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataChanged -= OnLobbyPlayerDataChanged;
        _lobbyEventCallback.PlayerDataRemoved -= OnLobbyPlayerDataChanged;
        _lobbyEventCallback.LobbyEventConnectionStateChanged -= OnLobbyEventConnectionStateChanged;

        EventMessenger.Main.Unsubscribe<PlayerJoinedLobbyEvent>(OnPlayerJoinedEvent);
    }

    // Event Handlers (call _lobbySystem's internal methods or move logic here as needed)
    private void OnPlayerJoinedEvent(PlayerJoinedLobbyEvent obj) => _lobbySystem.OnPlayerJoinedEvent(obj);
    private void OnLobbyPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> obj) => _lobbySystem.OnLobbyPlayerDataChanged(obj);
    private void OnLobbyDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj) => _lobbySystem.OnLobbyDataChanged(obj);
    private void OnLobbyChanged(ILobbyChanges obj) => _lobbySystem.OnLobbyChanged(obj);
    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state) => _lobbySystem.OnLobbyEventConnectionStateChanged(state);
    private void OnPlayerKickedFromLobby() => _lobbySystem.OnPlayerKickedFromLobby();
    private void OnPlayerJoinLobby(List<LobbyPlayerJoined> players) => _lobbySystem.OnPlayerJoinLobby(players);
    private void OnPlayerLeftLobby(List<int> playerIndex) => _lobbySystem.OnPlayerLeftLobby(playerIndex);
}