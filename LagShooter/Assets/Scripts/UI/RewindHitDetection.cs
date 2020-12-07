using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewindHitDetection : MonoBehaviour
{

    Toggle myToggle;
    [SerializeField]
    private CapsuleCollider prevCapsule;

    // Start is called before the first frame update
    void Start()
    {
        myToggle = GetComponent<Toggle>();
        myToggle.onValueChanged.AddListener(delegate {ToggleValueChanged(myToggle);});
    }

    void ToggleValueChanged(Toggle change)
    {
        // GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // foreach(GameObject player in players)
        // {
        //     player.GetComponent<SyncPosition>().RpcChangeLatencyRewind(myToggle.isOn);
            HitTracking hitTracker = GameObject.Find("HitTracker").GetComponent<HitTracking>();
            hitTracker.RpcChangeRewindHitDetection(myToggle.isOn);
            //hitTracker.CmdChangeRewindHitDetection(myToggle.isOn);
            hitTracker.UpdateUseRewindHitDetection(myToggle.isOn);
            prevCapsule.enabled = myToggle.isOn;
            

        //     hitTracker.RpcChangeLatencyRewind(myToggle.isOn);
        //    // player.GetComponent<SyncRotation>().RpcChangeInterpolation();
        //     hitTracker.UpdateUseLatencyRewind(myToggle.isOn);
        }
}
