using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

//Short main menu script to start or stop server

public class MainMenu : MonoBehaviour
{

    [SerializeField] 
    private LobbyManager networkManager = null;

    public void HostLobby()
    {
        //Start server
        networkManager.StartServer(); 
    }

    public void StopLobby()
    {
        //Stop Server
        networkManager.StopServer();
    }
}
