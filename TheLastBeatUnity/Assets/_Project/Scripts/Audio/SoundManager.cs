using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    AK.Wwise.Event ambStart = null;

    [SerializeField]
    AK.Wwise.Event musStart = null;

    [SerializeField]
    AK.Wwise.State musStateStart = null;

    [SerializeField]
    AK.Wwise.Event stopEvent;

    [SerializeField]
    BeatManager bm = null;

    private int musicPosition;

    void Start()
    {
        musStateStart.SetValue();
        ambStart.Post(gameObject);
        musStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncAll, SyncReference, /*this,*/ (uint)AkCallbackType.AK_EnableGetMusicPlayPosition);
    }

    void SyncReference(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (bm == null) return;

        AkMusicSyncCallbackInfo musicInfo = in_info as AkMusicSyncCallbackInfo;
        switch (in_type)
        {
            case AkCallbackType.AK_MusicSyncUserCue:
                break;

            case AkCallbackType.AK_MusicSyncBeat:
                float beatDuration = musicInfo.segmentInfo_fBeatDuration;
                bm.BeatAll(beatDuration, BeatManager.TypeBeat.BEAT);
                break;

            case AkCallbackType.AK_MusicSyncGrid:
                musicPosition = musicInfo.segmentInfo_iCurrentPosition;
                Debug.Log("TempsEcoule: " + musicPosition/1000f);
                AkSoundEngine.SetRTPCValue("musicPosition", musicPosition / 1000f, gameObject);
                break;

            case AkCallbackType.AK_MusicSyncBar:
                float barDuration = musicInfo.segmentInfo_fBarDuration;
                bm.BeatAll(barDuration, BeatManager.TypeBeat.BAR);
                break;

            default:
                break;
        }
    }

    private void OnDestroy()
    {
        stopEvent.Post(gameObject);
    }

}
