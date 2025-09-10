using UnityEngine;
using Unity.Netcode;

public class Enemy : NetworkBehaviour
{
    public float moveSpeed = 3f;
    public int damage = 10;
    public int maxHealth = 100;

    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
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
            currentHealth.Value = maxHealth;
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

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
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
            SimplePlayerController player = other.GetComponent<SimplePlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsServer) return;

        currentHealth.Value -= damageAmount;

        if (currentHealth.Value <= 0)
        {
            Die();
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