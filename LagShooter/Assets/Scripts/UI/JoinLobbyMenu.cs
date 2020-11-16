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
    private Button joinButton = null;

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
        string ipaddress = ipInputField.text;

        networkManager.networkAddress = ipaddress;
        networkManager.OnStartClient();

        joinButton.interactable = false;
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
