using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        healthSlider.value = mapHp(curHp, 0, maxHp, 0, 100);
        healthText.text = "HP: " + curHp + " / " + maxHp;
    }

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
        Debug.Log("Should update ammo");
        ammoCount.text = "Ammo - " + ammoRemain + " / " + ammoMax;
    }
}
