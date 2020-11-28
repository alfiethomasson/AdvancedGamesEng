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
    [SerializeField]
    private string menuScene = string.Empty;

    [SerializeField]
    private int minPlayers = 2;

    [SerializeField]
    private NetworkRoomPlayerLobby roomPlayerPrefab = null;
    
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public List<NetworkRoomPlayerLobby> RoomPlayers {get;} = new List<NetworkRoomPlayerLobby>();

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
        if(numPlayers < minPlayers) { return false; }

        foreach(var player in RoomPlayers)
        {
            if(!player.IsReady) {return false;}
        }

        return true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
