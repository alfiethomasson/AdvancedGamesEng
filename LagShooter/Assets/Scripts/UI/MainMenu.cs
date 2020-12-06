using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField] 
    private LobbyManager networkManager = null;

    [SerializeField]
    private GameObject landingPagePanel = null;

    public void HostLobby()
    {
        networkManager.StartServer();
       // landingPagePanel.SetActive(false);
    }

    public void StopLobby()
    {
        networkManager.StopServer();
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
