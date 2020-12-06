using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
 
public class MouseHandler : NetworkBehaviour
{
    // horizontal rotation speed
    public float horizontalSpeed = 1f;
    // vertical rotation speed
    public float verticalSpeed = 1f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private Camera cam;
    private GameObject player;
    
    void Start()
    {
        cam = this.GetComponentInChildren<Camera>();
        if(hasAuthority)
        {
        Camera serverCam = GameObject.Find("ServerCam").GetComponent<Camera>();
        serverCam.enabled = false;
        cam.enabled = true;
        }

    }
 
    void Update()
    {
        if(!hasAuthority) { return;}
        float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;
 
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
 
        cam.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);
        this.transform.rotation = cam.transform.rotation;
       // CmdSendRotate(xRotation, yRotation);
     // player.transform.eulerAngles = new Vector3(xRotation, 0.0f, 0.0f);
    }

    [Command]
    void CmdSendRotate(float xRotation, float yRotation)
    {
        RpcCharacterRotate(xRotation, yRotation);
    }

    [ClientRpc]
    void RpcCharacterRotate(float xRotation, float yRotation)
    { 
        Vector3 tempRotation = new Vector3(xRotation, yRotation, 0.0f);

        this.transform.eulerAngles = tempRotation;
    }
}