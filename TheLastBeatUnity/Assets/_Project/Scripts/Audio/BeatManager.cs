using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    [SerializeField]
    List<Beatable> Beats = new List<Beatable>();

    [SerializeField]
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
    int countBeat = 0;
    int lastBeatValidated = 0;
    bool isPausing = false;

    public struct BeatDetection
    {
        public float lastTimeBeat;
        public float beatInterval;
    }
    public static BeatManager Instance => GameObject.FindObjectOfType<BeatManager>();

    public bool IsInRythm(float sampleTime , TypeBeat layer)
    {
        if (layer == TypeBeat.BAR)
        {
            if (sampleTime - LastBar.lastTimeBeat > 0 && sampleTime - LastBar.lastTimeBeat < tolerance && countBeat > lastBeatValidated)
            {
                return true;
            }

            float nextBeat = LastBar.lastTimeBeat + LastBar.beatInterval;
            //A bit early
            if (sampleTime - nextBeat < 0 && sampleTime - nextBeat > -tolerance && countBeat + 1 > lastBeatValidated)
            {
                return true;
            }
        }
        else
        {
            //A bit late
            if (sampleTime - LastBeat.lastTimeBeat > 0 && sampleTime - LastBeat.lastTimeBeat < tolerance && countBeat > lastBeatValidated)
            {
                lastBeatValidated = countBeat;
                return true;
            }

            float nextBeat = LastBeat.lastTimeBeat + LastBeat.beatInterval;
            //A bit early
            if (sampleTime - nextBeat < 0 && sampleTime - nextBeat > -tolerance && countBeat + 1 > lastBeatValidated)
            {
                lastBeatValidated = countBeat + 1;
                return true;
            }
        }

        return false;
    }

    //One made the action in the right time , flag it as validated
    public void ValidateLastBeat(TypeBeat tb)
    {
        StopAllCoroutines();
        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.ValidateBeat();
        }
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
            countBeat++;
            LastBeat = bd;
        }

        StartCoroutine(BeatCoundown(tb));
        StartCoroutine(DelayedBeat(tb));      
    }

    IEnumerator DelayedBeat(TypeBeat tb)
    {
        yield return new WaitForSeconds(visualDelay);
        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.Beat();
        }

        if (OnBeatTriggered != null)
        {
            OnBeatTriggered(tb);
        }
    }

    IEnumerator BeatCoundown(TypeBeat tb)
    {
        yield return new WaitForSeconds(tolerance);
        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.MissedBeat();
        }        
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
}
