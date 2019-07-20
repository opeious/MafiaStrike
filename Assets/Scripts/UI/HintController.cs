using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintController : MonoBehaviour
{
    bool seenHint = false;
    [SerializeField] Animator hintAnimator;

    [SerializeField] GameObject hintContainer;

    bool heldDown = false;

    private void Awake() {
        VariableJoystick.OnJoystickEnable += Disable;
        VariableJoystick.OnJoystickRelease += Enable;
    }

    private void OnDestroy() {
        VariableJoystick.OnJoystickEnable -= Disable;
        VariableJoystick.OnJoystickRelease += Enable;
    }

    void Enable() {
        heldDown = false;
    }

    void Disable() {
        heldDown = true;
    }
    
    private void FixedUpdate() {
        if(!seenHint) {
            if(TurnManager.Instance.currentTurnExecuted) {
                seenHint = true;
            }
        }

        if(!seenHint && !heldDown && TurnManager.Instance.isMyTurn()) {
            if(!hintAnimator.enabled) {
                hintAnimator.enabled = true;
                hintContainer.SetActive(true);
            }
        } else {
            if(hintAnimator.enabled) {
                hintAnimator.enabled = false;
                hintContainer.SetActive(false);
            }
        }
    }
}
