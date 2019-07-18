using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public List<CharacterDataScriptableObject> characterClasses;
    
    public List<CharacterClassTypes> team1Data;
    public List<CharacterClassTypes> team2Data;    


    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public GameObject GetVisualPrefabForClass(CharacterClassTypes type)
    {
        GameObject retVal = null;
        if (characterClasses != null && characterClasses.Count > 0)
        {
            for (int i = 0; i < characterClasses.Count; i++)
            {
                if (characterClasses[i].typeOfChar == type)
                {
                    retVal = characterClasses[i].visualPrefab;
                    break;
                }
            }
        }
        return retVal;
    }
    
}
