using System.Net;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : MonoBehaviour
{
    private Allocation allocation;
    private string joinCode;

    [SerializeField] private const int maxConnections = 1;
    [SerializeField] private string gameSceneName = "GamePlay";

    private float hostStartTime;

    public async Task InitAsync()
    {
        // Authenticate Player

        //return Task.CompletedTask;
    }

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        }
        catch (System.Exception exception)
        {
            Debug.Log($"ERROR CreateAllocation {exception}");
            throw;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameManager.Instance.joinCode = joinCode;
            Debug.Log($"Joincode {joinCode}");
        }
        catch (System.Exception exception)
        {
            Debug.Log($"ERROR GetJoinCOde {exception}");
            throw;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("UnityTransport not found on NetworkManager.");
            return;
        }

        //transport.SetRelayServerData(serverData);

        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            //allocation.ConnectionData, // Menggunakan ConnectionData sebagai HostConnectionData
            isSecure: true
        );

        //NetworkManager.Singleton.StartHost();
        //NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single); //Go To Gameplay

        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started successfully.");
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }

        hostStartTime = Time.time;
    }

    public static bool isMobilePlatform()
    {
        // Deteksi platform runtime
        return Application.isMobilePlatform ||
               IsRunningOnMobile();
    }

    private static bool IsRunningOnMobile()
    {
        // Deteksi tambahan untuk editor Unity
        #if UNITY_EDITOR
        return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ||
               UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS;
        #else
            return false;
        #endif
    }

    void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            float timeSinceStart = Time.time - hostStartTime;
            Debug.Log($"[HOST] still running");
            //Debug.Log($"[HOST] Running for {timeSinceStart} seconds");
        }
    }
}