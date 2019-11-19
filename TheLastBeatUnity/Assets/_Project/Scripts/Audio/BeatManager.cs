using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    [SerializeField]
    List<Beatable> Beats = new List<Beatable>();

    [SerializeField]
    List<Beatable> Bar = new List<Beatable>();

    public enum TypeBeat
    {
        BEAT,
        BAR
    }

    public BeatDetection LastBar { get; private set; }
    public BeatDetection LastBeat { get; private set; }

    public struct BeatDetection
    {
        public float lastTimeBeat;
        public float beatInterval;
    }
    public static BeatManager Instance => GameObject.FindObjectOfType<BeatManager>();

    public bool IsInRythm(float toleranceSec , float sampleTime , TypeBeat layer)
    {
        float rest;
        if (layer == TypeBeat.BAR)
        {
            rest = (LastBar.lastTimeBeat - sampleTime) % LastBar.beatInterval;
        }
        else
        {
            rest = (LastBeat.lastTimeBeat - sampleTime) % LastBeat.beatInterval;
        }

        if (Mathf.Abs(rest) < toleranceSec)
            return true;

        return false;
    }

    public void BeatDelayed(float timeBetweenBeat, TypeBeat tb)
    {
        BeatDetection bd = new BeatDetection();
        bd.lastTimeBeat = Time.realtimeSinceStartup;
        bd.beatInterval = timeBetweenBeat;

        if (tb == TypeBeat.BAR)
            LastBar = bd;
        else
            LastBeat = bd;

        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.BeatDelayed(timeBetweenBeat);
        }
    }
}
