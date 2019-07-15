using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PhotonGBSetup : MonoBehaviour
{
    private void Start()
    {
        var PV = GetComponent<PhotonView>();
        if (PV.IsMine || PhotonNetwork.OfflineMode)
        {
            TestSingletonManager.Instance.playerPV = PV;
//            JoystickPlayer.variableJoystick = GetComponentInChildren<VariableJoystick>();
        }
        if (PhotonNetwork.IsMasterClient && (PV.IsMine || PhotonNetwork.OfflineMode))
        {
            PV.RPC("PVSpawn", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void PVSpawn()
    {
        TestSingletonManager.Instance.SpawnStuff();
    }
}
