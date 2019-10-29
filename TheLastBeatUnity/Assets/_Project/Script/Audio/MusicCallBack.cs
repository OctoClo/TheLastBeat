using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicCallBack : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event musicEvent;
    float beatDuration;
    float tempo;
    public string myWeapon;

    [SerializeField]
    Health health;

    void Start()
    {
        musicEvent.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncAll, SyncReference, this);
    }

   void SyncReference(object in_cookie, AkCallbackType in_type, object in_info)
    {
        switch(in_type)
        {
            case AkCallbackType.AK_MusicSyncUserCue:
                AkMusicSyncCallbackInfo musicUserCue = in_info as AkMusicSyncCallbackInfo;
                if (musicUserCue.userCueName == myWeapon)
                {
                    Debug.Log(myWeapon);
                }
                break;

            case AkCallbackType.AK_MusicSyncBeat:
                AkMusicSyncCallbackInfo musicBeatDuration = in_info as AkMusicSyncCallbackInfo;
                beatDuration = musicBeatDuration.segmentInfo_fBeatDuration;
                break;

            case AkCallbackType.AK_MusicSyncGrid:
                break;

            default:
                break;
        }
    }
}