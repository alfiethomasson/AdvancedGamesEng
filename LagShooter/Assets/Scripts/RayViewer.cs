using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple ray viewer to see where player is aiming in debug

public class RayViewer : MonoBehaviour
{
    public float range = 50.0f;
    private Camera cam;

    void Start()
    {
        //Gets camera to draw from
        cam = GetComponentInParent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //Gets origin of ray
        Vector3 lineOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        //Draws ray
        Debug.DrawRay(lineOrigin, cam.transform.forward * range, Color.green);
    }
}
