using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        SetUpInput();
    }

    private void SetUpInput()
    {
        if(!PlayerPrefs.HasKey(PlayerPrefsNameKey)) { return;}

        string defName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        nameInputField.text = defName;
        SetPlayerName(defName);
    }

    public void SetPlayerName(string name)
    {
        //continueButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DispName = nameInputField.text;
        Debug.Log("Saved player name as: " + DispName);

        PlayerPrefs.SetString(PlayerPrefsNameKey, DispName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
