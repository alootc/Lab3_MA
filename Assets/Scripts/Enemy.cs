using UnityEngine;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    public float speed = 3f;
    public float detectRange = 10f;
    private Transform playerTarget;
    private GameManager manager;

    public void SetGameManager(GameManager mgr)
    {
        manager = mgr;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) FindClosestPlayer();
    }

    void Update()
    {
        if (!IsServer) return;

        if (playerTarget == null)
        {
            FindClosestPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist > detectRange + 2f)
        {
            playerTarget = null;
            return;
        }

        Vector3 dir = (playerTarget.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = Mathf.Infinity;
        playerTarget = null;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                float d = Vector3.Distance(transform.position, players[i].transform.position);
                if (d < detectRange && d < minDist)
                {
                    minDist = d;
                    playerTarget = players[i].transform;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            if (manager != null) manager.EnemyDestroyed(gameObject);

            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null) netObj.Despawn();
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}