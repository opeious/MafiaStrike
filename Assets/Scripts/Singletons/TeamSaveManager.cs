using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TeamSaveManager : MonoBehaviour, ISaveData
{
    public CharacterClassTypes[] teamComp;

    private void Awake() {
        teamComp = new CharacterClassTypes[3];
        GameSaveManager.AddSavableManager(this);
    }

    private void Start() {
        GameSaveManager.Instance.SetSaveDirty();
    }

    public Dictionary<string, object> OnSaveData(Dictionary<string, object> saveDict) {
        Dictionary<string, object> test = new Dictionary<string, object>();
        int iterator = 1;
        foreach (var singleComp in teamComp) {
            test.Add(iterator++ + "", singleComp);
        }
        saveDict.Add("tcomp", test);
        return saveDict;
    }
    
    public void OnLoadData(Dictionary<string, object> saveDict) {
        Debug.Log("p");
        var test = saveDict["tcomp"] as Dictionary<string, object>;

        int iterator = 0;
        foreach(var kvp in test) {
            if(iterator < teamComp.Length) {
                teamComp[iterator++] = (CharacterClassTypes) Enum.Parse(typeof(CharacterClassTypes), kvp.Value + "");
            }
        }

        Debug.Log("p");
    }
}
