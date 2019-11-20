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
    float tolerance;

    public enum TypeBeat
    {
        BEAT,
        BAR
    }

    public BeatDetection LastBar { get; private set; }
    public BeatDetection LastBeat { get; private set; }
    int validationToken = 0;

    //Used to identify
    int countBeat = 0;
    int lastBeatValidated = 0;

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

    //On made the action in the right time , flag it as validated
    public void ValidateLastBeat(TypeBeat tb)
    {
        StopAllCoroutines();
        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.ValidateBeat();
        }
    }

    public void BeatDelayed(float timeBetweenBeat, TypeBeat tb)
    {
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
        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.BeatDelayed(timeBetweenBeat);
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
}
