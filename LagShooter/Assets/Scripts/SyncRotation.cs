using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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

    // Update is called once per frame
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
             //   Debug.Log("Should historical Lerp");
                HistoricalLerp();
            }
            else
            {
                NormalLerp();
            }
       // curTransform.rotation = Quaternion.Lerp(curTransform.rotation, syncPlayerRotation, Time.deltaTime * lerpRate);
        //camTransform.rotation = Quaternion.Lerp(camTransform.rotation, syncCamRotation, Time.deltaTime * lerpRate);
        }
    }

    void HistoricalLerp()
    {
        Debug.Log(syncPlayerRotationList.Count + "  Player Rotation List");
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

        Debug.Log(syncCamRotList.Count + "  Camera Rotation List");
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
       // LerpCamRot(syncCamRotation);
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
        //Debug.Log("Transmitting rotation!");
        syncPlayerRotation = playerRot;
        syncCamRotation = camRot;
    }

    [ClientRpc]
    void RpcSendToClient()
    {
        if(isLocalPlayer)
        {
            //if(Quaternion.Angle(curTransform.rotation, lastPlayerRot) > threshold || Quaternion.Angle(camTransform.rotation, lastCamRot) > threshold)
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
        Debug.Log("Rotate called");
        syncPlayerRotation = newrot;
        if(useHistoricalLerp)
        {
        syncPlayerRotationList.Add(syncPlayerRotation);
        }
       //  Debug.Log("Player List Count = " + syncCamRotList.Count);
    }

    [ClientRpc]
    void RpcCamRotationSync(float oldrot, float newrot)
    {
        syncCamRotation = newrot;
        if(useHistoricalLerp)
        {
        syncCamRotList.Add(syncCamRotation);
        }
       // Debug.Log("Cam List Count = " + syncCamRotList.Count);
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

    // [ClientRpc]
    // public void RpcChangeExtrapolation()
    // {
    //     if(isLocalPlayer)
    //     {
    //     = !useInterpolation;
    //     }
    // }
}
