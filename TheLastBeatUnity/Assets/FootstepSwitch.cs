using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSwitch : MonoBehaviour
{
    [SerializeField]
    AK.Wwise.Switch footstepsEnterSwitch = null;
    [SerializeField]
    AK.Wwise.Switch footstepsExitSwitch = null;

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            footstepsEnterSwitch.SetValue(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            footstepsExitSwitch.SetValue(other.gameObject);
        }
    }
}
