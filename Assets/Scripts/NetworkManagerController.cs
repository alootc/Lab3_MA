using UnityEngine;
using Unity.Netcode;

public class NetworkManagerController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    // Llamar este método desde un UI Button para iniciar como Host
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

    // Llamar este método desde un UI Button para iniciar como Server
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

    // Llamar este método desde un UI Button para iniciar como Client
    public void StartClient()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started");
        }
    }
}