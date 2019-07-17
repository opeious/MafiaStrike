using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public static Action RefreshViews;
    public static void RaiseRefreshViews()
    {
        if (RefreshViews != null) RefreshViews();
    }

    public List<GameboardCharacterController> TurnOrder;

    public GameboardCharacterController currentTurn = null;

    public bool currentTurnExecuted = false;

    //Make this different for both players
    public int thisPlayerId = 0;

    [SerializeField]
    private GameObject Enivronment;

    int currentTurnId = 0;

    public void ClientEnvSetup()
    {
        if (!PhotonNetwork.IsMasterClient && Enivronment != null)
        {
            Enivronment.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
    
    private void Awake()
    {
        
        thisPlayerId = PhotonNetwork.IsMasterClient ? 0 : 1;
        Instance = this;
        TestSingletonManager.OnWoke += OnAwakeSetup;
        TestSingletonManager.UnitDeadAction += OnUnitDead;
    }

    private void OnAwakeSetup()
    {
        RefreshTurnOrder();
        Debug.Log("WokeAF");
    }

    IEnumerator WinScreenAfter2()
    {
        yield return new WaitForSeconds(2f);
        Debug.LogError("WinScreen!");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("WinScreen", LoadSceneMode.Single);   
    }
    
    IEnumerator LoseScreenAfter2()
    {
        yield return new WaitForSeconds(2f);
        Debug.LogError("LoseScreen!");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("LoseScreen", LoadSceneMode.Single);   
    }

    private void OnUnitDead(GameboardCharacterController deadUnit)
    {
        TurnOrder.Remove(deadUnit);
        bool player1Win = true;
        bool player2Win = true;
        foreach (var turn in TurnOrder)
        {
            if (turn.Data.teamId == 0)
            {
                player2Win = false;
            }
            else
            {
                player1Win = false;
            }
        }

        if (player1Win)
        {
            if (TurnManager.Instance.thisPlayerId == 0)
            {
                Debug.Log("Starting player 1 win");
                StartCoroutine(WinScreenAfter2());
            }
            else
            {
                Debug.Log("Starting player 2 loss");
                StartCoroutine(LoseScreenAfter2());
            }
        }

        if (player2Win)
        {
            if (TurnManager.Instance.thisPlayerId != 0)
            {
                Debug.Log("Starting player 2 win");
                StartCoroutine(WinScreenAfter2());
            }
            else
            {
                Debug.Log("Starting player 1 loss");
                StartCoroutine(LoseScreenAfter2());
            }
        }
        RaiseRefreshViews();
    }

    private void RefreshTurnOrder()
    {
        TurnOrder = new List<GameboardCharacterController>();
        IEnumerable<GameboardCharacterController> SortedBySpeed =
            TestSingletonManager.Instance.unitsOnBoard.OrderBy(gameboardCharacterController =>
                gameboardCharacterController.speed);
        SortedBySpeed = SortedBySpeed.Reverse();

        for (int i = 0; i < TestSingletonManager.Instance.unitsOnBoard.Count; i++)
        {
            TurnOrder.Add(SortedBySpeed.ElementAt(i));
        }

        currentTurn = TurnOrder[0];
        SetActiveOnly(currentTurn);
        RaiseRefreshViews();
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

    public void SetActiveOnly(GameboardCharacterController gcc)
    {
        DeactivateAllCircle();
        DeactivateAllVFX();
        ActivateCircleForGCC(gcc);
        ActivateVFXForGCC(gcc);
    }

    private void OnDestroy()
    {
        Instance = null;
        TestSingletonManager.OnWoke -= OnAwakeSetup;
        TestSingletonManager.UnitDeadAction -= OnUnitDead;
    }

    private float _timer = 0f;

    [SerializeField] private TextMeshProUGUI timerText;
    
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > 30f)
        {
            if (_timer >= 15f)
            {
                StartCoroutine(GoNextTurn());
            }
            timerText.text = ((15 - (int)_timer)) + "";
        }
        else
        {
            timerText.text = "NEXT >";
        }
    }


    public IEnumerator GoNextTurn()
    {
        _timer = 0f;
        currentTurnExecuted = true;
        DeactivateAllCircle();
        yield return new WaitForSeconds(2.5f);
        DeactivateAllVFX();
        ActivateVFXForGCC(TurnOrder[1]);
        ActivateCircleForGCC(TurnOrder[1]);
        currentTurnExecuted = false;
        NextTurn();
        yield return null;
    }

    public bool isMyTurn()
    {
        return TurnOrder[0].Data.teamId == thisPlayerId;
    }
    
    public GameboardCharacterController NextTurn()
    {
        var turn = TurnOrder[0];
        TurnOrder.Remove(turn);
        TurnOrder.Add(turn);
        currentTurn = TurnOrder[0];
        RaiseRefreshViews();
        TestSingletonManager.Instance.ResetAllRotations();
        return turn;
    } 
}
