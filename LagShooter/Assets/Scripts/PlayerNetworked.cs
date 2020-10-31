using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworked : NetworkBehaviour
{
 private CharacterController characterController;

    public float MovementSpeed =1;
    public int MaxHP;
    [SyncVar]
    public int curHP;
    public float Gravity = 9.8f;
    private float velocity = 0;
    private HealthBar hpScript;

    private Vector3 prevPos;

    public bool interpolation;

    private Weapon weapon;
    // Start is called before the first frame update
    void Start()
    {
        interpolation = true;
        characterController = GetComponent<CharacterController>();
       // hpScript = GameObject.Find("HealthSlider").GetComponent<HealthBar>();
        //hpScript.player = this
        curHP = MaxHP;
        if(!isLocalPlayer) {return;}
    }

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer) {return;}
        float horizontal = Input.GetAxis("Horizontal") * MovementSpeed;
        float vertical = Input.GetAxis("Vertical") * MovementSpeed;
        CmdSendMove(horizontal, vertical);
    }

    [Command]
    void CmdSendMove(float hor, float ver)
    {
        RpcCharacterMove(hor, ver);
    }

    [ClientRpc]
    void RpcCharacterMove(float horizontal, float vertical)
    {   
        if(interpolation)
        {
        Vector3 newPos = (transform.right * horizontal + transform.forward * vertical) * Time.deltaTime;
        Vector3 lerpedPos = Vector3.Lerp(prevPos, newPos, 0.5f);
        //float midPos = (newPos + prevPos) / 2;
        characterController.Move(lerpedPos);
        prevPos = newPos;
        }
        else
        {
            
        }
    }
}
