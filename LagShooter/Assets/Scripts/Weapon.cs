using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage = 1;
    public float firerate = 0.5f;
    public float range = 50.0f;
    public float shotDuration = 0.1f;

    public Transform muzzle;

    private Camera fpsCam;
    private LineRenderer laser;
    private float nextShot;

    // Start is called before the first frame update
    void Start()
    {
        laser = GetComponent<LineRenderer>();
        fpsCam = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
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
        if (Input.GetButtonDown ("Fire1") && Time.time > nextShot) 
        {
            nextShot = Time.time + firerate;
            StartCoroutine(Shot());
            RaycastHit hit;
            laser.SetPosition(0, muzzle.position);
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint (new Vector3(0.5f, 0.5f, 0.0f));
            if (Physics.Raycast(rayOrigin,fpsCam.transform.forward, out hit, range))
            {
                laser.SetPosition(1, hit.point);
            }
            else
            {
                laser.SetPosition(1, fpsCam.transform.forward * range);
            }
        }
    }

    private IEnumerator Shot()
    {
        laser.enabled = true;
        yield return shotDuration;
        laser.enabled = false;
    }
}
