using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public interface ISaveData
{
    Dictionary<string, object> OnSaveData(Dictionary<string, object> saveDict);

    void OnLoadData(Dictionary<string, object> saveDict);
}

public class GameSaveManager : MonoBehaviour
{
    public static bool loadedOnce = false;

    static List<ISaveData> allSaveManagers = new List<ISaveData>();

    public static GameSaveManager Instance = null;

    bool saveStateDirty = false;

    void Awake()
    {
        Instance = this;
        GameSaveManager.saveFilePath = Application.persistentDataPath +  "/AutoSave.sav";
    }

    void OnDestroy()
    {
        Instance = null;
    }

    void Start()
    {
        //GameEvents.PeriodicUpdate5s += SetSaveDirty;
    }

    public void SetSaveDirty()
    {
        saveStateDirty = true;
    }

    static string saveFilePath;

    void Update()
    {
        if (saveStateDirty)
        {
            if(!loadedOnce) {
                LoadDataToManagers();
            }
            saveStateDirty = false;

            Dictionary<string, object> toWriteToFile = new Dictionary<string, object>();
            foreach (var singleSaveManager in allSaveManagers)
            {
                Dictionary<string, object> dictToSave = new Dictionary<string, object>();
                dictToSave = singleSaveManager.OnSaveData(dictToSave);
                toWriteToFile.Add(singleSaveManager.GetType() + "", dictToSave);
            }
            string dataToFile = MiniJSON.Json.Serialize(toWriteToFile);

            FileInfo file = new FileInfo(saveFilePath);
            file.Directory.Create();
            File.WriteAllText(file.FullName, dataToFile);
        }
    }

    public static void LoadDataToManagers()
    {
        loadedOnce = true;
        if(File.Exists(saveFilePath)) {
            string dataAsJson = File.ReadAllText(saveFilePath); 
            // var parsedJSONData = SimpleJSON.JSON.Parse(dataAsJson);
            var parsedJSONData = MiniJSON.Json.Deserialize(dataAsJson);
            Dictionary<string, object> dictToLoad = (Dictionary<string, object>) (parsedJSONData);
            foreach (var singleSaveManager in allSaveManagers) {
                if(dictToLoad.ContainsKey(singleSaveManager.GetType() + "")) {
                    singleSaveManager.OnLoadData((Dictionary<string, object>)dictToLoad[singleSaveManager.GetType() + ""]);
                }
            }
        }
    }

    public static void AddSavableManager(ISaveData savableManager)
    {
        if(!allSaveManagers.Contains(savableManager)) {
            allSaveManagers.Add(savableManager);
        }
    }
}
