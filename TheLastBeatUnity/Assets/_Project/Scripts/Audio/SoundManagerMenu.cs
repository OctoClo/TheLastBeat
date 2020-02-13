using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManagerMenu : MonoBehaviour
{
    public static SoundManagerMenu Instance { get; private set; }

    public BeatDetection LastBeat { get; private set; }
    public float TimePerBeat { get; private set; }
    bool destroyed = false;
    int musicPosition = 0;

    List<Beatable> Beats = new List<Beatable>();

    public struct BeatDetection
    {
        public float lastTimeBeat;
        public float beatInterval;
    }

    [SerializeField]
    AK.Wwise.Event music = null;

    [SerializeField]
    float tolerance = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        music.Post(gameObject, (uint)AkCallbackType.AK_MusicSyncAll, SyncReference, (uint)AkCallbackType.AK_EnableGetMusicPlayPosition);
    }

    public void Add(Beatable target)
    {
        Beats.Add(target);
    }

    public bool IsInRythm(float sampleTime)
    {
        return IsInTolerance(sampleTime, tolerance);
    }

    private void SyncReference(object in_cookie, AkCallbackType in_type, object in_info)
    {
        if (destroyed)
            return;

        AkMusicSyncCallbackInfo musicInfo = in_info as AkMusicSyncCallbackInfo;
        switch (in_type)
        {
            case AkCallbackType.AK_MusicSyncUserCue:
                break;

            case AkCallbackType.AK_MusicSyncBeat:
                float beatDuration = musicInfo.segmentInfo_fBeatDuration;
                TimePerBeat = beatDuration;
                BeatAll(beatDuration);
                break;

            case AkCallbackType.AK_MusicSyncGrid:
                musicPosition = musicInfo.segmentInfo_iCurrentPosition;            
                AkSoundEngine.SetRTPCValue("musicPosition", musicPosition / 1000f, gameObject);
                break;

            default:
                break;
        }
    }

    private void BeatAll(float timeBetweenBeat)
    {
        BeatDetection bd = new BeatDetection();
        bd.lastTimeBeat = TimeManager.Instance.SampleCurrentTime();
        bd.beatInterval = timeBetweenBeat;

        LastBeat = bd;

        foreach (Beatable beat in Beats)
            beat.Beat();
    }

    bool IsInTolerance(float sampleTime, float tol)
    {
        // A bit late
        if (sampleTime - LastBeat.lastTimeBeat > 0 && sampleTime - LastBeat.lastTimeBeat < tol)
            return true;

        float nextBeat = LastBeat.lastTimeBeat + LastBeat.beatInterval;

        // A bit early
        if (sampleTime - nextBeat < 0 && sampleTime - nextBeat > -tol)
            return true;

        return false;
    }

    private void OnDestroy()
    {
        destroyed = true;
    }
}
