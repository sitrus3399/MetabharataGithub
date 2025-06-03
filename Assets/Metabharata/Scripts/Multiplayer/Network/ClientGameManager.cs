using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : MonoBehaviour
{
    private JoinAllocation allocation;

    private const string MenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GamePlay";

    public async Task<bool> InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }

    public async Task StartClientAsync(string joinCode)
    {
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (System.Exception exception)
        {
            Debug.Log($"ERROR StartClient joinCode {joinCode} {exception}");
            throw;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            allocation.HostConnectionData, // Menggunakan ConnectionData sebagai HostConnectionData
            isSecure: true
        );

        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully.");
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("Failed to start client.");
        }


        //NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single); //Go To Gameplay
    }
}