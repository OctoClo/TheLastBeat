using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicStateTrigger : MonoBehaviour
{
    [SerializeField]
    List<AK.Wwise.State> newMusicStateList = new List<AK.Wwise.State>();

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (AK.Wwise.State state in newMusicStateList)
            {
                state.SetValue();
            }
            //newMusicState.SetValue();
        }
    }
}
