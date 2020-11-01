using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncPosition : NetworkBehaviour
{

    [SyncVar (hook = nameof(RpcSyncPosValues))]
    private Vector3 syncPos;

    [SerializeField]
    Transform curTransform;

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
    private bool useHistoricalLerp = false;

    [SyncVar]
    [SerializeField]
    private bool useDeadReckoning = false;

    [SyncVar]
    [SerializeField]
    private bool useInterpolation = true;

    private float closeness = 0.1f;

    void Start()
    {
        lerpRate = normLerpRate;
    }

    // Update is called once per frame
    void Update()
    {
        LerpPosition();
    }

    void FixedUpdate()
    {
        RpcSendToClient();
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
        if(useInterpolation)
        {
        curTransform.position = Vector3.Lerp(curTransform.position, syncPos, Time.deltaTime * lerpRate);
        }
        else
        {
            curTransform.position = syncPos;
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
        syncPos = pos;
    }
    
    [ClientRpc]
    void RpcSendToClient()
    {
        if(isLocalPlayer && Vector3.Distance(curTransform.position, lastPos) > threshold)
        {
            CmdSendPosition(curTransform.position);
            lastPos = curTransform.position;
        }
    }

    [ClientRpc]
    void RpcSyncPosValues(Vector3 oldPos, Vector3 recentPos)
    {
        syncPos = recentPos;
        syncPosList.Add(syncPos);
    }

    [ClientRpc]
    public void RpcChangeHistoricalLerp()
    {
        if(isLocalPlayer)
        {
        syncPosList.Clear();
        useHistoricalLerp = !useHistoricalLerp;
        }
    }

    [ClientRpc]
    public void RpcChangeInterpolation()
    {
        if(isLocalPlayer)
        {
        useInterpolation = !useInterpolation;
        }
    }
}
