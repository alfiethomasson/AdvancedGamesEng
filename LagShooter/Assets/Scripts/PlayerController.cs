using UnityEngine;
using Mirror;
 
public class PlayerController : NetworkBehaviour
{
    CharacterController characterController;

    [SerializeField]
    private UIManager uiManager;

    [SerializeField]
    private Transform myTransform;

    [SerializeField]
    private SyncPosition syncPositionScript;

    public float MovementSpeed =1;
    public int MaxHP;
    [SyncVar (hook = nameof(UpdateDisplay))]
    public int curHP;

    public int prevSpawn;

    public GameObject[] spawnPoints;

    [SyncVar (hook = nameof(UpdateDisplay))]
    public int deaths = 0;
    [SyncVar (hook = nameof(UpdateDisplay))]
    public int kills = 0;
    public float Gravity = 0f;
    private float velocity = 0;
    private HealthBar hpScript;

    private Weapon weapon;
 
    private void Start()
    {
        if(!isLocalPlayer) {return;}
        characterController = GetComponent<CharacterController>();
        weapon = GetComponentInChildren<Weapon>();
        uiManager = GameObject.Find("CanvasMain").GetComponent<UIManager>();
        curHP = MaxHP;
        uiManager.UpdateHealth(curHP, MaxHP);

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
    }
 
    void Update()
    {
        if (!isLocalPlayer) { return; }
        // if(curHP == 0)
        // {
        //     CmdRespawn();
        //     return;
        // }

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
        
        if(Input.GetKeyDown(KeyCode.M))
        {
            Cursor.visible = !Cursor.visible;
            if(Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        // if(Input.GetButtonDown ("Fire1"))
        // {
        //     weapon.Fire();
        // }

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

    [Server]
    public void TakeDamage(int dmg, uint shooter)
    {
        curHP -= dmg;
        TargetTakenDamage();
        if(curHP < 1)
        {
            curHP = 0;
            PlayerDie();
            NetworkIdentity.spawned[shooter].GetComponent<PlayerController>().kills++;
            NetworkIdentity.spawned[shooter].GetComponent<PlayerController>().TargetKill();
            curHP = MaxHP;
           // RpcRespawn();
           TargetRespawn();
            //CmdRespawn();
        }
        TargetTakenDamage();
        // if(curHP == 0)
        // {
        //     CmdRespawn();
        //     return;
        // }
        // else
        // {
        //     return;
        // }
    }

    [Server]
    public void PlayerDie()
    {
        deaths++;
        Debug.Log("From server: Player has died! ");
        TargetDie();
    }

    [TargetRpc]
    void TargetDie()
    {
        Debug.Log("You died :(");
        uiManager.UpdateDeaths(deaths);
    }

    [TargetRpc]
    public void TargetKill()
    {
        Debug.Log("You killed someone!");
        uiManager.UpdateKills(kills);
    }

    [TargetRpc]
    public void TargetTakenDamage()
    {
        //Here u should update HP on UI.  Instead of update do it here !!
        Debug.Log("You have been damaged");
        Debug.Log("Current HP = " + curHP);
        uiManager.UpdateHealth(curHP, MaxHP);

    }

     public void UpdateDisplay(int prevValue, int newValue)
    {
        uiManager.UpdateHealth(curHP, MaxHP);
        uiManager.UpdateDeaths(deaths);
        uiManager.UpdateKills(kills);
    }

    public override void OnStartLocalPlayer()
    {
        //GetComponent<MeshRenderer>().material.color = Color.blue;
    }

    // [Command]
    // void CmdRespawn()
    // {
    //     curHP = MaxHP;
    //     RpcRespawn();
    // }

    [ClientRpc]
    void RpcRespawn()
    {
        curHP = MaxHP;
        if(isLocalPlayer)
        {
            int ranSpawn;
            do{
            ranSpawn = Random.Range(0, 4);
            } while(ranSpawn == prevSpawn);
            prevSpawn = ranSpawn;
            ranSpawn++;
             GameObject resp = GameObject.Find("Respawn" + ranSpawn.ToString());
             transform.position = resp.transform.position;
             Debug.Log("Respawning!  Chosen spawn = " + ranSpawn);
             uiManager.UpdateHealth(curHP, MaxHP);
        }
    }

    [TargetRpc]
    public void TargetRespawn()
    {
         curHP = MaxHP;
            int ranSpawn;
            do{
            ranSpawn = Random.Range(0, 4);
            } while(ranSpawn == prevSpawn);
            prevSpawn = ranSpawn;
            //ranSpawn++;
             //GameObject resp = GameObject.Find("Respawn" + ranSpawn.ToString());
             //transform.position = resp.transform.position;
             transform.position = spawnPoints[ranSpawn].transform.position;
             //syncPositionScript.CmdRespawn(resp.transform.position);
             syncPositionScript.CmdRespawn(spawnPoints[ranSpawn].transform.position);
             Debug.Log("Respawning!  Chosen spawn = " + ranSpawn);
             uiManager.UpdateHealth(curHP, MaxHP);
    }

    public void UpdatePrevSpawn(int newSpawn)
    {
        prevSpawn = newSpawn;
    }
}