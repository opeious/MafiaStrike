using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class DelayStartWaitingRoomController : MonoBehaviourPunCallbacks
{
    /*This object must be attached to an object
    / in the waiting room scene of your project.*/

    // photon view for sending rpc that updates the timer
    private PhotonView myPhotonView;

    // scene navigation indexes
    [SerializeField]
    private int multiplayerSceneIndex;
    [SerializeField]
    private int menuSceneIndex;
    // number of players in the room out of the total room size
    private int playerCount;
    private int roomSize;
    [SerializeField]
    private int minPlayersToStart;

    [SerializeField] private TextMeshProUGUI findingMatchText;
    [SerializeField] private GameObject findingMatchGameObject;
    [SerializeField] private TextMeshProUGUI foundMatchText;
    [SerializeField] private GameObject foundMatchGameObject;

    // bool values for if the timer can count down
    private bool readyToCountDown;
    private bool readyToStart;
    private bool startingGame;
    //countdown timer variables
    private float timerToStartGame;
    private float notFullRoomTimer;
    private float fullRoomTimer;
    //countdown timer reset variables
    [SerializeField]
    private float maxWaitTime;
    [SerializeField]
    private float maxFullRoomWaitTime;


    List<string> findingMatchTexts = new List<string>();

    private void Start()
    {
        //initialize variables
        myPhotonView = GetComponent<PhotonView>();
        fullRoomTimer = maxFullRoomWaitTime;
        notFullRoomTimer = maxWaitTime;
        timerToStartGame = maxWaitTime;

        PlayerCountUpdate();

        findingMatchTexts.Add("FINDING \nOPPONENT\n.");
        findingMatchTexts.Add("FINDING \nOPPONENT\n..");
        findingMatchTexts.Add("FINDING \nOPPONENT\n...");
    }

    float countdownTextChange = 0f;
    int countdownTextChangeIterator = 0;
    private void Update()
    {
        countdownTextChange += Time.deltaTime;
        if(countdownTextChange > 1.5f) {
            countdownTextChange = 0f;
            if(countdownTextChangeIterator >= findingMatchTexts.Count) {
                countdownTextChangeIterator = 0;
            }
            findingMatchText.text = findingMatchTexts[countdownTextChangeIterator];
            countdownTextChangeIterator++;
        }
        WaitingForMorePlayers();
    }
    void PlayerCountUpdate()
    {
        // updates player count when players join the room
        // displays player count
        // triggers countdown timer
        playerCount = PhotonNetwork.PlayerList.Length;
        roomSize = PhotonNetwork.CurrentRoom.MaxPlayers;
        //playerCountDisplay.text = playerCount + ":" + roomSize;

        if (playerCount == roomSize)
        {
            readyToStart = true;
        }
        else if (playerCount >= minPlayersToStart)
        {
            readyToCountDown = true;
        }
        else
        {
            readyToCountDown = false;
            readyToStart = false;
        } 
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //called whenever a new player joins the room
        PlayerCountUpdate();
        //send master clients countdown timer to all other players in order to sync time.
        if(PhotonNetwork.IsMasterClient)
            myPhotonView.RPC("RPC_SyncTimer", RpcTarget.Others, timerToStartGame);
    }

    [PunRPC]
    private void RPC_SyncTimer(float timeIn)
    {
        //RPC for syncing the countdown timer to those that join after it has started the countdown
        timerToStartGame = timeIn;
        notFullRoomTimer = timeIn;
        if (timeIn < fullRoomTimer)
        {
            fullRoomTimer = timeIn;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //called whenever a player leaves the room
        PlayerCountUpdate();
    }


    void WaitingForMorePlayers()
    {
        //If there is only one player in the room the timer will stop and reset
        if (playerCount <= 1)
        {
            ResetTimer();
        }
        // when there is enough players in the room the start timer will begin counting down
        if (readyToStart)
        {
            fullRoomTimer -= Time.deltaTime;
            timerToStartGame = fullRoomTimer;
        }
        else if (readyToCountDown)
        {
            notFullRoomTimer -= Time.deltaTime;
            timerToStartGame = notFullRoomTimer;
        }
        // format and display countdown timer
        string tempTimer = string.Format("{0:00}", timerToStartGame);

        if(playerCount > 1) {
            if(timerToStartGame > 3f) {
                findingMatchText.gameObject.SetActive(false);
                foundMatchText.gameObject.SetActive(true);
            } else {
                findingMatchGameObject.SetActive(false);
                foundMatchGameObject.SetActive(true);
            }
        }
        //timerToStartDisplay.text = tempTimer;
        // if the countdown timer reaches 0 the game will then start
        if (timerToStartGame <= 0f)
        {
            if (startingGame)
                return;
            StartGame();
        }
    }

    void ResetTimer()
    {
        //resets the count down timer
        timerToStartGame = maxWaitTime;
        notFullRoomTimer = maxWaitTime;
        fullRoomTimer = maxFullRoomWaitTime;
    }

    void StartGame()
    {
        //Multiplayer scene is loaded to start the game
        startingGame = true;
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("MultiplayerGame");
    }

    public void DelayCancel()
    {
        //public function paired to cancel button in waiting room scene
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }
}
