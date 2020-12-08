using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Player name input script

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField]
    private InputField  nameInputField = null;

    [SerializeField]
    private Button  continueButton = null;

    public static string DispName {get; private set;}

    private const string PlayerPrefsNameKey = "PlayerName";
    // Start is called before the first frame update

    void Start()
    { 
        //When started, call set up input to read from player prefs (remembers name from last time)
        SetUpInput();
    }

    //Sets up the input field 
    private void SetUpInput()
    {
        if(!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return;} // If player prefs doesnt have a name entry, return 

        //Get player prefs name
        string defName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        //Set input field to player prefs name
        nameInputField.text = defName;
    }

    //Saves player name from input field 
    public void SavePlayerName()
    {
        //Sets name to text in input field 
        DispName = nameInputField.text;
        Debug.Log("Saved player name as: " + DispName);

        //Updates player prefs
        PlayerPrefs.SetString(PlayerPrefsNameKey, DispName);
    }
}
