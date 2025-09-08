using UnityEngine;
using Unity.Netcode;

public class Buff : NetworkBehaviour
{
    public float duration = 10f;
    public float speedMultiplier = 1.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
     
            SimplePlayerController player = other.GetComponent<SimplePlayerController>();
            if (player != null)
            {
                player.ApplySpeedBuff(speedMultiplier, duration);
                Debug.Log("Buff applied to player: " + other.name);

                DespawnBuff();
            }
        }
    }

    private void DespawnBuff()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Despawn();
        }
        Destroy(gameObject);
    }
}