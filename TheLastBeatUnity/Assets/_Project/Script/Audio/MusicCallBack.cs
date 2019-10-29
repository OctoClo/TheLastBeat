using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicCallBack : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event musicEvent;
    float beatDuration;
    float tempo;
    public string myWeapon;

    void Start()
    {
        musicEvent.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncAll, SyncReference, this);
    }

   void SyncReference(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (in_type == AkCallbackType.AK_MusicSyncUserCue)
        {
            //do synced stuff here 
            AkMusicSyncCallbackInfo musicUserCue = in_info as AkMusicSyncCallbackInfo;
            if (musicUserCue.userCueName == myWeapon)
            {              
                Debug.Log(myWeapon);
            }
        }
        else if (in_type == AkCallbackType.AK_MusicSyncBar)
        {
            //do synced stuff here
            Debug.Log("Bar");
            
        }
        else if (in_type == AkCallbackType.AK_MusicSyncBeat)
        {
            //do synced stuff here
            AkMusicSyncCallbackInfo musicBeatDuration = in_info as AkMusicSyncCallbackInfo;
            beatDuration = musicBeatDuration.segmentInfo_fBeatDuration;
            Debug.Log("Beat = " + beatDuration);
        }
        else if (in_type == AkCallbackType.AK_MusicSyncGrid)
        {
            //do synced stuff here 
            Debug.Log("Grid");
        }
    }
}