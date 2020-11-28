using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NameTag : NetworkBehaviour
{
    public TextMesh NamePlate;
    public GameObject Object;
    public Transform toLook;

    // Start is called before the first frame update
    void Start()
    {
        NamePlate = gameObject.GetComponent<TextMesh>();
        Object = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isLocalPlayer)
        this.gameObject.transform.LookAt(toLook);
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
                n.toLook = curTransform;
            }
        }
    }

    [Server]
    public void CallAllTags()
    {
        RpcUpdateNameTags();
    }
}
