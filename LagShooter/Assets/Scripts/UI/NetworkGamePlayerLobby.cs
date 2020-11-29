using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkGamePlayerLobby : NetworkBehaviour
{
    [SyncVar]
    public string DisplayName = "Loading...";
    private LobbyManager room;

    private LobbyManager Room
    {
        get
        {
            if(room != null) { return room; }
            return room = NetworkManager.singleton as LobbyManager;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);
    }

    public override void OnNetworkDestroy()
    {
        Room.GamePlayers.Remove(this);
    }

   // [Server]
   public void SetDisplayName(string displayName)
   {
       this.DisplayName = displayName;
   }
}
