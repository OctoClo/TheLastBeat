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
    List<AK.Wwise.State> allInitializeState = new List<AK.Wwise.State>();

    [SerializeField]
    BeatManager bm = null;
    public BeatManager BeatManager => bm;

    public static SoundManager Instance { get; private set; } 

    private int musicPosition;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach(AK.Wwise.State state in allInitializeState)
        {
            state.SetValue();
        }

        ambStart.Post(gameObject);
        musStart.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncAll, SyncReference, (uint)AkCallbackType.AK_EnableGetMusicPlayPosition);
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
}
