using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//UI Manager for each client
//Attached to canvas and has public methods to update ui elements 

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Slider healthSlider;

    [SerializeField]
    private Text healthText;

    [SerializeField]
    private Text killsText;

    [SerializeField]
    private Text deathsText;

    [SerializeField]
    private Text ammoCount;

    public void UpdateHealth(int curHp, int maxHp)
    {
        //Gets update slider value
        healthSlider.value = mapHp(curHp, 0, maxHp, 0, 100);
        healthText.text = "HP: " + curHp + " / " + maxHp;
    }

    //Maps health value to 0 - 100 so slider can be updated correctly
     private static int mapHp(int value, int fromLow, int fromHigh, int toLow, int toHigh) 
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }

    public void UpdateKills(int kills)
    {
        killsText.text = "Kills: " + kills;
    }

    public void UpdateDeaths(int deaths)
    {
        deathsText.text = "Deaths: " + deaths;
    }

    public void UpdateAmmo(int ammoRemain, int ammoMax)
    {
        ammoCount.text = "Ammo - " + ammoRemain + " / " + ammoMax;
    }
}
