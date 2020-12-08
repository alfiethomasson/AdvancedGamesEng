using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

//Intermediatery game object

public class NetworkGamePlayerLobby : NetworkBehaviour
{
    [SyncVar]
    public string DisplayName = "Loading...";
    private LobbyManager room;

    private LobbyManager Room
    {
        get
        {
            if(room != null) { return room; } // Return network manager
            return room = NetworkManager.singleton as LobbyManager; //if Room unassigned, get network manager as singleton for consistency
        }
    }

    //When this is started on client
    public override void OnStartClient()
    {
        //Sets this to not be destroyed on scene transition
        DontDestroyOnLoad(gameObject);

        //Adds this to the players on network manager
        Room.GamePlayers.Add(this);
    }

    //When this is destroyed
    public override void OnNetworkDestroy()
    {
        //Remove from list on network manager
        Room.GamePlayers.Remove(this);
    }

   //Sets display name 
   public void SetDisplayName(string displayName)
   {
       this.DisplayName = displayName;
   }
}
