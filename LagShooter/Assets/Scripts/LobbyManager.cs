using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

//Custom Network Manager class

public class LobbyManager : NetworkManager
{
    [Scene]
    private string menuScene = "UIScene";

    [SerializeField]
    private int minPlayers = 2;

    [SerializeField]
    private NetworkRoomPlayerLobby roomPlayerPrefab = null;

    [SerializeField]
    private NetworkGamePlayerLobby gamePlayerPrefab = null;
    
    [SerializeField]
    private GameObject spawnSystem = null;
    
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;


    public List<NetworkRoomPlayerLobby> RoomPlayers {get;} = new List<NetworkRoomPlayerLobby>();
    public List<NetworkGamePlayerLobby> GamePlayers {get;} = new List<NetworkGamePlayerLobby>();

    public bool countdownActive = false;
    public float defaultCountDown = 5.0f;
    public float countdownTime = 5.0f;

    //Runs on the server when it starts
    public override void OnStartServer()
    {
        //Registers prefabs in SpawnablePrefabs folder, needed to spawn objects on the server
        List<GameObject> spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
    }

    //Runs on client when it starts
    public override void OnStartClient()
    {
        Debug.Log("Trying to start client");
        //Gets all spawnable prefabs
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
        foreach (var prefab in spawnablePrefabs)
        {
            //For each, register to the network manager
            //This is needed to spawn objects on the server and have them synced to the client
            ClientScene.RegisterPrefab(prefab);
        }
    }

    //When Server stops
    public override void OnStopServer()
    {
        RoomPlayers.Clear(); // Clear player list
    }

    //When client trys to conncet
    public override void OnClientConnect(NetworkConnection connection)
    {
        Debug.Log("Client Attempting to Connect");
        //Call base function
        base.OnClientConnect(connection);

        OnClientConnected?.Invoke();
    }

    //Called when something connects to server
    public override void OnServerConnect(NetworkConnection connection)
    {
        //If server full
        if(numPlayers >= maxConnections)
        {
            //Disconnect attempted connector
            connection.Disconnect();
            return;
        }
    }

    //When player disconnects 
    public override void OnServerDisconnect(NetworkConnection connection)
    {
        //If player exists
        if(connection.identity != null)
        {
            //In lobby, get player object
            var player = connection.identity.GetComponent<NetworkRoomPlayerLobby>();

            //Remove from lobby list
            RoomPlayers.Remove(player);

            //Update players
            NotifyPlayer();
        }

        //Call base function
        base.OnServerDisconnect(connection);
    }

    //When server succesfully adds a player
   public override void OnServerAddPlayer(NetworkConnection connection)
    {
        Debug.Log("Should spawn!");

        //Assign first joined player as lobby leader
        bool isLeader = RoomPlayers.Count == 0;

        //Instantiate room player object
        NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

        //Sets this player as leader or not leader
        roomPlayerInstance.IsLeader = isLeader;

        //Adds instantiated object to the connection, it is now the player for that connection
        NetworkServer.AddPlayerForConnection(connection, roomPlayerInstance.gameObject);
    }

    //Only needed if using leader starts game
    //This build has server starting automatically if players are ready
    public void NotifyPlayer()
    {
        //Loops through all players
        foreach(var player in RoomPlayers)
        {
            //Checks if ready to start and if so, allows leader to start game
            player.HandleReadyToStart(IsReadyStart());
        }
    }

    //Checks if all players are ready to start
    private bool IsReadyStart()
    {
        //Checks if there are enough players to start 
        if(RoomPlayers.Count < minPlayers) { Debug.Log("False, not enough players"); return false; }

        //Loops through all palyers
        foreach(var player in RoomPlayers)
        {
            //Checks if player is ready
            if(!player.IsReady) {Debug.Log("False, player not ready: " + player.DisplayName); return false;} // If not, return false
        }

        //If all players are ready, return true
        return true;
    }

    //Start game function to begin game starting
    public void StartGame()
    {
        if(SceneManager.GetActiveScene().name == menuScene) // If scene is the menuscene
        {
            if(!IsReadyStart()) //Check if players are ready to start
            {
                //If not ready, countdown is not active 
                countdownActive = false;
                countdownTime = defaultCountDown;
                for(int i = 0; i < RoomPlayers.Count; i++)
                {
                    RoomPlayers[i].countdownActive = false;
                }
                return;
            }
            else
            {
                Debug.Log("Ready to start!");
                //Set countdown to active!
                countdownActive = true;
                for(int i = 0; i < RoomPlayers.Count; i++)
                {
                    RoomPlayers[i].countdownActive = true;
                }
            }
        }
    }

    //Begin Scene change!
    public override void ServerChangeScene(string newScene)
    {
        //If going from menu to game
        if(SceneManager.GetActiveScene().name == menuScene && newScene == "GameScene")
        {
            //Stop Countdown
            countdownActive = false;
            //Loop through all players backwards to stop errors occuring
            for(int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                //Get players connection
                var connection = RoomPlayers[i].connectionToClient;

                //Spawn intermediatery game prefab!
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
                //Set display name to player name
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                //Destroy current game object attached to player
                NetworkServer.Destroy(connection.identity.gameObject);

                //And replace the player object with the new one!
                NetworkServer.ReplacePlayerForConnection(connection, gameplayerInstance.gameObject, true);
            }

            //Call base change scene
            base.ServerChangeScene(newScene);
        }
    }

    //Called on server when client is ready 
    public override void OnServerReady(NetworkConnection connection)
    {
        //Base method 
        base.OnServerReady(connection);

        OnServerReadied?.Invoke(connection);
    }

    //When scene is changed, this is called ons erver
    public override void OnServerSceneChanged(string sceneName)
    {
        if(sceneName == menuScene) // check if current scene equals the menu
        {
            //Spawn the spawn system used for creating the proper player prefabs
            GameObject playerSpawnSystem = Instantiate(spawnSystem);
            //Spawns on the network server which spawns on clients as well
            NetworkServer.Spawn(playerSpawnSystem);
        }
    }

    //Only runs update on the server 
    [Server]
    void Update()
    {
        if(countdownActive && SceneManager.GetActiveScene().name == menuScene) // Checks if countdown is active and scene is menu
        {
            //Decrements countdown time
            countdownTime -= Time.deltaTime;

            //Updates all clients with countdown time
            for(int i = 0; i < RoomPlayers.Count; i++)
            {
                RoomPlayers[i].countdownTime = (int)countdownTime;
            }

            //If countdown has ended
            if(countdownTime < 0)
            {
                //Change the scene
                ServerChangeScene("GameScene");
                countdownActive = false;
            }
        }
    }
}
