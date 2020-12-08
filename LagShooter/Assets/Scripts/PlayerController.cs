using UnityEngine;
using Mirror;
 
//Main Player Controller
//Controls movement, respawn, and HP

public class PlayerController : NetworkBehaviour
{
    CharacterController characterController;

    [SerializeField]
    private UIManager uiManager;

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
    private Weapon weapon;
 
    private void Start()
    {
        //Does not run if this is not local player
        if(!isLocalPlayer) {return;}
        //Gets character controller for movement
        characterController = GetComponent<CharacterController>();
        //Gets weapon script for firing 
        weapon = GetComponentInChildren<Weapon>();
        //Get UI Manager to update HP on ui 
        uiManager = GameObject.Find("CanvasMain").GetComponent<UIManager>();
        curHP = MaxHP;
        uiManager.UpdateHealth(curHP, MaxHP);

        //Gets all spawnpoints in scene
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
    }
 
    void Update()
    {
        //Does not run if not local player
        if (!isLocalPlayer) { return; }

        // Player movement - forward, backward, left, right
        float horizontal = Input.GetAxis("Horizontal") * MovementSpeed;
        float vertical = Input.GetAxis("Vertical") * MovementSpeed;
        characterController.Move((transform.right * horizontal + transform.forward * vertical) * Time.deltaTime);
        
        //Locks/Unlocks cursor, useful for debugging 
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

        // Gravity, player is always grounded in build
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

    //Takes damage on server
    //Updates on all clients as hp is a syncVar
    [Server]
    public void TakeDamage(int dmg, uint shooter)
    {
        curHP -= dmg;

        //If player has less than 1 HP it has died 
        if(curHP < 1)
        {
            curHP = 0;

            //Runs die script to increment deaths and ui 
            PlayerDie();

            //Updates kills of the player that killed this
            NetworkIdentity.spawned[shooter].GetComponent<PlayerController>().kills++;
            NetworkIdentity.spawned[shooter].GetComponent<PlayerController>().TargetKill();

            //Resets HP and calls respawn on killed client
            curHP = MaxHP;
            TargetRespawn();
        }

        //Runs on the client that has taken damage
        TargetTakenDamage();
    }

    //Runs on the server if player has died
    [Server]
    public void PlayerDie()
    {
        //Increments deaths 
        deaths++;
        Debug.Log("From server: Player has died! ");
        //Runs this on the target that has died
        TargetDie();
    }

    [TargetRpc]
    void TargetDie()
    {
        Debug.Log("You died :(");
        //Updates ui with deaths
        uiManager.UpdateDeaths(deaths);
    }

    [TargetRpc]
    public void TargetKill()
    {
        Debug.Log("You killed someone!");
        //Updates ui with kills
        uiManager.UpdateKills(kills);
    }

    [TargetRpc]
    public void TargetTakenDamage()
    {
        Debug.Log("You have been damaged");
        Debug.Log("Current HP = " + curHP);
        //Updates ui with hp after being damaged
        uiManager.UpdateHealth(curHP, MaxHP);

    }

    //Updates all ui elements at once
    public void UpdateDisplay(int prevValue, int newValue)
    {
        uiManager.UpdateHealth(curHP, MaxHP);
        uiManager.UpdateDeaths(deaths);
        uiManager.UpdateKills(kills);
    }

    //Respawn script for client that has died, called on client
    [TargetRpc]
    public void TargetRespawn()
    {
            //Resets HP
            curHP = MaxHP;
            int ranSpawn; 
            //Gets a random spawn number
            do{
            ranSpawn = Random.Range(0, 4);
            } while(ranSpawn == prevSpawn); // Ensures it wont spawn in same space as previous spawn point
            prevSpawn = ranSpawn;
            
            //Sets position to spawn position
            transform.position = spawnPoints[ranSpawn].transform.position;
            //Call respawn function on synced position script
            syncPositionScript.CmdRespawn(spawnPoints[ranSpawn].transform.position);

            Debug.Log("Respawning!  Chosen spawn = " + ranSpawn);

            //Updates UI with health after resapwn
            uiManager.UpdateHealth(curHP, MaxHP);
    }

    //Updates previous spawn 
    public void UpdatePrevSpawn(int newSpawn)
    {
        prevSpawn = newSpawn;
    }
}