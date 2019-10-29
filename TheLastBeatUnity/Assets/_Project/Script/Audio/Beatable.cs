using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Beatable : MonoBehaviour
{
    [SerializeField]
    protected float sequenceDuration;
    public float SequenceDuration => sequenceDuration;

    public abstract void Beat();
    public void BeatDelayed(float timeBetweenBeat)
    {
        StartCoroutine(CoroutineBeat(timeBetweenBeat));
    }

    IEnumerator CoroutineBeat(float delayBeat)
    {
        yield return new WaitForSeconds(delayBeat - (SequenceDuration / 2));
        Beat();
    }
}
