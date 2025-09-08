using UnityEngine;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    public float moveSpeed = 3f;
    public int damage = 10;

    private GameManager gameManager;
    private Transform playerTarget;

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            FindClosestPlayer();
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (playerTarget == null)
        {
            FindClosestPlayer();
            return;
        }

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                playerTarget = player.transform;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            // Hacer daño al jugador
            SimplePlayerController player = other.GetComponent<SimplePlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }

    public void Die()
    {
        if (gameManager != null)
        {
            gameManager.EnemyDestroyed(gameObject);
        }

        if (IsServer)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Despawn();
            }
            Destroy(gameObject);
        }
    }
}