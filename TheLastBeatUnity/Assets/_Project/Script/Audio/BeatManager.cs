using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    [SerializeField]
    List<Beatable> allBeatables;

    public void BeatDelayed(float timeBetweenBeat)
    {
        foreach (Beatable beat in allBeatables)
        {
            beat.BeatDelayed(timeBetweenBeat);
        }
    }
}
