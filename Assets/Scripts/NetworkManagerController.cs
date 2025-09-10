using UnityEngine;
using Unity.Netcode;

public class NetworkManagerController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            if (gameManager != null)
            {
                gameManager.SetNetworkReady(true);
            }
            Debug.Log("Host started");
        }
    }

    public void StartServer()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartServer();
            if (gameManager != null)
            {
                gameManager.SetNetworkReady(true);
            }
            Debug.Log("Server started");
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started");
        }
    }
}
