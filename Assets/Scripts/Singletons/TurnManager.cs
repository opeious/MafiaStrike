﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    
    [SerializeField] JoystickPlayer playerJoy;

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
        if(TurnOrder == null || TurnOrder.Count == 0) {
            return false;
        }
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
        ToggleMyTurnText();
        RaiseRefreshViews();
    }
    
    [SerializeField] Slider turnTimer;

    private void FixedUpdate() {
        _countdownTimer += Time.deltaTime;
        float normalizedTime = (TURN_TIME - _countdownTimer) / TURN_TIME;
        turnTimer.value = normalizedTime;
        if ( _countdownTimer > TURN_TIME)
        {
            if(GameSetupController.isGameSinglePlayer) {
                GameSetupController.PCInstance.TurnExpired(0f);
            } else if (PhotonNetwork.IsMasterClient && SpawningManager.Instance.serverPhotonView.IsMine)
            {
                SpawningManager.Instance.serverPhotonView.RPC("TurnExpired", RpcTarget.AllBuffered, 0f);   
            }
        }

        if(GameSetupController.isGameSinglePlayer && !doingBotTurn && !TurnManager.Instance.currentTurnExecuted && !TurnManager.Instance.isMyTurn()) {
            doingBotTurn = true;
            StartCoroutine(DoingBotTurn());
        }
    }

    IEnumerator DoingBotTurn() {
        TurnManager.Instance.currentTurnExecuted = true;
        yield return new WaitForSeconds(8f);
        foreach(var singleTurn in TurnManager.Instance.TurnOrder) {
            if(singleTurn.isPlayerMe()) {
                Vector3 direction = TurnManager.Instance.TurnOrder[0].transform.position - singleTurn.transform.position;
                GameSetupController.PCInstance.DoNetworkRelease(direction.normalized.x, direction.normalized.y, direction.normalized.z);
                break;
            }
        }
        doingBotTurn = false;
    }


    bool doingBotTurn;
    
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
        StartCoroutine(GoNextTurn(0f));
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

    [SerializeField] GameObject yourTextGO;

    public void ToggleMyTurnText() {
        if(isMyTurn()) {
            if(!yourTextGO.activeSelf) {
                yourTextGO.SetActive(true);
            }
        } else {
            if(yourTextGO.activeSelf) {
                yourTextGO.SetActive(false);
            }
        }
    }

    public void NextTurn()
    {
        var turn = TurnOrder[0];
        TurnOrder.Remove(turn);
        TurnOrder.Add(turn);
        currentTurn = TurnOrder[0];
        ToggleMyTurnText();

        RaiseRefreshViews();
        SpawningManager.Instance.ResetAllRotations();
    } 
}
