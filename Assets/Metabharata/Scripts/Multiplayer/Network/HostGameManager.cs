using Metabharata.Network.Multiplayer.NetworkServiceSystem;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using NetworkEvent = Unity.Netcode.NetworkEvent;

public class HostGameManager : MonoBehaviour
{
    public static HostGameManager Instance { get; private set; }

    #region Fields and Properties

    private Allocation allocation;
    private string joinCode;
    public Lobby currentLobby;

    private const int maxConnections = 1;
    [SerializeField] private string gameSceneName = "GamePlay";

    private float hostStartTime;


    private int hostRetryCount = 0;
    private const int maxHostRetries = 3;
    private const float retryDelaySeconds = 2f;

    private CancellationTokenSource lobbyPollTokenSource;
    private string lastLobbyPlayerIds = "";
    private HashSet<string> lastLobbyPlayerIdSet = new HashSet<string>();

    // Add a field to store the password (optional, for reference)
    private string lobbyPassword;

    #endregion

    #region Unity Lifecycle

    private async void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (NetworkManager.Singleton is { IsHost: true })
        {
            float timeSinceStart = Time.time - hostStartTime;
            // Debug.Log($"[HOST] Running for {timeSinceStart} seconds");
        }
    }

    #endregion

    #region Host Start Logic

    // Add a new method overload to accept a password
    public async Task StartHostAsync(string password = null)
    {
        lobbyPassword = password;
        hostRetryCount = 0;
        await TryStartHostAsync(password);
    }

    // Update TryStartHostAsync to accept the password
    private async Task TryStartHostAsync(string password = null)
    {
        await NetworkServiceInitiator.Instance.WaitUntilInitializationFinished();

        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            var relayServerData = allocation.ToRelayServerData("dtls");
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport not found on NetworkManager.");
                return;
            }

            transport.OnTransportEvent -= OnTransportEvent;
            transport.OnTransportEvent += OnTransportEvent;

            transport.SetRelayServerData(relayServerData);

            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameManager.Instance.joinCode = joinCode;
            Debug.Log($"Join code: {joinCode}");

            // Create lobby with password option
            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
            {
                { "relay", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
            }
            };

            if (!string.IsNullOrEmpty(password))
            {
                // Store the password with private visibility
                lobbyOptions.Data.Add("password", new DataObject(DataObject.VisibilityOptions.Private, password));
                // Set a public flag indicating the lobby is password protected
                lobbyOptions.Data.Add("hasPassword", new DataObject(DataObject.VisibilityOptions.Public, "true"));
            }
            else
            {
                // Explicitly set hasPassword to false if no password is provided
                lobbyOptions.Data.Add("hasPassword", new DataObject(DataObject.VisibilityOptions.Public, "false"));
            }

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyLobby", maxConnections + 1, lobbyOptions);
            Debug.Log($"Created lobby with Join Code: {currentLobby.LobbyCode}");

            if (NetworkManager.Singleton.StartHost())
            {
                RegisterPasswordCheckHandler();
                Debug.Log("Host started successfully.");
                hostStartTime = Time.time;
                NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("Failed to start host.");
                await RetryStartHostAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Host start error: {ex}");
            await RetryStartHostAsync();
        }
    }

    private void RegisterPasswordCheckHandler()
    {
        //NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("CheckJoinPassword", OnCheckJoinPassword);
    }

    // Update RetryStartHostAsync to pass the password
    private async Task RetryStartHostAsync()
    {
        hostRetryCount++;
        if (hostRetryCount <= maxHostRetries)
        {
            Debug.LogWarning($"Retrying to start host... Attempt {hostRetryCount}/{maxHostRetries}");
            await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
            await TryStartHostAsync(lobbyPassword);
        }
        else
        {
            Debug.LogError("Max host start retries reached. Could not start host.");
        }
    }

    #endregion

    #region Transport Failure Handling

    // Handles transport failure events and triggers retry logic
    private async void OnTransportEvent(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload, float receiveTime)
    {
        if (eventType == NetworkEvent.TransportFailure)
        {
            Debug.LogError("Transport failed to listen. Retrying...");
            await RetryStartHostAsync();
        }
    }

    #endregion

    #region Platform Detection

    public static bool IsMobilePlatform()
    {
        // Runtime platform detection
        return Application.isMobilePlatform || IsRunningOnMobile();
    }

    private static bool IsRunningOnMobile()
    {
#if UNITY_EDITOR
        return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ||
               UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS;
#else
        return false;
#endif
    }

    #endregion
}