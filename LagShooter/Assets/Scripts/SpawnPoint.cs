using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Spawn point script 

public class SpawnPoint : MonoBehaviour
{
    private void Awake()
    {
        //On awake, add this to spawn point list 
        PlayerSpawn.AddSpawnPoint(transform);
        Debug.Log("Adding spawn point at " + transform.position);
    }

    //On destroy, remove from list 
    private void OnDestroy()
    {
        PlayerSpawn.RemoveSpawnPoint(transform);
    }

    //Draw gizmos
    private void OnDrawGizmos()
    {
        //Draws sphere on spawn point for visibilty
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1.0f);
        //Draws line for spawn point facing
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3.0f);
    }
}
