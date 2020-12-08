using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//Class for each player in game that tracks past positions
//Used for latency rewind time

public class TrackedPlayer : NetworkBehaviour
{

    public GameObject playerBody = null;
    public List<Vector3> positions = new List<Vector3>();


    public TrackedPlayer() {}

    public TrackedPlayer(GameObject player)
    {
        playerBody = player;
    }

    //Only runs on server to track player position over 2 seconds
    [Server]
    public void Update()
    {
        positions.Add(this.playerBody.transform.position);

        if(positions.Count > 120)
        {
            positions.RemoveAt(0);
        }
    }
}