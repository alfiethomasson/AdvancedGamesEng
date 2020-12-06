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

    [SyncVar]
    Vector3 rayPoint;
    [SyncVar]
    bool rayActive;

    [SyncVar (hook = nameof(UpdateAmmoDisplay))]
    public int curAmmo;

    public int maxAmmo;

    private Camera fpsCam;
    private LineRenderer laser;
    private float nextShot;
    private float killcounter = 0;
    private Text killText;

    private bool isReloading = false;

    // Start is called before the first frame update
    void Start()
    {
        laser = GetComponent<LineRenderer>();
        fpsCam = GetComponentInChildren<Camera>();
        //Debug.Log("Should find canvas");
        hitTrackerGameObject = GameObject.Find("HitTracker");
        hitTracker = hitTrackerGameObject.GetComponent<HitTracking>();
        // Debug.Log("Should find canvas");
        if(isLocalPlayer)
        {
            uiManager = GameObject.Find("CanvasMain").GetComponent<UIManager>();
        }

        curAmmo = maxAmmo;
        uiManager.UpdateAmmo(curAmmo, maxAmmo);

        if(isServer)
        {
            Debug.Log("is server herro");
            //hitTracker = GameObject.Find("HitTracker").GetComponent<HitTracking>();
        }
        // uiManager.UpdateAmmo(curAmmo, maxAmmo);
    }

    // Update is called once per frame
    void Update()
    {
       if(!isLocalPlayer) { return;}
       /*  RaycastHit hit;
            laser.SetPosition(0, muzzle.position);
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(rayOrigin,fpsCam.transform.forward, out hit, range))
            {
                laser.SetPosition(1, hit.point);
            }
            else
            {
                laser.SetPosition(1, fpsCam.transform.forward * range);
            }*/
        if(Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            isReloading = true;
            StartCoroutine(Reload());
        }


        //Fire weapon!
        if (Input.GetButtonDown ("Fire1") && ReadyToFire()) 
        {
          //  Debug.Log("FIRING MY LAZOR!");
            nextShot = Time.time + firerate;
            //StartCoroutine(Shot());
          //  laser.SetPosition(0, muzzle.position);
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.5f));
            double latencyTime = NetworkTime.rtt;
           // laser.SetPosition(0, muzzle.position);
           curAmmo--;
           uiManager.UpdateAmmo(curAmmo, maxAmmo);
            CmdCheckShot(latencyTime, rayOrigin, fpsCam.transform.forward);
            // if (Physics.Raycast(rayOrigin,fpsCam.transform.forward, out hit, range))
            // {
            //     laser.SetPosition(1, hit.point);
            //     if(hit.collider.tag == "PlayerBody")
            //     {
            //         Debug.Log("Hit Player");
            //         dealDamage(hit);
            //     }
            //     else
            //     {
            //         Debug.Log("I hit: " + hit.collider.tag);
            //     }
            // }
            // else
            // {
            //     laser.SetPosition(1, fpsCam.transform.forward * range);
            // }
        }
    }

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

    [Command]
    public void CmdCheckShot(double latency, Vector3 rayOrigin, Vector3 rayPoint)
    {
        RaycastHit hit;
        // laser.SetPosition(0, muzzle.position);
        //Debug.Log("heyo");
        hit = hitTracker.BeginComputeHit(latency, rayOrigin, rayPoint);

        laser.SetPosition(0, muzzle.position);
        laser.SetPosition(1, hit.point);
        //RpcShowLine(hit.point);
       // Debug.DrawRay(rayOrigin, fpsCam.transform.forward * 500, Color.red, 5.0f);
       // Vector3 rayLine = muzzle.position + (fpsCam.transform.forward * 25);
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(muzzle.position, rayLine);
        Debug.Log("heyo2");
        Debug.Log("I hit: " + hit.collider.tag);
        RpcPlayerShot(GetComponent<NetworkIdentity>().netId, hit.point, hit.normal);
         if (hit.collider.tag == "PlayerBody")
            {
               // Debug.DrawRay(rayOrigin, fpsCam.transform.forward, Color.green, 1);
                //laser.SetPosition(1, hit.point);
              // RpcShowLine(hit.transform.position);
                   // dealDamage(hit);
                   GameObject enemy = hit.collider.gameObject;
                   PlayerController enemyController = enemy.GetComponentInParent<PlayerController>();
                   enemyController.TakeDamage(40, GetComponentInParent<NetworkIdentity>().netId);
                    Debug.Log("I hit: " + hit.collider.tag);
            }
            else
            {
              //  RpcShowLine(hit.transform.position);
                //laser.SetPosition(1, fpsCam.transform.forward * range);
                //RpcShowLine(fpsCam.transform.forward * range);
              //  RpcShowLine(fpsCam.transform.forward * range);
            }

         RpcClientLine(hit.point);   
    }

    [ClientRpc]
    void RpcPlayerShot(uint shooter, Vector3 impactPos, Vector3 impactRot)
    {
        Instantiate(bulletHitPrefab, impactPos + impactRot * 0.1f, Quaternion.LookRotation(impactRot));
    }

    [ClientRpc]
    private void RpcClientLine(Vector3 toShoot)
    {
        Debug.Log("Trying to show line");
        laser.SetPosition(0, muzzle.position);
        laser.SetPosition(1, toShoot);
      // laser.SetPosition(1, new Vector3(0.0f, 0.0f, 0.0f));
        waitShot();
    }

    [Server]
    public void RpcShowLine(Vector3 hitPos)
    {
       // rayPoint = hitPos;
         rayPoint = hitPos;
        laser.SetPosition(1, hitPos);
       // RpcClientLine();
    }

    private IEnumerator waitShot()
    {
        laser.enabled = true;
        yield return shotDuration;
       // laser.enabled = false;
    } 

    public void UpdateAmmoDisplay(int oldVal, int newVal)
    {
        Debug.Log("Trying to update ammo");
        uiManager.UpdateAmmo(curAmmo, maxAmmo);
    }

    // private IEnumerator Shot()
    // {

    //     //laser.enabled = true;
    //     yield return shotDuration;
    //     //laser.enabled = false;
    // }

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

    void dealDamage(RaycastHit hit)
    {
       // PlayerController enemy = hit.collider.GetComponentInParent<PlayerController>();
       GameObject enemy = hit.collider.gameObject;
       GameObject enemyparent = enemy.transform.parent.gameObject;
       killcounter++;
       GameObject.Find("HitCounter").GetComponent<Text>().text = killcounter.ToString();
       SendDamage(enemyparent);
        //enemy.curHP -= 1;
    }

    //Sends command to all instances of the player
    [Command]  
    void SendDamage(GameObject enemy)
    {
        // bool isKill = enemy.GetComponent<PlayerController>().TakeDamage(1);
        // if(isKill)
        // {
        //     killcounter++;
        //     killText = GameObject.Find("KillCounter").GetComponent<Text>();
        //     killText.text = killcounter.ToString();
        // }
    }
}
