using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public float SliderAmount;
    public Slider slider;

    public PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
      //  Debug.Log("Player HP = " + player.curHP);
        int mappedHP = map(player.curHP, 0, player.MaxHP, 0, 100);
        slider.value = mappedHP;
    }
    private static int map(int value, int fromLow, int fromHigh, int toLow, int toHigh) 
    {
        return (value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}
