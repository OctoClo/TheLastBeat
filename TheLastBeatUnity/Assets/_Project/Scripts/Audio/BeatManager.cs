using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    [SerializeField]
    List<Beatable> Beats;

    [SerializeField]
    List<Beatable> Bar;

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
