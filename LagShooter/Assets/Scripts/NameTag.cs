using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
        NamePlate = gameObject.GetComponentInChildren<TextMesh>();
        if(isLocalPlayer)
        {
       // nameText = LobbyManager.GetPlayerName();
        Debug.Log("Player Prefs name = " + PlayerPrefs.GetString("PlayerName", "Anonymous"));
        string nameTemp = PlayerPrefs.GetString("PlayerName", "Anonymous");
        CmdUpdateName(nameTemp);

        //UpdateAllTags();
        }
       // UpdateAllTags();
    }

    [Command]
    void CmdUpdateName(string name)
    {
        nameText = name;
    }

    void HandleNameUpdated(string oldVal, string newVal)
    {
        Debug.Log("HandleNameUpdated");
        UpdateAllTags();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer)
        {
            //Debug.Log("Should look");
            nameTagGameObject.transform.rotation = Quaternion.LookRotation(transform.position - toLook.position);
        }
    }

    void SetName()
    {
        NamePlate.text = nameText;
    }

    [ClientRpc]
    public void RpcUpdateNameTags()
    {
     //   Debug.Log("Calling RpcupdateNameTags here");
        if(isLocalPlayer)
        {
          //  Debug.Log("I AM LOCAL PLAYER HELLO ALFIE");
            GameObject[] players;
            players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject p in players)
            {
                NameTag n = p.GetComponentInChildren<NameTag>();
                n.SetName();
                n.toLook = this.gameObject.transform;
            }
        }
    }

    public void UpdateNameTags()
    {
       // Debug.Log("Calling updateNameTags here");
            GameObject[] players;
            players = GameObject.FindGameObjectsWithTag("Player");
            Transform curTransform = this.gameObject.transform;
            foreach(GameObject p in players)
            {
                NameTag n = p.GetComponentInChildren<NameTag>();
                n.SetName();
                if(isLocalPlayer)
                {
                n.toLook = curTransform;
                }
            }
    }

    [Command]
    public void UpdateAllTags()
    {
        Debug.Log("Called command");
        GameObject[] players;
        players = GameObject.FindGameObjectsWithTag("Player");
         foreach(GameObject p in players)
            {
                NameTag n = p.GetComponentInChildren<NameTag>();
                n.RpcUpdateNameTags();
            }
    }

    // [Server]
    // public void CallAllTags()
    // {
    //     RpcUpdateNameTags();
    // }
}
