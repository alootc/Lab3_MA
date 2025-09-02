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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null) return;

        if (IsServer)
        {
            HandleBuffSpawning();
            HandleEnemySpawning();
        }
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
        if (buffPrefab == null) return;

        Vector3 randomPosition = new Vector3(Random.Range(-8, 8), 0.5f, Random.Range(-8, 8));
        GameObject buff = Instantiate(buffPrefab, randomPosition, Quaternion.identity);
        buff.GetComponent<NetworkObject>().Spawn(true);
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        NetworkObject enemyNetObj = enemy.GetComponent<NetworkObject>();
        if (enemyNetObj != null)
        {
            enemyNetObj.Spawn(true);
            enemy.GetComponent<Enemy>().SetGameManager(this);
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
                enemy.GetComponent<NetworkObject>().Despawn();
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
        currentEnemies = 0;
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton != null && IsServer)
        {
            Debug.Log("Connected players: " + NetworkManager.Singleton.ConnectedClients.Count);
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
        player.GetComponent<SimplePlayerController>().PlayerID.Value = ownerID;
        player.GetComponent<NetworkObject>().SpawnWithOwnership(ownerID, true);

        Debug.Log("Player spawned for client: " + ownerID);
    }

    public static GameManager GetInstance() { return Instance; }
}