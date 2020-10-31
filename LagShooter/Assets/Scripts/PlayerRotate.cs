using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    private float rotX;
    private float rotY;
    // Start is called before the first frame update
    void Start()
    {
        rotX = 0.0f;
        rotY = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
 
        rotX += -mouseY;
        rotY += mouseX;
 
        //Debug.Log("w/o " + (mouseX * Time.deltaTime) + ", w/ " + (mouseX * mouseSensitivity * Time.deltaTime) + ", mouseSensitivity: " + mouseSensitivity);
 
        rotX = Mathf.Clamp(rotX, -90.0f, 90.0f);
 
        transform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }
}
