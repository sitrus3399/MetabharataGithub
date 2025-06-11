using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientGameManager : MonoBehaviour
{
    public static ClientGameManager Instance { get; private set; }

    private JoinAllocation allocation;

    private const string MenuSceneName = "MainMenu";
    [SerializeField] private string gameSceneName = "GamePlay";

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

        transport.SetRelayServerData(allocation.ToRelayServerData("dtls"));

        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started successfully.");
            // Do NOT call LoadScene here. The server will handle scene changes.
        }
        else
        {
            Debug.LogError("Failed to start client.");
        }
    }
}