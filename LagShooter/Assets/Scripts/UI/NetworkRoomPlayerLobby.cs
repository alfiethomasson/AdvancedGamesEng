﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{

    [SerializeField]
    private GameObject lobbyUI = null;

    [SerializeField]
    private Text[] playerNames = new Text[4];
    [SerializeField]
    private Text[] playersReady = new Text[4];
    [SerializeField]
    private Button startButton = null;

    [SyncVar (hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar (hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startButton.gameObject.SetActive(value);
        }
    }

    private LobbyManager room;

    private LobbyManager Room
    {
        get
        {
            if(room != null) { return room; }
            return room = NetworkManager.singleton as LobbyManager;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNameInput.DispName);

        lobbyUI.SetActive(true);

    }

    public override void OnStartClient()
    {
        Room.RoomPlayers.Add(this);

        UpdateDisplay();
    }

    public override void OnNetworkDestroy()
    {
        Room.RoomPlayers.Remove(this);
    
        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldval, bool newval)
    {
        UpdateDisplay();
    }

    public void HandleDisplayNameChanged(string oldval, string newval)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if(!hasAuthority)
        {
            foreach(var player in Room.RoomPlayers)
            {
                if(player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        for(int i = 0; i < playerNames.Length; i++)
        {
            // playerNames[i].text = "Empty...";
            // playersReady[i].text = string.Empty;
        }
        Debug.Log("Trying to update names over here");

        for(int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            Debug.Log("Current roomplayers name = " + Room.RoomPlayers[i].DisplayName);
            playerNames[i].text = Room.RoomPlayers[i].DisplayName;
            playersReady[i].text = Room.RoomPlayers[i].IsReady ? 
            "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool ready)
    {
        if(!isLeader) { return; }

        startButton.interactable = ready;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayer();
    }

    [Command]
    public void CmdStartGame()
    {
        if(Room.RoomPlayers[0].connectionToClient != connectionToClient)
        {
            return;
        }

        Debug.Log("trying to start game");
        SERVERRUN();
        if(isServer)
        {
            Debug.Log("Hi I am server!");
        }
        Room.StartGame();
    }

    [Server]
    public void SERVERRUN()
    {
        Debug.Log("Server");
    }
}
