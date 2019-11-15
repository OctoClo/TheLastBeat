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

    public void BeatDelayed(float timeBetweenBeat, TypeBeat tb)
    {
        foreach (Beatable beat in (tb == TypeBeat.BAR ? Bar : Beats))
        {
            beat.BeatDelayed(timeBetweenBeat);
        }
    }
}
