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

    public Transform muzzle;

    [SyncVar]
    Vector3 rayPoint;
    [SyncVar]
    bool rayActive;

    private Camera fpsCam;
    private LineRenderer laser;
    private float nextShot;
    private float killcounter = 0;
    private Text killText;

    // Start is called before the first frame update
    void Start()
    {
        laser = GetComponent<LineRenderer>();
        fpsCam = GetComponentInChildren<Camera>();
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

        //Fire weapon!
        if (Input.GetButtonDown ("Fire1") )//&& Time.time > nextShot) 
        {
          //  Debug.Log("FIRING MY LAZOR!");
            nextShot = Time.time + firerate;
            StartCoroutine(Shot());
            RaycastHit hit;
          //  laser.SetPosition(0, muzzle.position);
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));
            CmdCheckShot();
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

    [Command]
    public void CmdCheckShot()
    {
        RaycastHit hit;
        // laser.SetPosition(0, muzzle.position);
        Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));


         if (Physics.Raycast(rayOrigin,fpsCam.transform.forward, out hit, range))
            {
               // Debug.DrawRay(rayOrigin, fpsCam.transform.forward, Color.green, 1);
                //laser.SetPosition(1, hit.point);
              // RpcShowLine(hit.transform.position);
              laser.SetPosition(0, muzzle.position);
              RpcShowLine(hit.point);
                if(hit.collider.tag == "PlayerBody")
                {
                    Debug.Log("Hit Player");
                   // dealDamage(hit);
                   GameObject enemy = hit.collider.gameObject;
                   PlayerController enemyController = enemy.GetComponentInParent<PlayerController>();
                   enemyController.TakeDamage(1);
                }
                else
                {
                    Debug.Log("I hit: " + hit.collider.tag);
                }
            }
            else
            {
              //  RpcShowLine(hit.transform.position);
                //laser.SetPosition(1, fpsCam.transform.forward * range);
                RpcShowLine(fpsCam.transform.forward * range);
              //  RpcShowLine(fpsCam.transform.forward * range);
            }
    }

    [ClientRpc]
    private void RpcClientLine()
    {
        laser.SetPosition(0, muzzle.position);
        laser.SetPosition(1, rayPoint);
        waitShot();
    }

    [Server]
    public void RpcShowLine(Vector3 hitPos)
    {
       // rayPoint = hitPos;
         rayPoint = hitPos;
        laser.SetPosition(1, hitPos);
        RpcClientLine();
    }

    private IEnumerator waitShot()
    {
        laser.enabled = true;
        yield return shotDuration;
        laser.enabled = false;
    } 

    public void Fire()
    {
            // nextShot = Time.time + firerate;
            // StartCoroutine(Shot());
            // RaycastHit hit;
            // laser.SetPosition(0, muzzle.position);
            // Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));
            // if (Physics.Raycast(rayOrigin,fpsCam.transform.forward, out hit, range))
            // {
            //     laser.SetPosition(1, hit.point);
            //    // Debug.Log("Hit something");
            //     if(hit.collider.tag == "Player")
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

    private IEnumerator Shot()
    {

        //laser.enabled = true;
        yield return shotDuration;
        laser.enabled = false;
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
