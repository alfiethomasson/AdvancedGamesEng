using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
********************************************************************************************************************
This script syncs rotation of players on clients
Currently it is not set to use any lag compensation techniques and is just bare bones, but syncs rotation correctly
The method of syncing is near identical to SyncPosition
********************************************************************************************************************
*/

public class SyncRotation : NetworkBehaviour
{
    [SyncVar (hook = nameof(RpcPlayerRotationSync))]
    private float syncPlayerRotation;

    [SyncVar (hook = nameof(RpcCamRotationSync))]
    private float syncCamRotation;

    [SerializeField]
    private Transform curTransform;

    [SerializeField]
    private Transform camTransform;

    private float lerpRate;

    [SerializeField]
    private float normalLerpRate = 50;

    [SerializeField]
    private float fasterLerpRate = 50;

    private float lastPlayerRot;
    private float lastCamRot;
    
    [SerializeField]
    private float threshold = 1.0f;

    private List<float> syncPlayerRotationList = new List<float>();
    private List<float> syncCamRotList = new List<float>();

    [SerializeField]
    private float closeness = 0.5f;

    [SyncVar]
    [SerializeField]
    private bool useHistoricalLerp;

    [SyncVar]
    [SerializeField]
    private bool useInterpolation = true;

    void Start()
    {
        lerpRate = normalLerpRate;
    }

    void Update()
    {
        lerpRotate();
    }

    void FixedUpdate()
    {
        RpcSendToClient();
    }

    void lerpRotate()
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

    void HistoricalLerp()
    {
        if(syncPlayerRotationList.Count > 0)
        {
            LerpPlayerRot(syncPlayerRotationList[0]);
            if(Mathf.Abs(curTransform.localEulerAngles.y - syncPlayerRotationList[0]) < closeness)
            {
                syncPlayerRotationList.RemoveAt(0);
            }

            if(syncPlayerRotationList.Count > 20)
            {
                lerpRate = fasterLerpRate;
            }
            else
            {
                lerpRate = normalLerpRate;
            }
        }

        if(syncCamRotList.Count > 0)
        {
            LerpCamRot(syncCamRotList[0]);
            if(Mathf.Abs(camTransform.localEulerAngles.x - syncCamRotList[0]) < closeness)
            {
                syncCamRotList.RemoveAt(0);
            }
        }
    }

    void NormalLerp()
    {
        LerpPlayerRot(syncPlayerRotation);
    }

    void LerpPlayerRot(float newRot)
    {
        Vector3 playerNewRot = new Vector3(0, newRot, 0);
        if(useInterpolation)
        {
       // curTransform.rotation = Quaternion.Lerp(curTransform.rotation, Quaternion.Euler(playerNewRot), Time.deltaTime * lerpRate);
        }
        else
        {
            curTransform.rotation = Quaternion.Euler(playerNewRot);
        }
    }

    void LerpCamRot(float newRot)
    {
        Vector3 camNewRot = new Vector3(newRot, 0, 0);
        if(useInterpolation)
        {
       // camTransform.localRotation = Quaternion.Lerp(camTransform.localRotation, Quaternion.Euler(camNewRot), Time.deltaTime * lerpRate);
        }
        else
        {
           // camTransform.rotation = Quaternion.Euler(camNewRot);
        }
    }

    bool CheckThreshold(float rotation1, float rotation2)
    {
        if(Mathf.Abs(rotation1 - rotation2) > threshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [Command]
    void CmdSendRotate(float playerRot, float camRot)
    {
        syncPlayerRotation = playerRot;
        syncCamRotation = camRot;
    }

    [ClientRpc]
    void RpcSendToClient()
    {
        if(isLocalPlayer)
        {
            if(CheckThreshold(curTransform.localEulerAngles.y, lastPlayerRot) || CheckThreshold(camTransform.localEulerAngles.x, lastCamRot))
            {
                lastPlayerRot = curTransform.localEulerAngles.y;
                lastCamRot = camTransform.localEulerAngles.x;
                CmdSendRotate(lastPlayerRot, lastCamRot);
            }
        }
    }

    [ClientRpc]
    void RpcPlayerRotationSync(float oldrot, float newrot)
    {
        syncPlayerRotation = newrot;
        if(useHistoricalLerp)
        {
            syncPlayerRotationList.Add(syncPlayerRotation);
        }
    }

    [ClientRpc]
    void RpcCamRotationSync(float oldrot, float newrot)
    {
        syncCamRotation = newrot;
        if(useHistoricalLerp)
        {
            syncCamRotList.Add(syncCamRotation);
        }
    }

    [ClientRpc]
    public void RpcChangeHistoricalLerp(bool isOn)
    {
        if(!isLocalPlayer)
        {
            syncPlayerRotationList.Clear();
            syncCamRotList.Clear();
            useHistoricalLerp = isOn;
        }
    }

    [ClientRpc]
    public void RpcChangeInterpolation(bool isOn)
    {
        if(!isLocalPlayer)
        {
            useInterpolation = isOn;    
        }
    }
}
