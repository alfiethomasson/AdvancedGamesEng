using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

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
    
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public List<NetworkRoomPlayerLobby> RoomPlayers {get;} = new List<NetworkRoomPlayerLobby>();
    public List<NetworkGamePlayerLobby> GamePlayers {get;} = new List<NetworkGamePlayerLobby>();

    public override void OnStartServer()
    {
        List<GameObject> spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
    }

    public override void OnStartClient()
    {
        Debug.Log("Trying to start client");
         var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
            Debug.Log(spawnablePrefabs.Count());
            foreach (var prefab in spawnablePrefabs)
            {
                ClientScene.RegisterPrefab(prefab);
            }
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }

    public override void OnClientConnect(NetworkConnection connection)
    {
        Debug.Log("Client Attempting to Connect");
        base.OnClientConnect(connection);

        OnClientConnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection connection)
    {
        if(numPlayers >= maxConnections)
        {
            connection.Disconnect();
            return;
        }
    }

    public override void OnServerDisconnect(NetworkConnection connection)
    {
        if(connection.identity != null)
        {
            var player = connection.identity.GetComponent<NetworkRoomPlayerLobby>();

            RoomPlayers.Remove(player);

            NotifyPlayer();
        }

        base.OnServerDisconnect(connection);
    }
   public override void OnServerAddPlayer(NetworkConnection connection)
    {
        Debug.Log("Should add player");
    
        // if(SceneManager.GetActiveScene().name == menuScene)
        // {
            Debug.Log("Should spawn!");

            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(connection, roomPlayerInstance.gameObject);
    //    }
    }

    public void NotifyPlayer()
    {
        foreach(var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyStart());
        }
    }

    private bool IsReadyStart()
    {
        if(RoomPlayers.Count < minPlayers) { Debug.Log("False bc not enough players"); 
        Debug.Log("num players = " + numPlayers); return false; }

        Debug.Log("RoomPlayers size = " + RoomPlayers.Count);

        foreach(var player in RoomPlayers)
        {
            if(!player.IsReady) {Debug.Log("False bc player not ready: " + player.DisplayName);
            return false;}
        }

        return true;
    }

    public void StartGame()
    {
       // Debug.Log("In start game");
        if(SceneManager.GetActiveScene().name == menuScene)
        {
          //  Debug.Log("Menu Scene correct");
            if(!IsReadyStart()) {return;}
            {
             //   Debug.Log("Ready to start!");
                ServerChangeScene("GameScene");
            }
        }
        else
        {
            Debug.Log("Scene manager name = " + SceneManager.GetActiveScene().name);
            Debug.Log("Menu scene name = " + menuScene);
        }
    }

    public override void ServerChangeScene(string newScene)
    {
        if(SceneManager.GetActiveScene().name == menuScene && newScene == "GameScene")
        {
           // CmdChangeScene(newScene);
            Debug.Log("Room players count = " + RoomPlayers.Count);
            for(int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                Debug.Log("I =" + i);
                var connection = RoomPlayers[i].connectionToClient;
                var gameplayerInstance = Instantiate(gamePlayerPrefab);
               // Debug.Log("RoomPlayers I = " + RoomPlayers[i].DisplayName);
                 gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.Destroy(connection.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(connection, gameplayerInstance.gameObject, true);
            }
            // Debug.Log("Room players count = " + RoomPlayers.Count);
            // for(int i = 0; i < RoomPlayers.Count; i++)
            // {
            //     Debug.Log("I =" + i);
            //     var connection = RoomPlayers[i].connectionToClient;
            //     var gameplayerInstance = Instantiate(gamePlayerPrefab);
            //    // Debug.Log("RoomPlayers I = " + RoomPlayers[i].DisplayName);
            //     //Debug.Log("Past the point!");
            //     gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

            //     //NetworkServer.Destroy(connection.identity.gameObject);

            //    // NetworkServer.ReplacePlayerForConnection(connection, gameplayerInstance.gameObject);
            // }
            base.ServerChangeScene(newScene);
        }
    }

    // [Command]
    // public void CmdChangeScene(string newScene)
    // {
    //    // Debug.Log("Room players count = " + RoomPlayers.Count);
    //         for(int i = RoomPlayers.Count - 1; i >= 0; i--)
    //         {
    //            // Debug.Log("I =" + i);
    //             var connection = RoomPlayers[i].connectionToClient;
    //             var gameplayerInstance = Instantiate(gamePlayerPrefab);
    //            // Debug.Log("RoomPlayers I = " + RoomPlayers[i].DisplayName);
    //             gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

    //             NetworkServer.Destroy(connection.identity.gameObject);

    //             NetworkServer.ReplacePlayerForConnection(connection, gameplayerInstance.gameObject);
    //         }
    // }
}
