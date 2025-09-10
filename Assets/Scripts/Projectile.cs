using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float speed = 10f;
    public int damage = 20;
    public GameObject owner;

    void Start()
    {
        if (IsServer)
        {
            Invoke("SimpleDespawn", 5);
        }
    }

    void Update()
    {
        if (IsServer)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    public void SimpleDespawn()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            SimpleDespawn();
            return;
        }

        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            SimpleDespawn();
        }

        SimplePlayerController player = collision.gameObject.GetComponent<SimplePlayerController>();
        if (player != null && player.gameObject != owner)
        {
            player.TakeDamage(damage);
            SimpleDespawn();
        }
    }
}