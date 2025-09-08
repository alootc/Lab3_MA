using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
   
    void Start()
    {
        if (IsServer)
        {
            Invoke("SimpleDespawn", 5);
        }
    }

    public void SimpleDespawn()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }

    private void OnCollisionEnter(Collision collision)
    {

    }
}
