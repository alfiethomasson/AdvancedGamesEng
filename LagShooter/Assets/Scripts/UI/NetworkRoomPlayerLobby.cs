using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{

    [SerializeField]
    private GameObject lobbyUI = null;

    [SerializeField]
    private Text[] playerNames = new Text[4];
    [SerializeField]
    private Text[] playersReady = new Text[4];
    [SerializeField]
    private Button startButton = null;
    [SerializeField]
    private Text countdownText = null;
    [SerializeField]
    private Text welcomeText = null;

    private string countString = "Launching game in: ";

    [SyncVar (hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar (hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SyncVar (hook = nameof(HandleCountDownTimeChanged))]
    public int countdownTime = 300;
    [SyncVar (hook = nameof(HandleCountDownActive))]
    public bool countdownActive = false;

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            //Sets leader value 
            isLeader = value;
            //And enables/disables start game button (if used)
            startButton.gameObject.SetActive(value);
        }
    }

    private LobbyManager room;

    private LobbyManager Room
    {
        get
        {
            // If room has been assigned 
            if(room != null) { return room; } 
            return room = NetworkManager.singleton as LobbyManager; // Singleton to ensure consistency if room has not been assigned
        }
    }

    //If has authority when created
    public override void OnStartAuthority()
    {
        //Set player name on the server
        CmdSetDisplayName(PlayerNameInput.DispName);
        //Updates the welcome text to display player name
        welcomeText.text = "Welcome: " + PlayerNameInput.DispName;
        //Sets the lobbyUI to be active
        lobbyUI.SetActive(true);

    }

    //When this is started on client
    public override void OnStartClient()
    {
        //Adds this to network managers list of objects
        Room.RoomPlayers.Add(this);

        //Updates Display
        UpdateDisplay();
    }

    //When this is started on server
    public override void OnStartServer()
    {
        //Adds this to network managers list of objects
        Room.RoomPlayers.Add(this);
    }

    //When destroyed 
    public override void OnNetworkDestroy()
    {
        //Removet his from network manager list
        Room.RoomPlayers.Remove(this);
    
        //And update display
        UpdateDisplay();
    }

    //When ready status is changed this is called
    public void HandleReadyStatusChanged(bool oldval, bool newval)
    {
        //Update display
        UpdateDisplay();
        //Check if game can be started
        CmdStartGame();
    }

    //Called when name is changed
    public void HandleDisplayNameChanged(string oldval, string newval)
    {
        //Update display
        UpdateDisplay();
    }

    //Called when countdown time changed
    public void HandleCountDownTimeChanged(int oldVal, int newVal)
    {
        //Updates countdown time
        countdownText.text = countString + newVal;
    }

    //Called when countdown is activated/deacctivated
    public void HandleCountDownActive(bool oldVal, bool newVal)
    {
        //Enables/disables countdown time on ui 
        countdownText.gameObject.SetActive(newVal);
    }

    //Updates the display of tthe lobby 
    private void UpdateDisplay()
    {
        if(!hasAuthority) // If this does not have authority, recursively check for the one that does 
        {
            foreach(var player in Room.RoomPlayers)
            {
                if(player.hasAuthority)
                {
                    player.UpdateDisplay(); //When player with authority is found, call this again
                    break;
                }
            }
            return;
        }

        //Loop through all players
        for(int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            //Updates player names to equal their names
            playerNames[i].text = Room.RoomPlayers[i].DisplayName;
            //Updates ready status
            playersReady[i].text = Room.RoomPlayers[i].IsReady ? 
            "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    //Called when ready bool is changed
    //Only used if using the leader starts game 
    public void HandleReadyToStart(bool ready)
    {
        if(!isLeader) { return; }

        startButton.interactable = ready;
    }

    //Sets display name on server 
    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    //Sets ready status on server
    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        //Call function on network manager to check if game is ready to start
        Room.NotifyPlayer();
    }

    //Used if using the leader starts game method
    [Command]
    public void CmdStartGame()
    {
        //Start Game function called
        Room.StartGame();
    }
}
