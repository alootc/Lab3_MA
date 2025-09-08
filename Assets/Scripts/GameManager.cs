using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private static GameManager Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject buffPrefab;
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public float buffSpawnInterval = 4f;
    public float enemySpawnInterval = 8f;
    public int maxEnemies = 3;
    public float enemySpawnRadius = 10f;

    private float buffTimer = 0f;
    private float enemyTimer = 0f;
    private int currentEnemies = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isNetworkReady = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("Player Prefab: " + (playerPrefab != null ? "Assigned" : "NULL"));
            Debug.Log("Buff Prefab: " + (buffPrefab != null ? "Assigned" : "NULL"));
            Debug.Log("Enemy Prefab: " + (enemyPrefab != null ? "Assigned" : "NULL"));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isNetworkReady) return;
        if (!IsServer) return;

        HandleBuffSpawning();
        HandleEnemySpawning();
    }

    // Método para verificar cuando la red está lista
    public void SetNetworkReady(bool ready)
    {
        isNetworkReady = ready;
        Debug.Log("Network ready: " + ready);
    }

    private void HandleBuffSpawning()
    {
        buffTimer += Time.deltaTime;
        if (buffTimer >= buffSpawnInterval)
        {
            SpawnBuff();
            buffTimer = 0f;
        }
    }

    private void HandleEnemySpawning()
    {
        enemyTimer += Time.deltaTime;
        if (enemyTimer >= enemySpawnInterval && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            enemyTimer = 0f;
        }
    }

    void SpawnBuff()
    {
        if (buffPrefab == null)
        {
            Debug.LogError("Buff prefab is not assigned in GameManager!");
            return;
        }

        // Verificar que NetworkManager esté escuchando
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager is not listening. Cannot spawn buff.");
            return;
        }

        Vector3 randomPosition = new Vector3(Random.Range(-8, 8), 0.5f, Random.Range(-8, 8));
        GameObject buff = Instantiate(buffPrefab, randomPosition, Quaternion.identity);

        NetworkObject networkObject = buff.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
            Debug.Log("Buff spawned at position: " + randomPosition);
        }
        else
        {
            Debug.LogError("Instantiated buff is missing NetworkObject component!");
            Destroy(buff);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned in GameManager!");
            return;
        }

        // Verificar que NetworkManager esté escuchando
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager is not listening. Cannot spawn enemy.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
        if (enemyNetObj != null)
        {
            enemyNetObj.Spawn(true);

            Enemy enemyComponent = enemy.GetComponent<Enemy>();
            if (enemyComponent != null)
            {
                enemyComponent.SetGameManager(this);
            }
        }

        activeEnemies.Add(enemy);
        currentEnemies++;
        Debug.Log("Enemy spawned. Total: " + currentEnemies);
    }

    Vector3 GetValidSpawnPosition()
    {
        Vector3 position;
        int attempts = 0;
        const int maxAttempts = 20;

        do
        {
            position = new Vector3(
                Random.Range(-enemySpawnRadius, enemySpawnRadius), 0.5f,
                Random.Range(-enemySpawnRadius, enemySpawnRadius)
            );
            attempts++;
        }
        while (!IsPositionValid(position) && attempts < maxAttempts);

        return position;
    }

    bool IsPositionValid(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 3f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                return false;
            }
        }
        return true;
    }

    public void EnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            currentEnemies--;
            Debug.Log($"Enemy destroyed. Total: {currentEnemies}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                NetworkObject netObj = enemy.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Despawn();
                }
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        currentEnemies = 0;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("GameManager spawned on server");
            SetNetworkReady(true);
            SpawnPlayers();
        }
    }

    private void SpawnPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerRpc(clientId);
        }
    }

    [Rpc(SendTo.Server)]
    public void SpawnPlayerRpc(ulong ownerID)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not assigned!");
            return;
        }

        GameObject player = Instantiate(playerPrefab);

        SimplePlayerController playerController = player.GetComponent<SimplePlayerController>();
        if (playerController != null)
        {
            playerController.PlayerID.Value = ownerID;
        }

        NetworkObject netObj = player.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.SpawnWithOwnership(ownerID, true);
            Debug.Log("Player spawned for client: " + ownerID);
        }
        else
        {
            Debug.LogError("Player prefab is missing NetworkObject component!");
            Destroy(player);
        }
    }

    public static GameManager GetInstance() { return Instance; }
}