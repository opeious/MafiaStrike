using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoystickPlayer : MonoBehaviour
{
    public static VariableJoystick variableJoystick;

    public static Vector3 direction;
    
    public const float RELEASE_THRESHOLD = 0.45f;

    public const float CANCEL_ALPHA_THRESHOLD = 0.30f;

    [SerializeField] private GameObject JoystickBackground;
    [SerializeField] private GameObject JoystickHandle;

    private Image JoystickBackgroundImage, JoystickHandleImage;

    private void Start()
    {
        variableJoystick = GetComponent<VariableJoystick>();
        VariableJoystick.OnJoystickRelease += DoOnRelease;
        if (JoystickBackground != null)
        {
            JoystickBackgroundImage = JoystickBackground.GetComponent<Image>();
        }
        if (JoystickHandle != null)
        {
            JoystickHandleImage = JoystickHandle.GetComponent<Image>();
        }
    }

    private void OnDestroy()
    {
        VariableJoystick.OnJoystickRelease -= DoOnRelease;
    }

    private void FixedUpdate()
    {
        if (variableJoystick == null)
            return;
        
        if (variableJoystick.Direction.magnitude > 0)
        {
            direction = Vector3.forward * variableJoystick.Vertical + Vector3.right * variableJoystick.Horizontal;
            if (JoystickBackgroundImage != null && JoystickHandleImage != null)
            {
                Color newColor = JoystickBackgroundImage.color;
                if (direction.magnitude > RELEASE_THRESHOLD)
                {
                    newColor.a = 1f;
                }
                else
                {
                    newColor.a = CANCEL_ALPHA_THRESHOLD;
                }

                JoystickBackgroundImage.color = newColor;
                JoystickHandleImage.color = newColor;
            }
        }
    }

    [PunRPC]
    public void DoNetworkRelease(float x, float y, float z)
    {
        var currentTurn = TurnManager.Instance.TurnOrder[0];
        if (currentTurn != null)
        {
            currentTurn.OnTryMove(- ( new Vector3(x,y,z)));
//            if (PhotonNetwork.IsMasterClient)
//            {
//                currentTurn.OnTryMove( ( new Vector3(x,y,z)));   
//            }
//            else
//            {
//                currentTurn.OnTryMove(- ( new Vector3(x,y,z)));
//            }
        }

        StartCoroutine(TurnManager.Instance.GoNextTurn());
    }

    public void DoOnRelease()
    {
        if (!TurnManager.Instance.isMyTurn())
        {
//            return;
        }
        if (direction.magnitude > RELEASE_THRESHOLD && !TurnManager.Instance.currentTurnExecuted)
        {
            DoNetworkRelease(direction.x, direction.y, direction.z);
            if (PhotonNetwork.IsMasterClient)
            {
//                TestSingletonManager.Instance.playerPV.RPC("DoNetworkRelease", RpcTarget.AllBuffered, direction.x, direction.y, direction.z);   
            }
            else
            {
//                TestSingletonManager.Instance.playerPV.RPC("DoNetworkRelease", RpcTarget.AllBuffered, -direction.x, -direction.y, -direction.z);
            }
//            TestSingletonManager.Instance.playerPV.RPC("DoNetworkRelease", RpcTarget.AllBuffered, direction.x, direction.y, direction.z);   
//            Debug.LogError("Player" + TurnManager.Instance.thisPlayerId + " released");
        }
    }
}