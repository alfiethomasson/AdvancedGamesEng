using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//Name Tag Script
//Name Tag will look at local player so name can always be read

public class NameTag : NetworkBehaviour
{
    public TextMesh NamePlate;
    [SerializeField]
    private GameObject nameTagGameObject = null;
    public Transform toLook;

    [SyncVar (hook = nameof(HandleNameUpdated))]
    public string nameText = null;

    // Start is called before the first frame update
    void Start()
    {
        //Gets name plate text mesh
        NamePlate = gameObject.GetComponentInChildren<TextMesh>();
        //If this is called on local player
        if(isLocalPlayer)
        {
            Debug.Log("Player Prefs name = " + PlayerPrefs.GetString("PlayerName", "Anonymous"));
            //Gets name from player prefs, or Anonymous if empty
            string nameTemp = PlayerPrefs.GetString("PlayerName", "Anonymous");
            //Updates name tag on the server
            CmdUpdateName(nameTemp);
        }
    }

    //Updates player name
    [Command]
    void CmdUpdateName(string name)
    {
        //As name is a synced variable, this will update name on all clients
        nameText = name;
    }

    //Called when name is changed
    void HandleNameUpdated(string oldVal, string newVal)
    {
        UpdateAllTags();
    }

    //Update causes name tag to look at local player
    void Update()
    {
        if(!isLocalPlayer && !isServer) // Check if this is not attached to local player or not on the server
        {
            //Looks at player
            nameTagGameObject.transform.rotation = Quaternion.LookRotation(transform.position - toLook.position);
        }
    }

    //Sets name plate text to player name
    void SetName()
    {
        NamePlate.text = nameText;
    }

    //Called on clients
    //Loops through all players and updates name tags 
    [ClientRpc]
    public void RpcUpdateNameTags()
    {
        //If this is local player
        if(isLocalPlayer)
        {
            //Loops through all players in scene
            GameObject[] players;
            players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject p in players)
            {
                //For each player found, get name tag
                NameTag n = p.GetComponentInChildren<NameTag>();
                //And update name!
                n.SetName();
                //Set that tag to look at this object as it is local player
                n.toLook = this.gameObject.transform;
            }
        }
    }

    public void UpdateAllTags()
    {
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
         foreach(GameObject p in players)
            {
                NameTag n = p.GetComponentInChildren<NameTag>();
                n.RpcUpdateNameTags();
            }
    }
}
