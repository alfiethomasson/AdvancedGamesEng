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

    NetworkPingDisplay networkPingDisplay;

    public Dictionary<int, Vector3> FrameData = new Dictionary<int, Vector3>();
    public List<int> Keys = new List<int>();

    private Vector3 savedPos = new Vector3();

    private static int frameid;

    private bool overRide = false;

    void Start()
    {
        lerpRate = normLerpRate;
        networkPingDisplay = GameObject.Find("Network Manager").GetComponent<NetworkPingDisplay>();
        syncPos = curTransform.position;
        hitTracker = GameObject.Find("HitTracker").GetComponent<HitTracking>();
    }

    // Update is called once per frame
    void Update()
    {
        LerpPosition();
        if(useAutomaticToggle)
        {
        CheckPing();
        }
    }

    void FixedUpdate()
    {
         RpcSendToClient();
    }

    // [Command]
    // private void CmdSendToClient()
    // {
    //     RpcSendToClient();
    // }

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
               // CmdChangeHistoricalLerp(false);
           }
         //  Debug.Log("Above 100");
        }
        else if(NetworkTime.rtt * 1000 < 100) // If ping below 100
        {
            if(useDeadReckoning) // Then turn extrapolation off
            {
                CmdChangeExtrapolation(false);
                //CmdChangeHistoricalLerp(true);
            }
            Debug.Log("Below 100");
        }
        Debug.Log("Extrapolation on = " +  useDeadReckoning);
        //Debug.Log("Network Time rtt * 1000 = " + (NetworkTime.rtt * 1000));
    }

    void LerpPosition()
    {
        if(!isLocalPlayer)
        {
            if(useHistoricalLerp)
            {
                HistoricalLerp();
            }
            else
            {
                NormalLerp();
            }
        }
    }

    void NormalLerp()
    {
        if(!overRide)
        {
        if(useInterpolation)
        {
            if(useDeadReckoning)
            {
                
               // curTransform.position = Vector3.Lerp(curTransform.position, syncPos + pastMove, Time.deltaTime * lerpRate);
                curTransform.position = Vector3.Lerp(curTransform.position, syncPos + (pastMove * moveSpeed * Time.deltaTime), Time.deltaTime * lerpRate);
               // curTransform.position = Vector3.Lerp(curTransform.position, curTransform.position + (pastMove * moveSpeed * Time.deltaTime), Time.deltaTime * lerpRate);
                pastMove += (pastMove * moveSpeed * Time.deltaTime);
            }
            else
            {
                curTransform.position = Vector3.Lerp(curTransform.position, syncPos, Time.deltaTime * lerpRate);
            }
        }
        else
        {
            if(useDeadReckoning)
            {
                curTransform.position = syncPos + (pastMove * moveSpeed * Time.deltaTime);
            }
            else
            {
                //Debug.Log("Syncing pos: cur transform = " + curTransform.position);
               // Debug.Log("Syncing pos: sync pos = " + syncPos);
                curTransform.position = syncPos;
            }
        }
        if(useDeadReckoning)
        {
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
           // Debug.Log(syncPosList.Count.ToString());
        }
    }

    [Command]
    void CmdSendPosition(Vector3 pos)
    {
        Debug.Log("SENDING POSITTION");
        Debug.Log("here in sync pos");
        syncPos = pos;
        pastMove = syncPos - curTransform.position;
        hitTracker.UpdateAllFrames();
       // hitTracker.UpdateAllFrames();
        //UpdateFrameData();
        // if(useHistoricalLerp)
        // {
        //     syncPosList.Add(pos);
        // }
       // pastMove = new Vector3(pastMove.x, 0, pastMove.z);
        //Debug.Log(pastMove);
        //lastPos = syncPos - curTransform.position;
    }

    [Server]
    public void UpdateFrameData()
    {
        if(!overRide)
        {
        //Debug.Log("Updating Frame Data");
        if(Keys.Count > 120)
        {
            int key = Keys[0];
            Keys.RemoveAt(0);
            FrameData.Remove(key);
        }

        // if(FrameData[Time.frameCount] != null)
        // {
        //     FrameData.Remove(Time.frameCount);
        // }
        FrameData.Add(Time.frameCount, curTransform.position);
        Keys.Add(Time.frameCount);
        RpcUpdateFrameId(Time.frameCount);
        }
        //Debug.Log("Adding new item tto dictionary:  At Frame Count" + Time.frameCount);
    }

    [ClientRpc]
    public void RpcUpdateFrameId(int frameId)
    {
        //Debug.Log("Updating frameid here! : " + frameid);
        //curTransform.position = syncPos;
        frameid = frameId;
    }

    public int GetFrameId()
    {
        return frameid;
    }

    public int GetRecentFrameid()
    {
        return Keys[Keys.Count - 1];
    }

    public void SetNewTransform(int frameid)
    {
        Debug.Log("Trying to read for frame: " + frameid);
        Debug.Log("Current Frame Time = " + Time.frameCount);
        Debug.Log("Oldest Key present = " + Keys[0]);
        savedPos = curTransform.position;
        Debug.Log("Setting this to: " + FrameData[frameid]);
        //curTransform.position = Vector3.Lerp(FrameData[frameid], FrameData[frameid], lerpRate);
        curTransform.position = FrameData[frameid];
        overRide = true;
    }

    public void ResetTransform()
    {
        Debug.Log("curTRANSFORM on reset = " + curTransform.position);
        curTransform.position = savedPos;
        overRide = false;
    }

    public Vector3 GetTransformAtFrame(int frameid)
    {
        return FrameData[frameid];
    }
    
    [ClientRpc]
    void RpcSendToClient()
    {
        if(isLocalPlayer)
        {
        if(!useDeadReckoning)
        {
        if(Vector3.Distance(curTransform.position, lastPos) > threshold)
        {
            //Debug.Log("THREHOLD EXCEEDED: last pos = " + lastPos);
           // Debug.Log("Should send command");
            CmdSendPosition(curTransform.position);
            lastPos = curTransform.position;
        }
        }
        else
        {
           // Debug.Log("THREHOLD EXCEEDED: last pos = " + lastPos);
            CmdSendPosition(curTransform.position);
            lastPos = curTransform.position;
        }
        }
    }

    [ClientRpc]
    void RpcSyncPosValues(Vector3 oldPos, Vector3 recentPos)
    {
        syncPos = recentPos;
        pastMove = syncPos - oldPos;
        // if(useHistoricalLerp)
        // {
        // syncPosList.Add(syncPos);
        // }
    }

    [Command]
    public void CmdRespawn(Vector3 newPos)
    {
        if(!overRide)
        {
        syncPos = newPos;
        curTransform.position = newPos;
        }
    }

    [ClientRpc]
    public void RpcChangeHistoricalLerp(bool isOn)
    {
        if(!isLocalPlayer)
        {
        syncPosList.Clear();
      //  useHistoricalLerp = isOn;
        //Debug.Log("Calling on this client!");
        }
        else
        {
            //Debug.Log("Calling on this client!");
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
           // Debug.Log("Calling on this client!");
            useInterpolation = isOn;
        }
        else
        {
           // Debug.Log("Calling on this client!");
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
           // Debug.Log("Calling on this client!");
            useDeadReckoning = isOn;
        }
        else
        {
            //Debug.Log("Calling on this client!");
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
           // Debug.Log("Calling on this client!");
            useFrameRewind = isOn;
        }
        else
        {
            //Debug.Log("Calling on this client!");
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
           // Debug.Log("Calling on this client!");
            useLatencyRewind = isOn;
        }
        else
        {
            //Debug.Log("Calling on this client!");
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
           // Debug.Log("Calling on this client!");
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
            //Debug.Log("Calling on this client!");
            CmdChangeAutomatic(isOn);
            CmdChangeInterpolation(isOn);
           // CmdChangeHistoricalLerp(isOn);
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
