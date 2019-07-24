using Photon.Pun;
using UnityEngine;

public class InitPhotonCharacter : MonoBehaviour
{
    private PhotonView thisPV;
    
    // Start is called before the first frame update
    private void Awake()
    {
        thisPV = GetComponent<PhotonView>();

        if (thisPV.IsMine)
        {
            SpawningManager.Instance.myPhotonView = thisPV;
        }
        
        if (thisPV.isMasterClient())
        {
            SpawningManager.Instance.serverPhotonView = thisPV;
            SpawnOnServer();
        }
        else
        {
            thisPV.Synchronization = ViewSynchronization.Off;
        }
    }

    void SpawnOnServer()
    {
        if (PhotonNetwork.IsMasterClient && (thisPV.IsMine || PhotonNetwork.OfflineMode))
        {
            thisPV.RPC("PVSpawn", RpcTarget.AllBuffered);
        }
    }
    
    [PunRPC]
    void TurnExpired(float nuller)
    {
        TurnManager.Instance.TurnExpired();
    }
    
    [PunRPC]
    void PVSpawn()
    {
        SpawningManager.Instance.serverPhotonViewSet = true;
    }
    
    [PunRPC]
    public void DoNetworkRelease(float x, float y, float z)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            thisPV.RPC("ServerNetworkRelease", RpcTarget.AllBuffered, x, y, z);
        }
    }

    [PunRPC]
    public void ServerNetworkRelease(float x, float y, float z)
    {
        var currentTurn = TurnManager.Instance.TurnOrder[0];
        if (currentTurn != null)
        {
            currentTurn.OnTryMove(- ( new Vector3(x,y,z)));
        }
        StartCoroutine(TurnManager.Instance.GoNextTurn());
    }

    [PunRPC]
    public void TakeDamage(int indexInTurnOrder, int damageTaken, int indexOf)
    {
        TurnManager.Instance.TurnOrder[indexInTurnOrder].TakeDamage(damageTaken, indexOf);
    }

    [PunRPC]
    public void SlomoMaybe(int indexFrom, float distance, int indexOf) {
        TurnManager.Instance.TurnOrder[indexOf].SlomoMaybe(indexFrom, distance);
    }
}
