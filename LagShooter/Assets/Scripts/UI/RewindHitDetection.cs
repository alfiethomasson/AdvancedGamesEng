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
            HitTracking hitTracker = GameObject.Find("HitTracker").GetComponent<HitTracking>();
            hitTracker.RpcChangeRewindHitDetection(myToggle.isOn);
            hitTracker.UpdateUseRewindHitDetection(myToggle.isOn);
            prevCapsule.enabled = myToggle.isOn;
        }
}
