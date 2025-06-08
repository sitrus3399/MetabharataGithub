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
    private Lobby currentLobby;

    private const int maxConnections = 1;
    [SerializeField] private string gameSceneName = "GamePlay";

    private float hostStartTime;


    private int hostRetryCount = 0;
    private const int maxHostRetries = 3;
    private const float retryDelaySeconds = 2f;

    private CancellationTokenSource lobbyPollTokenSource;
    private string lastLobbyPlayerIds = "";
    private HashSet<string> lastLobbyPlayerIdSet = new HashSet<string>();

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
        await InitAsync();
    }

    private void Update()
    {
        if (NetworkManager.Singleton is {IsHost: true})
        {
            float timeSinceStart = Time.time - hostStartTime;
            // Debug.Log($"[HOST] Running for {timeSinceStart} seconds");
        }
    }

    #endregion

    #region Initialization

    public async Task InitAsync()
    {
        // TODO: Authenticate Player

        if (!NetworkServiceSystem.Instance.IsSystemReady())
        {
            await NetworkServiceSystem.Instance.InitializeSystemAsync();
        }
    }

    #endregion

    #region Host Start Logic

    public async Task StartHostAsync()
    {
        hostRetryCount = 0;
        await TryStartHostAsync();
    }

    private async Task TryStartHostAsync()
    {
        await WaitForNetworkServiceInitialization();

        try
        {
            // Allocate relay with maxConnections
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            // Set relay server data using the new RelayServerData object
            var relayServerData = allocation.ToRelayServerData("dtls");
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport not found on NetworkManager.");
                return;
            }

            // Subscribe to transport failure event (unsubscribe first to avoid duplicates)
            transport.OnTransportEvent -= OnTransportEvent;
            transport.OnTransportEvent += OnTransportEvent;

            transport.SetRelayServerData(relayServerData);

            // Get join code and update lobby if needed
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameManager.Instance.joinCode = joinCode;
            Debug.Log($"Join code: {joinCode}");

            // Example: Update lobby with join code if you have a lobby system
            if (currentLobby != null)
            {
                currentLobby.Data["relay"] = new DataObject(DataObject.VisibilityOptions.Public, joinCode, 0);
                await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions()
                {
                    Data = currentLobby.Data
                });
            }

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started successfully.");
                hostStartTime = Time.time;

                // Load the GamePlay scene as host
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

    private async Task WaitForNetworkServiceInitialization()
    {
        if (!NetworkServiceSystem.Instance.IsInitialized)
        {
            var cancellationToken = new CancellationTokenSource(5000);
            while (UnityServices.State == ServicesInitializationState.Initializing)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new Exception()
                    {
                        Source = "Request Timed Out when waiting for initialization..."
                    };
                }
                await Task.Yield();
            }
        }
    }

    private async Task RetryStartHostAsync()
    {
        hostRetryCount++;
        if (hostRetryCount <= maxHostRetries)
        {
            Debug.LogWarning($"Retrying to start host... Attempt {hostRetryCount}/{maxHostRetries}");
            await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
            await TryStartHostAsync();
        }
        else
        {
            Debug.LogError("Max host start retries reached. Could not start host.");
            // Optionally notify user or take further action
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

    public static bool isMobilePlatform()
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