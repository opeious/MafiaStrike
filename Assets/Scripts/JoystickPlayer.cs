using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoystickPlayer : MonoBehaviour
{
    public VariableJoystick variableJoystick;

    public static Vector3 direction;
    
    public const float RELEASE_THRESHOLD = 0.25f;

    public const float CANCEL_ALPHA_THRESHOLD = 0.30f;

    [SerializeField] private GameObject JoystickBackground;
    [SerializeField] private GameObject JoystickHandle;

    private Image JoystickBackgroundImage, JoystickHandleImage;

    private void Start()
    {
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

    public void DoOnRelease()
    {
        if (!TurnManager.Instance.isMyTurn())
        {
            return;
        }
        if (direction.magnitude > RELEASE_THRESHOLD && !TurnManager.Instance.currentTurnExecuted)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                direction = direction * -1;
            }
            if(GameSetupController.isGameSinglePlayer) {
                GameSetupController.PCInstance.DoNetworkRelease(direction.x, direction.y, direction.z);
            } else {
                SpawningManager.Instance.myPhotonView.RPC("DoNetworkRelease", RpcTarget.AllBuffered, direction.x, direction.y, direction.z);   
            }
        }
    }
}