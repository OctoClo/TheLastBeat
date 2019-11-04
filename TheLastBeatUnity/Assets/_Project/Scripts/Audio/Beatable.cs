using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Beatable : MonoBehaviour
{
    [SerializeField]
    protected float sequenceDuration;
    public float SequenceDuration => sequenceDuration;

    bool pause;
    Coroutine beatCoroutine;

    public abstract void Beat();

    public void BeatDelayed(float timeBetweenBeat)
    {
        if (!pause)
            beatCoroutine = StartCoroutine(CoroutineBeat(timeBetweenBeat));
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
        pause = e.pause;

        if (pause)
            StopCoroutine(beatCoroutine);
    }

    IEnumerator CoroutineBeat(float delayBeat)
    {
        yield return new WaitForSeconds(delayBeat - (SequenceDuration / 2));
        Beat();
    }
}
