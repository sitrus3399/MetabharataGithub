using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;

    public ClientGameManager gameManager {  get; private set; }

    [SerializeField] private ClientGameManager clientGameManagerPrefab;

    public static ClientSingleton Instance
    { 
        get 
        {
            if (instance != null) { return instance; }

            instance = FindFirstObjectByType<ClientSingleton>();

            if (instance == null)
            {
                instance = new ClientSingleton();

                //return null;
            }
            
            return instance;
        } 
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    public async Task<bool> CreateClient()
    {
        //gameManager = new ClientGameManager();

        gameManager = Instantiate(clientGameManagerPrefab, this.transform);

        return await gameManager.InitAsync();
    }
}
