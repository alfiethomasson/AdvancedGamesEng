using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerSpawn : NetworkBehaviour
{
    [SerializeField]
    private GameObject player = null;

    private static List<Transform> spawnPoints = new List<Transform>();

    private int index = 0;

    public HitTracking hitTracker;

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }

    public static void RemoveSpawnPoint(Transform transform)
    {
        spawnPoints.Remove(transform);
    }

    public override void OnStartServer()
    {
        LobbyManager.OnServerReadied += SpawnPlayer;
        hitTracker = GameObject.Find("HitTracker").GetComponent<HitTracking>();
    }

    [ServerCallback]
    private void OnDestroy()
    {
         LobbyManager.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    public void SpawnPlayer(NetworkConnection connection)
    {
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(index);

        if(spawnPoint == null)
        {
            Debug.Log("Missing spawn point at index " + index);
            return;
        }

        GameObject playerInstantiated = Instantiate(player, spawnPoints[index].position, spawnPoints[index].rotation);
        TextMesh onPlayerNameTag = playerInstantiated.GetComponentInChildren<TextMesh>();
       // onPlayerNameTag.text = Disp
        NetworkServer.Spawn(playerInstantiated, connection);
        NetworkServer.ReplacePlayerForConnection(connection, playerInstantiated, true);
        Debug.Log("Player spawned at " + spawnPoints[index].position);

        hitTracker.AddPlayerToList(playerInstantiated);

        index += 1;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
