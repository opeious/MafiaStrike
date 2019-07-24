using Photon.Pun;
using System.IO;
using UnityEngine;

public class GameSetupController : MonoBehaviour
{
    public static GameSetupController Instance;

    [SerializeField] private bool isSinglePlayer;

    public static InitPhotonCharacter PCInstance;

    // This script will be added to any multiplayer scene
    void Start()
    {
        Instance = this;
        CreatePlayer(); //Create a networked player object for each player that loads into the multiplayer scenes.
    }

    public static bool isGameSinglePlayer {
        get {
            if(Instance != null) {
                return Instance.isSinglePlayer;
            }
            return false;
        }
    }

    private void OnDestroy() {
        Instance = null;
    }

    private void CreatePlayer()
    {
        if(!isSinglePlayer) {
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PhotonPlayer"), Vector3.zero, Quaternion.identity);
        } else {
            Instantiate(Resources.Load(Path.Combine("PhotonPrefabs", "PhotonPlayer")), Vector3.zero, Quaternion.identity);
        }        
    }
}
