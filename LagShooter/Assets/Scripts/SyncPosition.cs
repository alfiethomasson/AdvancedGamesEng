using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class SyncPosition : NetworkBehaviour
{

    [SyncVar (hook = nameof(RpcSyncPosValues))]
    private Vector3 syncPos;

    [SerializeField]
    Transform curTransform;

    [SerializeField]
    private float moveSpeed = 1;

    float lerpRate;

    [SerializeField]
    private float normLerpRate = 18;
    [SerializeField]
    private float fastLerpRate = 28;

    private Vector3 lastPos;
    
    [SerializeField]
    private float threshold = 0.5f;

    private List<Vector3> syncPosList = new List<Vector3>();
    
    //Toggleable variables for different methods 

    [SyncVar]
    [SerializeField]
    private bool useAutomaticToggle = false;

    [SyncVar]
    [SerializeField]
    private bool useHistoricalLerp = false;

    [SyncVar]
    [SerializeField]
    private bool useDeadReckoning = true;

    [SyncVar]
    [SerializeField]
    private bool useInterpolation = true;

    [SyncVar]
    [SerializeField]
    private bool useLatencyRewind = false;

    [SyncVar]
    [SerializeField]
    private bool useFrameRewind = false;

    private float closeness = 0.1f;

    [SyncVar]
    private Vector3 pastMove;

    [SerializeField]
    private HitTracking hitTracker;

    //Used for frame rewinding
    public Dictionary<int, Vector3> FrameData = new Dictionary<int, Vector3>();
    public List<int> Keys = new List<int>();

    private Vector3 savedPos = new Vector3();

    private static int frameid;

    private bool overRide = false;

    void Start()
    {
        //Sets lerp rate to normal lerp rate 
        lerpRate = normLerpRate;

        //Sets sync position to current position 
        syncPos = curTransform.position;

        //Get hit tracker game object 
        hitTracker = GameObject.Find("HitTracker").GetComponent<HitTracking>();
    }

    // Update 
    void Update()
    {
        LerpPosition();
        //If automatic 
        if(useAutomaticToggle)
        {
        CheckPing();
        }
    }

    void FixedUpdate()
    {
         RpcSendToClient();
    }

    //If automatic is selected
    void CheckPing()
    {
        //Check if ping is over 100
        if((NetworkTime.rtt * 1000) > 100)
        {
            //If it is, turn extrapolation on 
           if(!useDeadReckoning)
           {
                CmdChangeExtrapolation(true);
           }
        }
        else if(NetworkTime.rtt * 1000 < 100) // If ping below 100
        {
            if(useDeadReckoning) // Then turn extrapolation off
            {
                CmdChangeExtrapolation(false);
            }
        }
    }

    //Lerp position 
    void LerpPosition()
    {
        if(!isLocalPlayer) // If not local player
        {
            if(useHistoricalLerp) // If using historical lerp 
            {
                HistoricalLerp();
            }
            else
            {
                NormalLerp(); //Normal lerp!
            }
        }
    }

    void NormalLerp()
    {
        if(!overRide) //If override not active
        {
        if(useInterpolation)
        {
            if(useDeadReckoning)
            {
                //Lerp position, using the estimation of next position as well
                curTransform.position = Vector3.Lerp(curTransform.position, syncPos + (pastMove * moveSpeed * Time.deltaTime), Time.deltaTime * lerpRate);
                //Updates previous move to equal the new move 
                pastMove += (pastMove * moveSpeed * Time.deltaTime);
            }
            else
            {
                //Lerp normally to new position 
                curTransform.position = Vector3.Lerp(curTransform.position, syncPos, Time.deltaTime * lerpRate);
            }
        }
        else
        {
            if(useDeadReckoning)
            {
                //Set position to new position estimation 
                curTransform.position = syncPos + (pastMove * moveSpeed * Time.deltaTime);
            }
            else
            {
                //Set new position 
                curTransform.position = syncPos;
            }
        }
        if(useDeadReckoning)
        {
            //No interpolation but extrapolation
            curTransform.position += (pastMove * moveSpeed * Time.deltaTime);
        }
        }
    }

    void HistoricalLerp()
    {
        if(syncPosList.Count > 0)
        {
            if(useInterpolation)
            {
            curTransform.position = Vector3.Lerp(curTransform.position, syncPosList[0], Time.deltaTime * lerpRate);
            }
            else
            {
                curTransform.position = syncPosList[0];
            }

            if(Vector3.Distance(curTransform.position, syncPosList[0]) < closeness)
            {
                syncPosList.RemoveAt(0);
            }

            if(syncPosList.Count > 10)
            {
                lerpRate = fastLerpRate;
            }
            else
            {
                lerpRate = normLerpRate;
            }
        }
    }

    //Called on server when player has moved 
    [Command]
    void CmdSendPosition(Vector3 pos)
    {
        //Updates synced position variable, updates on clients as well as it is a syncvar
        syncPos = pos;
        //Updates the past move based on movement
        pastMove = syncPos - curTransform.position;
        //Updates all frames in hit tracker 
        hitTracker.UpdateAllFrames();
    }

    //Called on the server to update frame data for use in rewind time 
    //Tracks posiions of players in a dictionary and uses the framecount on the server as a key
    [Server]
    public void UpdateFrameData()
    {
        if(!overRide) // If not overriding 
        {
            if(Keys.Count > 120) // Check if Keys list is full 
            {
                //If so, remove oldest variables from lists 
                int key = Keys[0];
                Keys.RemoveAt(0);
                FrameData.Remove(key);
            }
            //Tests if value already exists 
            Vector3 test;
            if(FrameData.TryGetValue(Time.frameCount, out test))
            {
                FrameData[Time.frameCount] = curTransform.position; // Updates dictionary 
            }
            else
            {
                FrameData.Add(Time.frameCount, curTransform.position); // Adds to dicitonary if doesnt exist
            }
            //Adds the key to the list 
            Keys.Add(Time.frameCount);
            //Sends the updated frame id to all clients 
            RpcUpdateFrameId(Time.frameCount);
        }
    }

    //Updates the frameid at the current moment on all clients 
    [ClientRpc]
    public void RpcUpdateFrameId(int frameId)
    {
        frameid = frameId;
    }

    //returns the frameid of the client 
    public int GetFrameId()
    {
        return frameid;
    }

    //Gets most recent key 
    public int GetRecentFrameid()
    {
        return Keys[Keys.Count - 1];
    }

    //Sets the transform of player to the point in the framedata dictionary
    //Used for rewind time, setting player back to where they were at a point 
    public void SetNewTransform(int frameid)
    {
        overRide = true; // Sets override to true 
        //Override is used to help consistency of shooting and raycasts (wip), still does not always work 
        //Updates the saved position to return to afterwards 
        savedPos = curTransform.position;
        //Sets position to rewinded pos
        curTransform.position = FrameData[frameid];
    }

    //Resets player back to where they were
    public void ResetTransform()
    {
        //Sets position to saved position 
        curTransform.position = savedPos;
        //Disable override 
        overRide = false;
    }

    //Returns the transform of player at a specified frame 
    public Vector3 GetTransformAtFrame(int frameid)
    {
        return FrameData[frameid];
    }
    
    //Checks for movement on player and updates others if so 
    [ClientCallback]
    void RpcSendToClient()
    {
        if(isLocalPlayer) // If this is local palyer 
        {
        if(!useDeadReckoning)
        {
        if(Vector3.Distance(curTransform.position, lastPos) > threshold) // If threshold is exceeded, can add a threshold for minimum movement if network is strained
        {
            //Send position to the server
            CmdSendPosition(curTransform.position);
            //Updates last position to current position 
            lastPos = curTransform.position;
        }
        }
        else
        {
            CmdSendPosition(curTransform.position);
            lastPos = curTransform.position;
        }
        }
    }

    //Called when syncpos is updated
    [ClientCallback]
    void RpcSyncPosValues(Vector3 oldPos, Vector3 recentPos)
    {
        //Sets syncpos
        syncPos = recentPos;
        //Updates the previous move 
        pastMove = syncPos - oldPos;
    }

    //Called on server when player needs to respawn 
    //Still buggy 
    [Command]
    public void CmdRespawn(Vector3 newPos)
    {
        if(!overRide)
        {
        //Sets position 
        syncPos = newPos;
        curTransform.position = newPos;
        }
    }

    /*
    All of the following functions update their respective  variables on all instances
    Also updates the toggle on the UI to reflect this 
    */

    [ClientRpc]
    public void RpcChangeHistoricalLerp(bool isOn)
    {
        if(!isLocalPlayer)
        {
        syncPosList.Clear();
        }
        else
        {
            CmdChangeHistoricalLerp(isOn);
            Toggle tog = GameObject.Find("HistoricalLerp").GetComponent<Toggle>();
            tog.isOn = isOn;
        }
    }

    [ClientRpc]
    public void RpcChangeInterpolation(bool isOn)
    {
        if(!isLocalPlayer)
        {
            useInterpolation = isOn;
        }
        else
        {
           CmdChangeInterpolation(isOn);
           Toggle tog = GameObject.Find("Interpolation").GetComponent<Toggle>();
           tog.isOn = isOn;
        }
    }

    [ClientRpc]
    public void RpcChangeExtrapolation(bool isOn)
    {
        if(!isLocalPlayer)
        {
            useDeadReckoning = isOn;
        }
        else
        {
            CmdChangeExtrapolation(isOn);
            Toggle tog = GameObject.Find("Extrapolation").GetComponent<Toggle>();
            tog.isOn = isOn;
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
            Toggle tog = GameObject.Find("FrameRewind").GetComponent<Toggle>();
           tog.isOn = isOn;
        }
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
            Toggle tog = GameObject.Find("LatencyRewind").GetComponent<Toggle>();
            tog.isOn = isOn;
        }
    }

    [ClientRpc]
    public void RpcChangeAutomatic(bool isOn)
    {
        if(!isLocalPlayer)
        {
            useAutomaticToggle = isOn;
            if(useAutomaticToggle)
            {
                useDeadReckoning = false;
                useHistoricalLerp = false;
                useInterpolation = true;
            }
        }
        else
        {
            CmdChangeAutomatic(isOn);
            CmdChangeInterpolation(isOn);
            Toggle tog = GameObject.Find("Automatic").GetComponent<Toggle>();
            tog.isOn = isOn;
        }
    }

    [Command]
    public void CmdChangeHistoricalLerp(bool isOn)
    {
        useHistoricalLerp = isOn;
        Toggle tog = GameObject.Find("HistoricalLerp").GetComponent<Toggle>();
        tog.isOn = isOn;
    }

    [Command]
    public void CmdChangeInterpolation(bool isOn)
    {
        useInterpolation = isOn;
        Toggle tog = GameObject.Find("Interpolation").GetComponent<Toggle>();
        tog.isOn = isOn;
    }

    [Command]
    public void CmdChangeExtrapolation(bool isOn)
    {
        useDeadReckoning = isOn;
        Toggle tog = GameObject.Find("Extrapolation").GetComponent<Toggle>();
        tog.isOn = isOn;
    }

    [Command]
    public void CmdChangeAutomatic(bool isOn)
    {
        useAutomaticToggle = isOn;
        Toggle tog = GameObject.Find("Automatic").GetComponent<Toggle>();
        tog.isOn = isOn;
    }

    [Command]
    public void CmdChangeLatencyRewind(bool isOn)
    {
        useLatencyRewind = isOn;
        Toggle tog = GameObject.Find("LatencyRewind").GetComponent<Toggle>();
        tog.isOn = isOn;
    }

    [Command]
    public void CmdChangeFrameRewind(bool isOn)
    {
        useFrameRewind = isOn;
        Toggle tog = GameObject.Find("FrameRewind").GetComponent<Toggle>();
        tog.isOn = isOn;
    }
}
