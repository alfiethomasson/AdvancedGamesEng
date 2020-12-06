using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField]
    private LobbyManager networkManager = null;

    [SerializeField]
    private GameObject landingPagePanel = null;
    
    [SerializeField]
    private InputField ipInputField = null;

    [SerializeField]
    private Text joinText = null;

    [SerializeField]
    private Button joinButton = null;

    private bool connecting = false;

    private void OnEnable()
    {
        LobbyManager.OnClientConnected += HandleClientConnected;
        LobbyManager.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        LobbyManager.OnClientConnected -= HandleClientConnected;
        LobbyManager.OnClientDisconnected -= HandleClientDisconnected;
    }

    public void JoinLobby()
    {
        if(!connecting)
        {
        string ipaddress = ipInputField.text;

        networkManager.networkAddress = ipaddress;
        Debug.Log("Read IP address as: " + ipaddress);
        networkManager.StartClient();
        joinText.text = "Cancel Join";

        connecting = true;
        }
        else
        {
            networkManager.StopClient();
            joinText.text = "Join Game";
            connecting = false;
        }
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
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
