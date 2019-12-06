using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    List<Beatable> Beats = new List<Beatable>();
    List<Beatable> Bar = new List<Beatable>();

    [SerializeField]
    float tolerance = 0;

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
    public static BeatManager Instance => GameObject.FindObjectOfType<BeatManager>();
    
    public void Add(Beatable target , TypeBeat tb)
    {
        (tb == TypeBeat.BAR ? Bar : Beats).Add(target);
    }

    public bool IsInRythm(float sampleTime , TypeBeat layer)
    {
        if (layer == TypeBeat.BAR)
        {
            if (sampleTime - LastBar.lastTimeBeat > 0 && sampleTime - LastBar.lastTimeBeat < tolerance && currentBeat > lastBeatValidated)
            {
                return true;
            }

            float nextBeat = LastBar.lastTimeBeat + LastBar.beatInterval;
            //A bit early
            if (sampleTime - nextBeat < 0 && sampleTime - nextBeat > -tolerance && currentBeat + 1 > lastBeatValidated)
            {
                return true;
            }
        }
        else
        {
            //A bit late
            if (sampleTime - LastBeat.lastTimeBeat > 0 && sampleTime - LastBeat.lastTimeBeat < tolerance && currentBeat > lastBeatValidated)
            {
                lastBeatValidated = currentBeat;
                return true;
            }

            float nextBeat = LastBeat.lastTimeBeat + LastBeat.beatInterval;
            //A bit early
            if (sampleTime - nextBeat < 0 && sampleTime - nextBeat > -tolerance && currentBeat + 1 > lastBeatValidated)
            {
                lastBeatValidated = currentBeat + 1;
                return true;
            }
        }

        return false;
    }

    public void ValidateLastBeat(TypeBeat tb)
    {
        StopAllCoroutines();
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
