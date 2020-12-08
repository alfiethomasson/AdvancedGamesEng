using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;
 
//Script to look around scene with mouse

public class MouseHandler : NetworkBehaviour
{
    // horizontal rotation speed
    public float horizontalSpeed = 1f;
    // vertical rotation speed
    public float verticalSpeed = 1f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    private Camera cam;
    
    void Start()
    {
        //Gets camera attached to this player
        cam = this.GetComponentInChildren<Camera>();

        //If the client has authority over camera (local player)
        if(hasAuthority)
        {
            //Finds server camera
            Camera serverCam = GameObject.Find("ServerCam").GetComponent<Camera>();
            //... and disables it
            serverCam.enabled = false;
            //but enables this one for the player!
            cam.enabled = true;
        }

    }
 
    void Update()
    {
        if(!hasAuthority) { return;} // Does not run if no authority

        //Get inputs for looking around 
        float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;
 
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
 
        //Moves camera around 
        cam.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);
        //Moves player transform based on camera
        this.transform.rotation = cam.transform.rotation;
    }

    [ClientRpc]
    void RpcCharacterRotate(float xRotation, float yRotation)
    { 
        Vector3 tempRotation = new Vector3(xRotation, yRotation, 0.0f);

        this.transform.eulerAngles = tempRotation;
    }
}