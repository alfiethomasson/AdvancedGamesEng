using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    private void Awake()
    {
        PlayerSpawn.AddSpawnPoint(transform);
    }

    private void OnDestroy()
    {
        PlayerSpawn.RemoveSpawnPoint(transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 1.0f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3.0f);
    }
}
