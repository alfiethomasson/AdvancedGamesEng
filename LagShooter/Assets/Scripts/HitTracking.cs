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
    [SerializeField]
    private GameObject curGameObject;
    [SerializeField]
    private GameObject shotGameObject;

    [SyncVar]
    [SerializeField]
    private bool useFrameRewind = false;
    
    [SyncVar]
    [SerializeField]
    private bool useLatencyRewind = false;

    [SyncVar]
    [SerializeField]
    public bool useRewindHitDetection = false;

    public float serverTickRate;

    private LayerMask yellowMask;

    // Start is called before the first frame update
    void Start()
    {
        //Sets the variable of server tick rate
        serverTickRate = 1.0f / 60.0f; 
        //Multiplied by 1000 for ease of viewing and calculations
        serverTickRate = serverTickRate * 1000.0f;   
        Debug.Log("Server Tick rate = " + serverTickRate);
        //Layermask used for rewind hit detection (Currently not working)
        yellowMask = LayerMask.GetMask("Player");
    }

    //Update only called on server
    [Server]
    void Update()
    {
        if(useLatencyRewind) //If latency rewind is active
        {
            for(int i = 0; i < trackedPlayers.Count; i++)
            {
                trackedPlayers[i].Update(); // Call update on all players in list
            }
        }
    }

    //Add player to tracked players
    public void AddPlayerToList(GameObject newPlayer)
    {
        playerGameObjects.Add(newPlayer); //Add to list of game objects
        //Temporary class trackedplayer based on game object
        TrackedPlayer tempPlayer = new TrackedPlayer(newPlayer.gameObject); 
        //add player to list 
        trackedPlayers.Add(tempPlayer);
    }

    //Only ran on server, is called by Weapons to calculate whether or not they hit on the server
    [Server]
    public RaycastHit BeginComputeHit(double latency, Vector3 rayOrigin, Vector3 rayForward, float frameTime)
    {
       if(useLatencyRewind) //If using latency rewind
       {
        //Multiply latency by 1000 for ease of viewing and calculations
        latency = latency * 1000;
        //This gets the position in list of gameobjects that the player should have been viewing at the time they fired 
        //Currently does not work as intended
        int calculatedPosition = (int)Mathf.Floor((float)(latency * 4) / (float)serverTickRate);
        //If higher latency than tracked, reset to max.
        if(calculatedPosition > 119) {calculatedPosition = 119;}
        //Subtract calculated position from 119, this is because the oldest values in the list are at list[0]
        calculatedPosition = 119 - calculatedPosition;
        Debug.Log("Calculated Position = " + (int)calculatedPosition);

        //Loops through all tracked players
        foreach(TrackedPlayer player in trackedPlayers)
        {
            //And moves them to calculated position for shot detection 
            player.playerBody.transform.position = player.positions[calculatedPosition];
        }
        RaycastHit hit;
        //Shoots ray 
        Physics.Raycast(rayOrigin,rayForward, out hit, 500.0f);

        //Moves capsules to calculated positions 
        MoveObjectRtt(hit, calculatedPosition);

        return hit;

       }
       else if(useFrameRewind) //Using frame rewind 
       {

        //Ensures the value recieved is not equal to current frame count as this throws error
        if(frameTime == Time.frameCount)
        {
            frameTime--;
        }

        RaycastHit hit = new RaycastHit();

        //Loops through all players in tracked players
        foreach(TrackedPlayer player in trackedPlayers)
        {
            //If not using rewind hit detection (using capsules to check hits instead of actual players)
            if(!useRewindHitDetection)
            {
                //Calls function SetNewTransform for player, should move it back to where it was when player shot 
                player.playerBody.GetComponent<SyncPosition>().SetNewTransform((int)frameTime);
                //Shoots ray 
                Physics.Raycast(rayOrigin,rayForward, out hit, 500.0f);
                //If hit player, break out of loop 
                if(hit.collider.tag == "PlayerBody")
                {
                   break;
                }
            }
            else // If using rewind hit detection by moving capsules instead of players (WIP, not currently working as intended)
            {
                //Sets the capsule position to calculated frame position using frame time
                prevGameObject.transform.position = player.playerBody.GetComponent<SyncPosition>().GetTransformAtFrame((int)frameTime);
                //Shoots ray, ommiting players 
                Physics.Raycast(rayOrigin, rayForward, out hit, 500.0f, ~yellowMask);
                //If Ray hits capsule
                if(hit.collider.tag == "ExampleHit")
                {
                    Debug.Log("Succesfully hit yellow example!");
                    break;
                }
            }

        }
        //Move players back to where they were before 
        foreach(TrackedPlayer player in trackedPlayers)
        {
           player.playerBody.GetComponent<SyncPosition>().ResetTransform();
        }

        //Function that moves capsules to calculated position to view rewind
        MoveObjectFrame(hit, frameTime);
        return hit;
        
       }
       else //No rewind
       {
           RaycastHit hit;
           //Shoot ray 
           Physics.Raycast(rayOrigin, rayForward, out hit, 500.0f);
           if(hit.collider.tag == "PlayerBody") // If player is hit 
           {
                //Move capsuule on all clients to view shot
                RpcMoveCurGameObject(hit.collider.gameObject.transform.parent.transform.position);
           }
           return hit;
       }
    }

    //Move capsules based on Latency Rewind   
    private void MoveObjectRtt(RaycastHit hit, int calculatedPos)
    {
        //If player hit 
        if(hit.collider.tag == "PlayerBody")
        {
            foreach(TrackedPlayer p in trackedPlayers) //Loops through all tracked players
            {
                //Checks if player was the one that was hit
                if(p.playerBody.GetComponentInChildren<CapsuleCollider>() == hit.collider)
                {
                    //Move capsule to calculated position on server
                    prevGameObject.transform.position = p.positions[calculatedPos];
                    //Move yellow capsule to calculated position on clients
                    RpcMovePrevGameObject(p.positions[calculatedPos]);
                    //Move blue capsule to where player is on server
                    RpcMoveCurGameObject(p.positions[119]);
                    return;
                }
            }
        }
    }

    //Move capsules based on Frame Rewind
    private void MoveObjectFrame(RaycastHit hit, float frameCount)
    {
        //If player hit
        if(hit.collider.tag == "PlayerBody")
        {
           foreach(TrackedPlayer p in trackedPlayers) //Loops through all tracked players
            {
                //Checks if player was the one that was hit 
                if(p.playerBody.GetComponentInChildren<CapsuleCollider>() == hit.collider)
                {
                    //Move yellow capsule to calculated position on clients 
                    RpcMovePrevGameObject(p.playerBody.GetComponent<SyncPosition>().GetTransformAtFrame((int) frameCount));
                    //Gets most recent frame count
                    int maxFrame = p.playerBody.GetComponent<SyncPosition>().GetRecentFrameid();
                    //Move blue capsule to where player is on server
                    RpcMoveCurGameObject(p.playerBody.GetComponent<SyncPosition>().GetTransformAtFrame(maxFrame));
                    return;
                }
            }
        }
        else if(hit.collider.tag == "ExampleHit") //Used for rewind hit detection (Currently not working as intended)
        {
            //Moves yellow capsule to hit position
            RpcMovePrevGameObject(hit.collider.transform.position);
        }
    }

    //Calls UpdateFrameData on all tracked players to keep frame data up to date
    public void UpdateAllFrames()
    {
        foreach(TrackedPlayer p in trackedPlayers)
        {
            p.playerBody.GetComponent<SyncPosition>().UpdateFrameData();
        }
    }

    /*
    Functions to move capsules to target position on server and clients
    */ 

    [ClientRpc]
    public void RpcMovePrevGameObject(Vector3 toMove)
    {
        Debug.Log("Rewind Position = " + toMove);
        toMove.y = 1.0f;
        prevGameObject.transform.position = toMove;
    }

    [ClientRpc]
    public void RpcMoveCurGameObject(Vector3 toMove)
    {
        Debug.Log("Most recent server update = " + toMove);
        toMove.y = 1.0f;
        curGameObject.transform.position = toMove;
    }

    [ClientRpc]
    public void RpcMoveShotGameObject(Vector3 toMove)
    {
        Debug.Log("Trying to move shot object");
        Debug.Log("Shot Player!  Shot position = " + toMove);
        toMove.y = 1.0f;
        shotGameObject.transform.position = toMove;
    }

    [Command]
    public void CmdMoveShotGameObject(Vector3 toMove)
    {
        toMove.y = 1.0f;
        shotGameObject.transform.position = toMove;
        RpcMoveShotGameObject(toMove);
    }

    [ClientRpc]
    public void RpcChangeLatencyRewind(bool isOn)
    {
         if(!isLocalPlayer)
        {
            useLatencyRewind = isOn;
        }
        else
        {
            CmdChangeLatencyRewind(isOn);
        }
    }

    [ClientRpc]
    public void RpcChangeRewindHitDetection(bool isOn)
    {
        if(!isLocalPlayer)
        {
            useRewindHitDetection = isOn;
        }
        else
        {
            CmdChangeRewindHitDetection(isOn);
        }
    }

    [ClientRpc]
    public void RpcChangeFrameRewind(bool isOn)
    {
         if(!isLocalPlayer)
        {
            useFrameRewind = isOn;
        }
        else
        {
            CmdChangeFrameRewind(isOn);
        }
    }

    [Command]
    public void CmdChangeLatencyRewind(bool isOn)
    {
        useLatencyRewind = isOn;
    }

    [Command]
    public void CmdChangeRewindHitDetection(bool isOn)
    {
        useRewindHitDetection = isOn;
    }

    [Command]
    public void CmdChangeFrameRewind(bool isOn)
    {
        useFrameRewind = isOn;
    }

    public void UpdateUseFrameRewind(bool value)
    {
        useFrameRewind = value;
    }

    public void UpdateUseLatencyRewind(bool value)
    {
        useLatencyRewind = value;
    }

    public void UpdateUseRewindHitDetection(bool value)
    {
        useRewindHitDetection = value;
    }
}
