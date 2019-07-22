using UnityEngine;
using Photon.Pun;

public class AimAssist : MonoBehaviour
{
    [SerializeField]
    public LineRenderer _lineRenderer;

    private bool lrEnabled = false;
    
    private void Awake()
    {
        VariableJoystick.OnJoystickEnable += Enable;
        VariableJoystick.OnJoystickRelease += Disable;
    }

    private void OnDestroy()
    {
        VariableJoystick.OnJoystickEnable -= Enable;
        VariableJoystick.OnJoystickRelease -= Disable;
    }

    public void Enable()
    {
        lrEnabled = true;        
//        _lineRenderer.enabled = (true);
    }


    private void FixedUpdate()
    {
//        return;
        _lineRenderer.enabled = (false);
        if (lrEnabled && !TurnManager.Instance.currentTurnExecuted && TurnManager.Instance.isMyTurn())
        {
            TurnManager.Instance.TurnOrder[0].AnimatorSetBool("pounceStart", false);
            if (JoystickPlayer.direction.magnitude < JoystickPlayer.RELEASE_THRESHOLD)
            {
                TurnManager.Instance.TurnOrder[0].SwitchArrow(false);
                return;
            }
            TurnManager.Instance.TurnOrder[0].SwitchArrow(true);
            TurnManager.Instance.TurnOrder[0].AnimatorSetBool("pounceStart", true);
            _lineRenderer.enabled = (true);
            _lineRenderer.positionCount = 2;
            Vector3 startPos = TurnManager.Instance.TurnOrder[0].transform.transform.position;
            startPos.y += 2.5f;
            
            _lineRenderer.SetPosition(0, new Vector3(startPos.x, 0, startPos.z));
            RaycastHit hitinfo;
            bool raycast;
            if(PhotonNetwork.IsMasterClient) {
                raycast = Physics.SphereCast(startPos, 5f, -JoystickPlayer.direction, out hitinfo);
            } else {
                raycast = Physics.SphereCast(startPos, 5f, JoystickPlayer.direction, out hitinfo);
            }
            if (raycast)
            {
                var point1 = hitinfo.point;
                point1.y = 0f;
                _lineRenderer.SetPosition(1, point1);
            }


            //Scale arrow
            float xyArrowScale = JoystickPlayer.direction.magnitude * 30f;
            TurnManager.Instance.TurnOrder[0].ArrowSpriteGO.transform.localScale = new Vector3(xyArrowScale,xyArrowScale * 1.4f,1);
            
            var quat = Quaternion.LookRotation(-JoystickPlayer.direction);
            
            //Rotate arrow
            var newRotVec = quat.eulerAngles;
            newRotVec.z = -newRotVec.y;
            newRotVec.y = 0;
            TurnManager.Instance.TurnOrder[0].ArrowSpriteGO.transform.localRotation = Quaternion.Euler(newRotVec);
            
            //Rotate visually
            newRotVec = quat.eulerAngles;
            if (TurnManager.Instance.thisPlayerId != TurnManager.Instance.TurnOrder[0].Data.teamId)
            {
                newRotVec.y += 180;
            }
            TurnManager.Instance.TurnOrder[0].VisualPrefabContainer.transform.localRotation = Quaternion.Euler(newRotVec);

        }
    }

    public void Disable()
    {
        lrEnabled = false;
//        _lineRenderer.enabled = (false);
    }
}
