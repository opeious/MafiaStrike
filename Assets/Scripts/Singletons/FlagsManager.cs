using System.Collections.Generic;
using UnityEngine;
using System;

public class FlagsManager : MonoBehaviour, ISaveData {

    public static FlagsManager Instance = null;

    public static Dictionary<string,int> gameFlags = new Dictionary<string, int>();

	void Awake () {
        Instance = this;
        GameSaveManager.AddSavableManager(this);
	}

    void OnDestroy() {
        Instance = null;
    }

    public static void SetFlag(string flagId, int val) {
        if(gameFlags.ContainsKey(flagId)) {
            gameFlags[flagId] = val;
        } else {
            gameFlags.Add(flagId, val);
        }
    }

    public static int GetFlag(string flagId) {
        int retVal = 0;
        if(gameFlags.ContainsKey(flagId)) {
            retVal = gameFlags[flagId];
        }
        return retVal;
    }

    public static bool IsFlagSet(string flagId) {
        bool retVal = false;
        if(gameFlags.ContainsKey(flagId)) {
            retVal = !(gameFlags[flagId] == 0);
        }
        return retVal;
    }

    public Dictionary<string, object> OnSaveData(Dictionary<string, object> saveDict) {
        foreach(var kvp in gameFlags) {
            saveDict.Add(kvp.Key, (object) kvp.Value);
        }
        return saveDict;
    }

    public void OnLoadData(Dictionary<string, object> saveDict) {
        gameFlags = new Dictionary<string, int>();
        foreach(var kvp in saveDict) {
            gameFlags.Add(kvp.Key, Convert.ToInt32(kvp.Value));
        }
    }
}
