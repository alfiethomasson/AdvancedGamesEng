using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoricalLerpToggle : MonoBehaviour
{

    Toggle myToggle;

    // Start is called before the first frame update
    void Start()
    {
        myToggle = GetComponent<Toggle>();
        myToggle.onValueChanged.AddListener(delegate {ToggleValueChanged(myToggle);});
    }

    void ToggleValueChanged(Toggle change)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach(GameObject player in players)
        {
            player.GetComponent<SyncPosition>().RpcChangeHistoricalLerp(myToggle.isOn);
            player.GetComponent<SyncRotation>().RpcChangeHistoricalLerp(myToggle.isOn);
        }
    }
}
