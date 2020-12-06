using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HitTracking : NetworkBehaviour
{

    public List<GameObject> playerGameObjects = new List<GameObject>();

    public List<TrackedPlayer> trackedPlayers = new List<TrackedPlayer>();

    [SerializeField]
    private GameObject prevGameObject;

    public float serverTickRate;

    // Start is called before the first frame update
    void Start()
    {
        serverTickRate = 1.0f / 60.0f; 
        serverTickRate = serverTickRate * 1000.0f;   
        Debug.Log("Server Tick rate = " + serverTickRate);
    }

    [Server]
    void Update()
    {
        //Debug.Log("TrackedPlayers list count = " + trackedPlayers.Count);
        for(int i = 0; i < trackedPlayers.Count; i++)
        {
            trackedPlayers[i].Update();
        }
    }

    public void AddPlayerToList(GameObject newPlayer)
    {
        playerGameObjects.Add(newPlayer);
        TrackedPlayer tempPlayer = new TrackedPlayer(newPlayer.gameObject);
       // tempPlayer.SetPlayerBody(newPlayer);
        trackedPlayers.Add(tempPlayer);
    }

    [Server]
    public RaycastHit BeginComputeHit(double latency, Vector3 rayOrigin, Vector3 rayForward)
    {
        //Debug.Log("BEGIN COMPUTING HIT ITS AWESOME");
       // Debug.Log("Server Tick Rate * 1000 = " + (int)(serverTickRate));
       // Debug.Log("Latency Time * 1000 = " + (int)(latency * 1000));
        latency = latency * 1000;
        int calculatedPosition = (int)Mathf.Floor((float)latency / (float)serverTickRate);
        if(calculatedPosition > 119) {calculatedPosition = 119;}
        calculatedPosition = 119 - calculatedPosition;
         Debug.Log("Calculated Position = " + (int)calculatedPosition);
         calculatedPosition = 0;

        foreach(TrackedPlayer player in trackedPlayers)
        {
            //Debug.Log("Player currently at: " + player.playerBody.transform.position);
           // Debug.Log("Player moving to: " + player.positions[calculatedPosition].position);
            player.playerBody.transform.position = player.positions[calculatedPosition];
            //prevGameObject.transform.position = player.positions[calculatedPosition];
           // Debug.Log("Positions number 0: " + player.positions[0]);
           // Debug.Log("Positions number 119: " + player.positions[119]);
        }
        RaycastHit hit;
        Physics.Raycast(rayOrigin,rayForward, out hit, 500.0f);
        StartCoroutine(ParallelMoveObject(hit, calculatedPosition));
        return hit;
    }

    private IEnumerator ParallelMoveObject(RaycastHit hit, int calculatedPos)
    {
        if(hit.collider.tag == "PlayerBody")
        {
            foreach(TrackedPlayer p in trackedPlayers)
            {
                if(p.playerBody.GetComponentInChildren<CapsuleCollider>() == hit.collider)
                {
                    prevGameObject.transform.position = p.positions[calculatedPos];
                    RpcMovePrevGameObject(p.positions[calculatedPos]);
                    yield break;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcMovePrevGameObject(Vector3 toMove)
    {
        toMove.y = 1.0f;
        prevGameObject.transform.position = toMove;
    }

    // public struct PlayerPositions
    // {
    //     public GameObject playerBody;
    //     public List<Transform> positions;

    //     [Server]
    //     void Update()
    //     {
    //         positions.Add(playerBody.transform);
    //         if(positions.Count > 60)
    //         {
    //             positions.RemoveAt(0);
    //         }
    //         Debug.Log("Positions count = " + positions.Count);
    //     }
    // }
}
