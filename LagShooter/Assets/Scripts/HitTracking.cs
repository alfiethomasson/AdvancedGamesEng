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
        if(useLatencyRewind)
        {
        for(int i = 0; i < trackedPlayers.Count; i++)
        {
            trackedPlayers[i].Update();
        }
        }
        //UpdateAllFrames();
    }

    public void AddPlayerToList(GameObject newPlayer)
    {
        playerGameObjects.Add(newPlayer);
        TrackedPlayer tempPlayer = new TrackedPlayer(newPlayer.gameObject);
       // tempPlayer.SetPlayerBody(newPlayer);
        trackedPlayers.Add(tempPlayer);
    }

    [Server]
    public RaycastHit BeginComputeHit(double latency, Vector3 rayOrigin, Vector3 rayForward, float frameTime)
    {
        //Debug.Log("BEGIN COMPUTING HIT ITS AWESOME");
       // Debug.Log("Server Tick Rate * 1000 = " + (int)(serverTickRate));
       // Debug.Log("Latency Time * 1000 = " + (int)(latency * 1000));
       if(useLatencyRewind)
       {
        latency = latency * 1000;
         int calculatedPosition = (int)Mathf.Floor((float)(latency * 4) / (float)serverTickRate);
        if(calculatedPosition > 119) {calculatedPosition = 119;}
        calculatedPosition = 119 - calculatedPosition;
        calculatedPosition = 0;
         Debug.Log("Calculated Position = " + (int)calculatedPosition);

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

        MoveObjectRtt(hit, calculatedPosition);

        return hit;

       }
       else if(useFrameRewind)
       {

        if(frameTime == Time.frameCount)
        {
            frameTime--;
        }

        RaycastHit hit = new RaycastHit();
        //Physics.Raycast(rayOrigin,rayForward, out hit, 500.0f);

        foreach(TrackedPlayer player in trackedPlayers)
        {
            if(!useRewindHitDetection)
            {
                player.playerBody.GetComponent<SyncPosition>().SetNewTransform((int)frameTime);
            // Physics.Raycast(rayOrigin,rayForward, out hitTemp, 500.0f);
            // if(hitTemp.collider.tag == "PlayerBody")
            // {
            // Debug.Log("Hit temp hit: " + hitTemp.collider.gameObject.transform.parent.transform.position);
            // }
                Physics.Raycast(rayOrigin,rayForward, out hit, 500.0f);
                if(hit.collider.tag == "PlayerBody")
                {
                   break;
                }
            }
            else
            {
                //prevGameObject
                Debug.Log("Should do this thing");
                prevGameObject.transform.position = player.playerBody.GetComponent<SyncPosition>().GetTransformAtFrame((int)frameTime);
                Physics.Raycast(rayOrigin, rayForward, out hit, 500.0f);
                if(hit.collider.tag == "ExampleHit")
                {
                    Debug.Log("Succesfully hit yellow example!");
                    break;
                }
            }

        }
        //Physics.Raycast(rayOrigin,rayForward, out hit, 500.0f);
        foreach(TrackedPlayer player in trackedPlayers)
        {
           player.playerBody.GetComponent<SyncPosition>().ResetTransform();
        }

        Debug.Log("Hit collider at: " + hit.collider.gameObject.transform.parent.transform.position);
        MoveObjectFrame(hit, frameTime);
        return hit;
        
       }
       else
       {
           RaycastHit hit;
           Physics.Raycast(rayOrigin, rayForward, out hit, 500.0f);
           return hit;
       }
    }

    private void MoveObjectRtt(RaycastHit hit, int calculatedPos)
    {
        if(hit.collider.tag == "PlayerBody")
        {
            foreach(TrackedPlayer p in trackedPlayers)
            {
                if(p.playerBody.GetComponentInChildren<CapsuleCollider>() == hit.collider)
                {
                    prevGameObject.transform.position = p.positions[calculatedPos];
                    RpcMovePrevGameObject(p.positions[calculatedPos]);
                    RpcMoveCurGameObject(p.positions[119]);
                    return;
                }
            }
        }
    }

    private void MoveObjectFrame(RaycastHit hit, float frameCount)
    {
        if(hit.collider.tag == "PlayerBody")
        {
           foreach(TrackedPlayer p in trackedPlayers)
            {
                if(p.playerBody.GetComponentInChildren<CapsuleCollider>() == hit.collider)
                {
                    RpcMovePrevGameObject(p.playerBody.GetComponent<SyncPosition>().GetTransformAtFrame((int) frameCount));
                    int maxFrame = p.playerBody.GetComponent<SyncPosition>().GetRecentFrameid();
                    RpcMoveCurGameObject(p.playerBody.GetComponent<SyncPosition>().GetTransformAtFrame(maxFrame));
                    return;
                }
            }
        }
    }

    public void UpdateAllFrames()
    {
        foreach(TrackedPlayer p in trackedPlayers)
        {
            p.playerBody.GetComponent<SyncPosition>().UpdateFrameData();
        }
    }

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
           // Debug.Log("Calling on this client!");
            useLatencyRewind = isOn;
        }
        else
        {
            //Debug.Log("Calling on this client!");
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
           // Debug.Log("Calling on this client!");
            useFrameRewind = isOn;
        }
        else
        {
            //Debug.Log("Calling on this client!");
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
