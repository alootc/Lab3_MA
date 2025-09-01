using UnityEngine;
using Unity.Netcode;
public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab;
    public GameObject buffPrefab;

    public float currentBuffCount;
    public float BuffSpawnCount;

    private static GameManager Instance;

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

    void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            print("CurrentPlayer" + NetworkManager.Singleton);
            SpawnPlayerRpc(NetworkManager.Singleton.LocalClientId);
        }
        
    }


    [Rpc(SendTo.Server)]
    public void SpawnPlayerRpc(ulong id)
    {
        GameObject player = Instantiate(playerPrefab);
        //player.GetComponent<NetworkObject>().Spawn(true);
        //player..GetComponent<Simple>
        player.GetComponent<NetworkObject>().SpawnWithOwnership(id,true);
    }

    void Update()
    {

        if(IsServer && NetworkManager.Singleton != null)
        {
            currentBuffCount += Time.deltaTime;

            if (currentBuffCount > BuffSpawnCount)
            {
                Vector3 randomPos = new Vector3(Random.Range(-8, 8), 0.5f, Random.Range(-8, 8));
                GameObject buff = Instantiate(buffPrefab, randomPos, Quaternion.identity);
                buff.GetComponent<NetworkObject>().Spawn(true);
                currentBuffCount = 0;
            }
        }
       

    }

    public static GameManager instance_ => Instance;
}
