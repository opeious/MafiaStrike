using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawningManager : MonoBehaviour
{
    public static Action OnUnitsSpawned;
    public static void RaiseOnUnitsSpawned()
    {
        if (OnUnitsSpawned != null) OnUnitsSpawned();
    }
    public static SpawningManager Instance = null;


    public PhotonView serverPhotonView;

    public PhotonView myPhotonView;

    public bool serverPhotonViewSet = false;

    public bool spawnedBefore = false;
    
    private List<GameboardUnitData> unitsDataOnBoardForInit;
    public List<GameboardCharacterController> unitsOnBoard;

    #region SerializedObjects
    [SerializeField]
    private Transform TopWall;
    [SerializeField]
    private Transform BottomWall;
    [SerializeField]
    private Transform LeftWall;
    [SerializeField]
    private Transform RightWall;
    [SerializeField]
    public RectTransform CanvasTransform;
    public GameObject prefabToBeSpawned;
    public GameObject prefabSpinOnLoad;
    #endregion

    
    private void Awake()
    {
        Instance = this;

        if (prefabSpinOnLoad != null && !PhotonNetwork.IsMasterClient)
        {
            prefabSpinOnLoad.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void FixedUpdate()
    {
        if (serverPhotonViewSet && serverPhotonView != null)
        {
            serverPhotonViewSet = false;
            SpawnUnits();
        }
    }
    
    public void SpawnUnits()
    {
        unitsDataOnBoardForInit = new List<GameboardUnitData>();
        unitsOnBoard = new List<GameboardCharacterController>();

        int iterator = 0;
        foreach (var singleUnit in DataManager.Instance.team1Data)
        {
            foreach (var singleCharClass in DataManager.Instance.characterClasses)
            {
                if (singleCharClass.typeOfChar == singleUnit)
                {
                    var singleUnitData = MakeDataFromOSbject(singleCharClass);
                    singleUnitData.teamId = 0;
                    singleUnitData.debugId = iterator++;
                    unitsDataOnBoardForInit.Add(singleUnitData);
                    break;
                }
            }
        }
        foreach (var singleUnit in DataManager.Instance.team2Data)
        {
            foreach (var singleCharClass in DataManager.Instance.characterClasses)
            {
                if (singleCharClass.typeOfChar == singleUnit)
                {
                    var singleUnitData = MakeDataFromOSbject(singleCharClass);
                    singleUnitData.teamId = 1;
                    singleUnitData.debugId = iterator++;
                    unitsDataOnBoardForInit.Add(singleUnitData);
                    break;
                }
            }
        }

        foreach (var unit in unitsDataOnBoardForInit)
        {
            SpawnOnGameboard(unit, prefabToBeSpawned);
        }


        PlaceUnitsInPositions();
        foreach (var singleUnit in unitsOnBoard)
        {
            singleUnit.CreateHealthbar();
        }
        
        RaiseOnUnitsSpawned();
    }
    
    public void ResetAllRotations()
    {
        foreach (var singeUnit in unitsOnBoard)
        {
            singeUnit.SetDefaultRoation();
        } 
    }
    
    void PlaceUnitsInPositions()
    {
        List<GameboardCharacterController> team1 = new List<GameboardCharacterController>();
        List<GameboardCharacterController> team2 = new List<GameboardCharacterController>();
        foreach (var singleUnit in unitsOnBoard)
        {
            if (singleUnit.Data.teamId == 0)
            {
                team1.Add(singleUnit);
            }
            else
            {
                team2.Add(singleUnit);
            }
        }

        float zPercentAwayFromWall = 0.1f;
        
        for (int i = 0; i < team1.Count; i++)
        {
            float zPos = BottomWall.position.z + (TopWall.position.z - BottomWall.position.z) * zPercentAwayFromWall;
            float xPos = LeftWall.position.x +
                         (RightWall.position.x - LeftWall.position.x) * (1f / (team1.Count + 1)) * (i + 1);
            team1[i].parent.transform.position = new Vector3(xPos, 0, zPos);
        }        
        for (int i = 0; i < team2.Count; i++)
        {
            float zPos = BottomWall.position.z +
                         (TopWall.position.z - BottomWall.position.z) * (1 - zPercentAwayFromWall);
            float xPos = LeftWall.position.x +
                         (RightWall.position.x - LeftWall.position.x) * (1f / (team1.Count + 1)) * (i + 1);
            team2[i].parent.transform.position = new Vector3(xPos, 0, zPos);
        }
        ResetAllRotations();
    }
    
    void SpawnOnGameboard(GameboardUnitData unitData, GameObject prefabToSpawn)
    {
        var spawnedGO = Instantiate(prefabToSpawn, serverPhotonView.transform);
        var charComp = spawnedGO.transform.GetComponentInChildren<GameboardCharacterController>();
        if (charComp != null)
        {
            charComp.Setup(unitData);
            unitsOnBoard.Add(charComp);
        }
        var charCompPV = spawnedGO.transform.GetComponentInChildren<PhotonView>();
        if (!spawnedBefore)
        {
            serverPhotonView.ObservedComponents = new List<Component>();
            spawnedBefore = true;
        }

        foreach (var singleObservedComponent in charCompPV.ObservedComponents)
        {
            serverPhotonView.ObservedComponents.Add(singleObservedComponent);
        }
    }

    private GameboardUnitData MakeDataFromOSbject(CharacterDataScriptableObject cdso)
    {
        var retVal = new GameboardUnitData();
        retVal.damage = cdso.damage;     
        retVal.maxHealth = cdso.maxHealth;
        retVal.UnitVelocity = cdso.gameboardVelocity;
        retVal.UnitSpeed = cdso.startingSpeed;
        retVal.charType = cdso.typeOfChar;
        retVal.icon = cdso.icon;
        return retVal;
    }
}
