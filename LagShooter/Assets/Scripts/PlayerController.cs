using UnityEngine;
using Mirror;
 
public class PlayerController : NetworkBehaviour
{
    CharacterController characterController;

    [SerializeField]
    private Transform myTransform;

    public float MovementSpeed =1;
    public int MaxHP;
    [SyncVar]
    public int curHP;
    public float Gravity = 0f;
    private float velocity = 0;
    private HealthBar hpScript;

    private Weapon weapon;
 
    private void Start()
    {
        if(!isLocalPlayer) {return;}
        characterController = GetComponent<CharacterController>();
        hpScript = GameObject.Find("HealthSlider").GetComponent<HealthBar>();
        hpScript.player = this;
        weapon = GetComponentInChildren<Weapon>();

        curHP = MaxHP;
    }
 
    void Update()
    {
        if (!isLocalPlayer) { return; }
        // if(curHP == 0)
        // {
        //     Debug.Log(curHP);
        //     Debug.Log("Should die :(");
        //     int ranSpawn = Random.Range(0, 3);
        //     ranSpawn++;
        //     GameObject resp = GameObject.Find("Respawn" + ranSpawn.ToString());
        //     myTransform.position = resp.transform.position;
        //     Debug.Log("Going to: " + resp.transform.position);
        //     Debug.Log("This transform: " + this.transform.position);
        //     curHP = MaxHP;
        // }

        // player movement - forward, backward, left, right
        float horizontal = Input.GetAxis("Horizontal") * MovementSpeed;
        float vertical = Input.GetAxis("Vertical") * MovementSpeed;
        characterController.Move((transform.right * horizontal + transform.forward * vertical) * Time.deltaTime);
        
        if(Input.GetButtonDown ("Fire1"))
        {
            weapon.Fire();
        }

        // Gravity
        if(characterController.isGrounded)
        {
            velocity = 0;
        }
        else
        {
            velocity -= Gravity * Time.deltaTime;
            characterController.Move(new Vector3(0, velocity, 0));
        }
    }

    public void TakeDamage(int dmg)
    {
        if(!isServer) {return;}
        curHP -= dmg;
        if(curHP < 0)
        {
            curHP = 0;
        }
        if(curHP == 0)
        {
            RpcRespawn();
            return;
        }
        else
        {
            return;
        }
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    [ClientRpc]
    void RpcRespawn()
    {
        if(isLocalPlayer)
        {
           // transform.position = Vector3.zero;
            int ranSpawn = Random.Range(0, 3);
             ranSpawn++;
             GameObject resp = GameObject.Find("Respawn" + ranSpawn.ToString());
             transform.position = resp.transform.position;
        }
    }
}