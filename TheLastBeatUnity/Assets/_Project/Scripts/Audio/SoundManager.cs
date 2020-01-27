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

    public float TimePerBeat { get; private set; }
    public float TimePerBar { get; private set; }

    public static SoundManager Instance { get; private set; } 
    private int musicPosition;

    //=============
    List<Beatable> Beats = new List<Beatable>();
    List<Beatable> Bar = new List<Beatable>();

    [SerializeField]
    float tolerance = 0;
    public float Tolerance => tolerance;

    [SerializeField]
    float perfectTolerance = 0;

    [SerializeField]
    float visualDelay = 0;

    public enum TypeBeat
    {
        BEAT,
        BAR
    }

    public BeatDetection LastBar { get; private set; }
    public BeatDetection LastBeat { get; private set; }

    public delegate void beatParams(TypeBeat tb);
    public event beatParams OnBeatTriggered;

    //Used to identify
    int currentBeat = 0;
    int lastBeatValidated = 0;
    bool isPausing = false;

    public struct BeatDetection
    {
        public float lastTimeBeat;
        public float beatInterval;
    }

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
        AkMusicSyncCallbackInfo musicInfo = in_info as AkMusicSyncCallbackInfo;
        switch (in_type)
        {
            case AkCallbackType.AK_MusicSyncUserCue:
                break;

            case AkCallbackType.AK_MusicSyncBeat:
                float beatDuration = musicInfo.segmentInfo_fBeatDuration;
                TimePerBeat = beatDuration;
                BeatAll(beatDuration, TypeBeat.BEAT);
                break;

            case AkCallbackType.AK_MusicSyncGrid:
                musicPosition = musicInfo.segmentInfo_iCurrentPosition;            
                AkSoundEngine.SetRTPCValue("musicPosition", musicPosition / 1000f, gameObject);
                break;

            case AkCallbackType.AK_MusicSyncBar:
                float barDuration = musicInfo.segmentInfo_fBarDuration;
                TimePerBar = barDuration;
                BeatAll(barDuration, TypeBeat.BAR);
                break;

            default:
                break;
        }
    }

    public void Add(Beatable target, TypeBeat tb)
    {
        (tb == TypeBeat.BAR ? Bar : Beats).Add(target);
    }

    bool IsInTolerance(float sampleTime , TypeBeat layer, float tol)
    {
        if (layer == TypeBeat.BAR)
        {
            if (sampleTime - LastBar.lastTimeBeat > 0 && sampleTime - LastBar.lastTimeBeat < tol && currentBeat > lastBeatValidated)
            {
                return true;
            }

            float nextBeat = LastBar.lastTimeBeat + LastBar.beatInterval;
            //A bit early
            if (sampleTime - nextBeat < 0 && sampleTime - nextBeat > -tol && currentBeat + 1 > lastBeatValidated)
            {
                return true;
            }
        }
        else
        {
            //A bit late
            if (sampleTime - LastBeat.lastTimeBeat > 0 && sampleTime - LastBeat.lastTimeBeat < tol)
            {
                return true;
            }

            float nextBeat = LastBeat.lastTimeBeat + LastBeat.beatInterval;
            //A bit early
            if (sampleTime - nextBeat < 0 && sampleTime - nextBeat > -tol)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsInRythm(float sampleTime, TypeBeat layer)
    {
        return IsInTolerance(sampleTime, layer, tolerance);
    }

    public bool IsPerfect(float sampleTime , TypeBeat layer)
    {
        return IsInTolerance(sampleTime, layer, perfectTolerance);
    }

    public void BeatAll(float timeBetweenBeat, TypeBeat tb)
    {
        if (isPausing) return;

        BeatDetection bd = new BeatDetection();
        bd.lastTimeBeat = TimeManager.Instance.SampleCurrentTime();
        bd.beatInterval = timeBetweenBeat;

        if (tb == TypeBeat.BAR)
            LastBar = bd;
        else
        {
            currentBeat++;
            LastBeat = bd;
        }

        StartCoroutine(DelayedBeat(tb));
    }

    public float GetTimeLeftNextBeat()
    {
        return (LastBeat.lastTimeBeat + LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();
    }

    IEnumerator DelayedBeat(TypeBeat tb)
    {
        yield return new WaitForSeconds(visualDelay);
        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.Beat();
        }

        OnBeatTriggered?.Invoke(tb);
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<PauseEvent>(OnPauseEvent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<PauseEvent>(OnPauseEvent);
    }

    private void OnPauseEvent(PauseEvent e)
    {
        isPausing = e.pause;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        OnBeatTriggered = null;
    }
}
