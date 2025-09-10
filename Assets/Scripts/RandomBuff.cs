using UnityEngine;
using Unity.Netcode;
public class RandomBuff : NetworkBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && IsServer)
        {
            //AddBuffToPlayerRpc(NetworkManager.Singleton.LocalClientId);
            ulong playerID = other.GetComponent<NetworkObject>().OwnerClientId;
            AddBuffToPlayerRpc(playerID);
        }
        
    }

    [Rpc(SendTo.Server)]
    private void AddBuffToPlayerRpc(ulong playerID)
    {
        print("Aplicar buff a : " + playerID);
        GetComponent<NetworkObject>().Despawn(true);
    }


    //public void AddHPToPlayer(ulong playerID, int amount)
    //{
    //    foreach (NetworkObject netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
    //    {
    //        print(netObj.OwnerClientId);
    //        if (netObj.OwnerClientId == playerID && netObj.TryGetComponent<SimplePlayerController>(out _))
    //        {
    //            netObj.GetComponent<SimplePlayerController>().Life.Value += amount;
    //        }


    //    }
    //}

}
