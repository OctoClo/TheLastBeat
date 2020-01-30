using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicStateTrigger : MonoBehaviour
{
    [SerializeField]
    AK.Wwise.State newMusicState = null;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            newMusicState.SetValue();
        }
    }
}
