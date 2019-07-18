using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public const float TURN_TIME = 30f;
    
    public int thisPlayerId = 0;

    public static Action RefreshViews;
    public static void RaiseRefreshViews()
    {
        if (RefreshViews != null) RefreshViews();
    }
    
    public delegate void UnitDied(GameboardCharacterController deadUnit);
    public static event UnitDied UnitDeadAction;
    public static void RaiseUnitDied(GameboardCharacterController deadUnit)
    {
        if (UnitDeadAction != null) UnitDeadAction(deadUnit);
    }

    private void Awake()
    {
        Instance = this;
        thisPlayerId = PhotonNetwork.IsMasterClient ? 0 : 1;
        SpawningManager.OnUnitsSpawned += RefreshTurnOrder;
        UnitDeadAction += OnUnitDead;
    }
    
    public bool isMyTurn()
    {
        return TurnOrder[0].Data.teamId == thisPlayerId;
    }

    private void OnDestroy()
    {
        Instance = null;
        SpawningManager.OnUnitsSpawned -= RefreshTurnOrder;
        UnitDeadAction -= OnUnitDead;
    }

    private float _countdownTimer;
    public bool currentTurnExecuted;
    public List<GameboardCharacterController> TurnOrder;
    public GameboardCharacterController currentTurn;


    [SerializeField] private TextMeshProUGUI turnTimerText;

    private void OnUnitDead(GameboardCharacterController deadUnit)
    {
        TurnOrder.Remove(deadUnit);
        RaiseRefreshViews();

        bool meLose = true;
        bool meWin = true;
        foreach (var turn in TurnOrder)
        {
            if (turn.isPlayerMe())
            {
                meLose = false;
            }
            else
            {
                meWin = false;
            }
        }

        if (meLose)
        {
            StartCoroutine(LoseScreenAfterDelay());
        } else if (meWin)
        {
            StartCoroutine(WinScreenAfterDelay());
        }
    }

    IEnumerator WinScreenAfterDelay(float delay = 2f)
    {
        yield return new WaitForSeconds(delay);
        Debug.LogError("WinScreen!");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("WinScreen", LoadSceneMode.Single);     
    }
    
    IEnumerator LoseScreenAfterDelay(float delay = 2f)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LoseScreen", LoadSceneMode.Single);     
    }

    private void RefreshTurnOrder()
    {
        TurnOrder = new List<GameboardCharacterController>();
        IEnumerable<GameboardCharacterController> SortedBySpeed =
            SpawningManager.Instance.unitsOnBoard.OrderBy(gameboardCharacterController =>
                gameboardCharacterController.speed);
        SortedBySpeed = SortedBySpeed.Reverse();

        for (int i = 0; i < SpawningManager.Instance.unitsOnBoard.Count; i++)
        {
            TurnOrder.Add(SortedBySpeed.ElementAt(i));
        }

        currentTurn = TurnOrder[0];
        SetActiveOnly(currentTurn);
        RaiseRefreshViews();
    }
    
    private void Update()
    {
        _countdownTimer += Time.deltaTime;
        turnTimerText.text = (int)(TURN_TIME - _countdownTimer) + "";
        if ( _countdownTimer > TURN_TIME)
        {
            if (PhotonNetwork.IsMasterClient && SpawningManager.Instance.serverPhotonView.IsMine)
            {
                SpawningManager.Instance.serverPhotonView.RPC("TurnExpired", RpcTarget.AllBuffered, 0f);   
            }
        }
    }
    
    public void SetActiveOnly(GameboardCharacterController gcc)
    {
        DeactivateAllCircle();
        DeactivateAllVFX();
        ActivateCircleForGCC(gcc);
        ActivateVFXForGCC(gcc);
    }

    public void DeactivateAllCircle()
    {
        foreach (var singleTurn in TurnOrder)
        {
            singleTurn.DisableActiveCircle();
        }
    }
    
    public void DeactivateAllVFX()
    {
        foreach (var singleTurn in TurnOrder)
        {
            singleTurn.DisableActiveVFX();
        }
    }
    
    public void ActivateCircleForGCC(GameboardCharacterController gcc)
    {
        gcc.EnableActiveCircle();
    }
    
    public void ActivateVFXForGCC(GameboardCharacterController gcc)
    {
        gcc.EnableActiveVFX();
    }
    
    public void TurnExpired()
    {
        _countdownTimer = 0f;
        GoNextTurn(0f); 
    }
    
    public void KillUnit(GameboardCharacterController died)
    {
        SpawningManager.Instance.unitsOnBoard.Remove(died);
        died.AnimatorSetBool("dieded", true);
        died.AnimatorSetBool("getHit", true);
        died.OnUnitDead();
        RaiseUnitDied(died);
    }

    public IEnumerator GoNextTurn(float delay = 3f)
    {
        _countdownTimer = 0f;
        currentTurnExecuted = true;
        DeactivateAllCircle();
        yield return new WaitForSeconds(delay);
        DeactivateAllVFX();
        ActivateVFXForGCC(TurnOrder[1]);
        ActivateCircleForGCC(TurnOrder[1]);
        currentTurnExecuted = false;
        NextTurn();
        yield return null;
    }

    public void NextTurn()
    {
        var turn = TurnOrder[0];
        TurnOrder.Remove(turn);
        TurnOrder.Add(turn);
        currentTurn = TurnOrder[0];
        RaiseRefreshViews();
        SpawningManager.Instance.ResetAllRotations();
    } 
}
