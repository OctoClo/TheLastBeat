using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicCallBack : MonoBehaviour
{
    [SerializeField]
    readonly AK.Wwise.Event musicEvent = null;

    [SerializeField]
    readonly BeatManager bm = null;

    void Start()
    {
        musicEvent.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncAll, SyncReference, this);
    }

    void SyncReference(object in_cookie, AkCallbackType in_type, object in_info)
    {
        AkMusicSyncCallbackInfo musicInfo = in_info as AkMusicSyncCallbackInfo;
        switch (in_type)
        {
            case AkCallbackType.AK_MusicSyncUserCue:
                break;

            case AkCallbackType.AK_MusicSyncBeat:
                float beatDuration = musicInfo.segmentInfo_fBeatDuration;
                bm.BeatDelayed(beatDuration, BeatManager.TypeBeat.BEAT);
                break;

            case AkCallbackType.AK_MusicSyncGrid:
                break;

            case AkCallbackType.AK_MusicSyncBar:
                float barDuration = musicInfo.segmentInfo_fBarDuration;
                bm.BeatDelayed(barDuration, BeatManager.TypeBeat.BAR);
                break;

            default:
                break;
        }
    }
}