using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CustomNetworkManager : NetworkManager
{
  public override void OnServerAddPlayer(NetworkConnection conn)
  {
      base.OnServerAddPlayer(conn);
      UpdateTags();
      Debug.Log("Succesfully registered new player joining");
  }

  private void UpdateTags()
  {
      GameObject[] players;
      players = GameObject.FindGameObjectsWithTag("Player");
      foreach(GameObject p in players)
      {
          Debug.Log("Trying to call rpcupdate ");
        p.GetComponentInChildren<NameTag>().CallAllTags();

      }
  }
}
