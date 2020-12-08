using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : NetworkBehaviour
{
    public int damage = 1;
    public float firerate = 0.5f;
    public float range = 50.0f;
    public float shotDuration = 0.3f;

    public HitTracking hitTracker;
    public GameObject hitTrackerGameObject;

    public Transform muzzle;

    [SerializeField]
    private GameObject bulletHitPrefab;

    [SerializeField]
    private UIManager uiManager;

    [SyncVar (hook = nameof(UpdateAmmoDisplay))]
    public int curAmmo;

    public int maxAmmo;

    private Camera fpsCam;
    private LineRenderer laser;
    private float nextShot;
    private bool isReloading = false;

    public SyncPosition syncPosParent;

    void Start()
    {
        //Gets linerenderer component to display shot paths
        laser = GetComponent<LineRenderer>();
        //Gets camera attached to same player
        fpsCam = GetComponentInChildren<Camera>();

        hitTrackerGameObject = GameObject.Find("HitTracker");
        hitTracker = hitTrackerGameObject.GetComponent<HitTracking>();

        //Checks if local player and if so assigns canvas (used for reloading) and movement sync script
        if(isLocalPlayer)
        {
            uiManager = GameObject.Find("CanvasMain").GetComponent<UIManager>();
            syncPosParent = gameObject.GetComponentInParent<SyncPosition>();
        }

        curAmmo = maxAmmo;
        uiManager.UpdateAmmo(curAmmo, maxAmmo);
    }

    void Update()
    {
        //Do not run update if not local player
        if(!isLocalPlayer) { return;}

        //Reload 
        if(Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            isReloading = true;
            StartCoroutine(Reload());
        }

        //Fire weapon!
        if (Input.GetButtonDown ("Fire1") && ReadyToFire()) 
        {
            //Ensures the frameid is up to date
            CmdUpdateAllFrames();

            //Reset time to fire
            nextShot = Time.time + firerate;
            
            // Gets point to start raycast from, middle of camera
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.5f));

            //Current latency
            double latencyTime = NetworkTime.rtt;

            //Updates ammo count
            curAmmo--;
            uiManager.UpdateAmmo(curAmmo, maxAmmo);

            //Local raycast to check hits on player screen, not server authorative
            RaycastHit hit;
            Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit);
            Debug.Log("Hit: " + hit.collider.tag);

            //Gets current frameid
            float frameTime = syncPosParent.GetFrameId();

            //Move red capsule to hit point, shows where shot player was on local screen
            if(hit.collider.tag == "PlayerBody")
            {
               Debug.Log("Hit on local client");
               CmdBeginMove(hit.collider.gameObject.transform.parent.transform.position);
            }

            //Begins server check of raycast
            CmdCheckShot(latencyTime, rayOrigin, fpsCam.transform.forward, frameTime);
        }
    }

    //Begins updates frameid on all clients
    [Command]
    public void CmdUpdateAllFrames()
    {
        hitTracker.UpdateAllFrames();
    }

    [Command]
    public void CmdBeginMove(Vector3 toMove)
    {
        Debug.Log("CmdBeginMove");
        toMove.y = 1.0f;
        hitTracker.RpcMoveShotGameObject(toMove);
    }

    //Returns true/false depending on whether player is able to fire or not
    private bool ReadyToFire()
    {
        if(Time.time > nextShot && curAmmo > 0 && !isReloading)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Begins server check of raycast to see if it hits a player
    [Command]
    public void CmdCheckShot(double latency, Vector3 rayOrigin, Vector3 rayPoint, float framecount)
    {
        //Runs the hit tracking script (used for rewind time)
        RaycastHit hit;
        hit = hitTracker.BeginComputeHit(latency, rayOrigin, rayPoint, framecount);

        //Sets the linerenderer on the server
        laser.SetPosition(0, muzzle.position);
        laser.SetPosition(1, hit.point);
        Debug.Log("I hit: " + hit.collider.tag);

        //Calls script to instantiate bullet hole on local clients
        RpcPlayerShot(GetComponent<NetworkIdentity>().netId, hit.point, hit.normal);

        //If hit object is player then deal damage
         if (hit.collider.tag == "PlayerBody")
        {
            GameObject enemy = hit.collider.gameObject;
            PlayerController enemyController = enemy.GetComponentInParent<PlayerController>();
            enemyController.TakeDamage(40, GetComponentInParent<NetworkIdentity>().netId);
        }

        //Renders shot line on clients 
        RpcClientLine(hit.point);   
    }

    [ClientRpc]
    void RpcPlayerShot(uint shooter, Vector3 impactPos, Vector3 impactRot)
    {
        Instantiate(bulletHitPrefab, impactPos + impactRot * 0.1f, Quaternion.LookRotation(impactRot));
    }

    //Moves the shot linerenderer to updates position on clients
    [ClientRpc]
    private void RpcClientLine(Vector3 toShoot)
    {
        Debug.Log("Trying to update line");
        laser.SetPosition(0, muzzle.position);
        laser.SetPosition(1, toShoot);

        //If shot is to disappear after a while, this would enable/disable laser
        waitShot();
    }

    private IEnumerator waitShot()
    {
        laser.enabled = true;
        yield return shotDuration;
        //laser.enabled = false;
    } 

    //Updates ammo count on UI
    public void UpdateAmmoDisplay(int oldVal, int newVal)
    {
        Debug.Log("Trying to update ammo");
        uiManager.UpdateAmmo(curAmmo, maxAmmo);
    }

    //Runs reload concurrently, reloads 1 bullet every 0.3 seconds
    private IEnumerator Reload()
    {
        while(curAmmo != maxAmmo)
        {
            yield return new WaitForSeconds(0.3f);
            curAmmo++;
            uiManager.UpdateAmmo(curAmmo, maxAmmo);
        }
        isReloading = false;
    }
}
