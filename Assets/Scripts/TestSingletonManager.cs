using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TestSingletonManager : MonoBehaviour
{
    public static TestSingletonManager Instance = null;
    
    public static Action OnWoke;
    public static void RaiseOnWoke()
    {
        if (OnWoke != null) OnWoke();
    }

//    public static Action UnitDied;
    public delegate void UnitDied(GameboardCharacterController deadUnit);
    public static event UnitDied UnitDeadAction;
    public static void RaiseUnitDied(GameboardCharacterController deadUnit)
    {
        if (UnitDeadAction != null) UnitDeadAction(deadUnit);
    }

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

    private List<GameboardUnitData> unitsDataOnBoardForInit;

    public List<GameboardCharacterController> unitsOnBoard;

    [SerializeField] private GameObject testPrefabToSpawn;

    [SerializeField] private GameObject GameboardPrefab;

    [SerializeField] private GameObject enivronmentToSpawn;

    public PhotonView playerPV;

    public PhotonView masterClientPV;

    private bool _spawnStuff = false;
    
    public bool spawnStuff
    {
        get { return _spawnStuff; }
        set
        {
            _spawnStuff = value;
        }
    }

    [SerializeField] private GameObject cameraAnchorPlayer2;
    [SerializeField] private GameObject mainCameraGO;
    
    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient && mainCameraGO != null && cameraAnchorPlayer2 != null)
        {
            mainCameraGO.transform.position = cameraAnchorPlayer2.transform.position;
            mainCameraGO.transform.rotation = cameraAnchorPlayer2.transform.rotation;
        }
        Instance = this;
        if(enivronmentToSpawn != null)
        {
            Instantiate(enivronmentToSpawn);
        }
    }

    public void KillUnit(GameboardCharacterController died)
    {
        unitsOnBoard.Remove(died);
        died.AnimatorSetBool("dieded", true);
        died.AnimatorSetBool("getHit", true);
        died.OnUnitDead();
//        Destroy(died.transform.parent.gameObject);
        RaiseUnitDied(died);
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

    public void SpawnStuff()
    {
        unitsDataOnBoardForInit = new List<GameboardUnitData>();
        unitsOnBoard = new List<GameboardCharacterController>();

        int iterator = 0;
        foreach (var singleUnit in TestManager.Instance.team1)
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
        foreach (var singleUnit in TestManager.Instance.team2)
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
            SpawnOnGameboard(unit, testPrefabToSpawn);
        }


        PlaceUnitsInPositions();
        foreach (var singleUnit in unitsOnBoard)
        {
            singleUnit.CreateHealthbar();
        }
        
        RaiseOnWoke();
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

    public void ResetAllRotations()
    {
        foreach (var singeUnit in unitsOnBoard)
        {
            singeUnit.SetDefaultRoation();
        } 
    }

    void SpawnOnGameboard(GameboardUnitData unitData, GameObject prefabToSpawn)
    {
        var spawnedGO = Instantiate(prefabToSpawn, masterClientPV.transform);
        var charComp = spawnedGO.transform.GetComponentInChildren<GameboardCharacterController>();
        if (charComp != null)
        {
            charComp.Setup(unitData);
            unitsOnBoard.Add(charComp);
        }
    }

    private void Update()
    {
        if (masterClientPV != null && _spawnStuff == true)
        {
            _spawnStuff = false;
            SpawnStuff();
        }
    }


    private void OnDestroy()
    {
        Instance = null;
    }
}
