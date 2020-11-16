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
    private NetworkRoomPlayerLobby roomPlayerPrefab = null;
    
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

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
   public override void OnServerAddPlayer(NetworkConnection connection)
    {
        Debug.Log("Should add player");
    
        // if(SceneManager.GetActiveScene().name == menuScene)
        // {
            Debug.Log("Should spawn!");
            NetworkRoomPlayerLobby roomPlayerInstance = Instantiate(roomPlayerPrefab);

            NetworkServer.AddPlayerForConnection(connection, roomPlayerInstance.gameObject);
    //    }
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
