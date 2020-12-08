using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

//Spawn system to spawn players into the game 

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField]
    private GameObject player = null;

    private static List<Transform> spawnPoints = new List<Transform>();

    private int index = 0;

    public HitTracking hitTracker;

    //Add spawn point to l ist 
    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        //Sort list
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }

    //Remove spawn point from list 
    public static void RemoveSpawnPoint(Transform transform)
    {
        spawnPoints.Remove(transform);
    }

    //When this is created on server 
    public override void OnStartServer()
    {
        //Call function based on actions invoked on network manager
        LobbyManager.OnServerReadied += SpawnPlayer;
        //Get hit tracker from scene
        hitTracker = GameObject.Find("HitTracker").GetComponent<HitTracking>();
    }

    //When destroyed 
    [ServerCallback]
    private void OnDestroy()
    {
        //Decrement action 
         LobbyManager.OnServerReadied -= SpawnPlayer;
    }

    //Spawns player and replaces connection 
    [Server]
    public void SpawnPlayer(NetworkConnection connection)
    {
        //Get spawn point at index
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(index);

        //Check for missing spawn point
        if(spawnPoint == null)
        {
            Debug.Log("Missing spawn point at index " + index);
            return;
        }

        //Instantiate player at spawn point
        GameObject playerInstantiated = Instantiate(player, spawnPoints[index].position, spawnPoints[index].rotation);

        //Spawn player on clients
        NetworkServer.Spawn(playerInstantiated, connection);
        //Replace the player for the local client with this, gives it authority and local player
        NetworkServer.ReplacePlayerForConnection(connection, playerInstantiated, true);

        Debug.Log("Player spawned at " + spawnPoints[index].position);

        //Update previous spawn of player
        playerInstantiated.GetComponent<PlayerController>().UpdatePrevSpawn(index);
        //Add player to list of players to track 
        hitTracker.AddPlayerToList(playerInstantiated);

        //Increments index
        index += 1;
    }
}
