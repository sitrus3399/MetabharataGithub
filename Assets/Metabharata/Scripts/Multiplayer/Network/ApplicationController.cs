using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;

    [SerializeField] private ClientSingleton clientSingleton;
    [SerializeField] private HostSingleton hostSingleton;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null); //Untuk cek apakah ada GPU, tidak berlaku untuk mobile
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {

        }
        else
        {
            hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();

            if (authenticated)
            {
                clientSingleton.gameManager.GoToMenu();
            }
        }
    }
}
