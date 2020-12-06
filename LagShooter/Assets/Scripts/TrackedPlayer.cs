using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TrackedPlayer : NetworkBehaviour
{

    public GameObject playerBody = null;
    public List<Vector3> positions = new List<Vector3>();


    public TrackedPlayer() {}

    public TrackedPlayer(GameObject player)
    {
        playerBody = player;
    }

    [Server]
    public void Update()
    {
        positions.Add(this.playerBody.transform.position);
       // Debug.Log("POSITION AT 0: " + positions[0]);
        //Debug.Log("POSITION AT 180: " + positions[179]);
       // Debug.Log("Adding position: " + playerBody.transform.position);
        if(positions.Count > 120)
        {
            positions.RemoveAt(0);
        }
       // Debug.Log(NetworkTransport.GetCurrentRtt);
        //Debug.Log("Positions count = " + positions.Count);
        //Debug.Log("NETWORK TIME! = " + NetworkTime.time);
        //Debug.Log("NETWORK RTT! = " + NetworkTime.rtt * 1000);
          //  Debug.Log("Positions = " + positions.Count);
    }
    
    // public void SetPlayerBody(GameObject player)
    // {
    //     playerBody = player;
    // }
}