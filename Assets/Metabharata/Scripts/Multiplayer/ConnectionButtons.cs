using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionButtons : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private void Start()
    {
        startHostButton.onClick.AddListener(() => { StartHost(); });
        startClientButton.onClick.AddListener(() => { StartClient(); });
    }

    public void StartHost()
    {
        Debug.Log($"StartHost");
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        Debug.Log($"StartClient");
        NetworkManager.Singleton.StartClient();
    }
}
