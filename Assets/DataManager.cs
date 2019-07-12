using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public List<CharacterDataScriptableObject> characterClasses;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public GameObject GetVisualPrefabForClass(characterClassTypes type)
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
