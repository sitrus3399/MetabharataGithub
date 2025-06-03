using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;

    public HostGameManager gameManager;

    [SerializeField] private HostGameManager hostGameManagerPrefab;

    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) { return instance; }

            instance = FindFirstObjectByType<HostSingleton>();

            if (instance == null)
            {
                instance = new HostSingleton();

                //return null;
            }

            return instance;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost()
    {
        gameManager = Instantiate(hostGameManagerPrefab, this.transform);
        //gameManager = new HostGameManager();

        Debug.Log($"Create Host {gameManager}");
    }
}
