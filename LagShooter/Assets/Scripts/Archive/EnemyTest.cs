using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTest : MonoBehaviour
{

    public int maxHP = 1;
    private int curHP;
    // Start is called before the first frame update
    void Start()
    {
        curHP = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        if(curHP == 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void TakeDamage(int dmg)
    {
        Debug.Log("I've been hit!");
        curHP -= dmg;
        if(curHP < 0)
        {
            curHP = 0;
        }
    }
}
