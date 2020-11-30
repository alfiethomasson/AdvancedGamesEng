using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NameTag : NetworkBehaviour
{
    public TextMesh NamePlate;
    public GameObject Object;
    public Transform toLook;

    [SyncVar]
    public string nameText;

    // Start is called before the first frame update
    void Start()
    {
        NamePlate = gameObject.GetComponentInChildren<TextMesh>();
        if(isLocalPlayer)
        {
        nameText = PlayerPrefs.GetString("PlayerName", "Anonymous");
        Debug.Log("Player Prefs name = " + PlayerPrefs.GetString("PlayerName", "Anonymous"));
        //UpdateAllTags();
        }
        Object = this.gameObject;
        UpdateNameTags();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer)
        this.gameObject.transform.LookAt(toLook);
    }

    void SetName()
    {
        NamePlate.text = nameText;
    }

    [ClientRpc]
    public void RpcUpdateNameTags()
    {
        Debug.Log("Calling RpcupdateNameTags here");
        if(isLocalPlayer)
        {
            GameObject[] players;
            players = GameObject.FindGameObjectsWithTag("Player");
            Transform curTransform = this.gameObject.transform.parent.transform;
            foreach(GameObject p in players)
            {
                NameTag n = p.GetComponentInChildren<NameTag>();
                n.SetName();
                n.toLook = curTransform;
            }
        }
    }

    public void UpdateNameTags()
    {
        Debug.Log("Calling updateNameTags here");
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
