using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PhotonGBSetup : MonoBehaviour
{
    private PhotonView PV;
    
    private void Start()
    {
        PV = GetComponent<PhotonView>();
        
        if (PV.isMasterClient())
        {
            TestSingletonManager.Instance.masterClientPV = PV;
            SpawnerFunction();
        }
        if (PV.IsMine || PhotonNetwork.OfflineMode)
        {
            TestSingletonManager.Instance.playerPV = PV;
//            JoystickPlayer.variableJoystick = GetComponentInChildren<VariableJoystick>();
        }
    }

    void SpawnerFunction()
    {
        if (PhotonNetwork.IsMasterClient && (PV.IsMine || PhotonNetwork.OfflineMode))
        {
            PV.RPC("PVSpawn", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void PVSpawn()
    {
        TestSingletonManager.Instance.spawnStuff = true;
//        TestSingletonManager.Instance.SpawnStuff();
    }
}
