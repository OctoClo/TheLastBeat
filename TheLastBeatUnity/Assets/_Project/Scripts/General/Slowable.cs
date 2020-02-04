using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class Slowable : MonoBehaviour
{
    List<Sequence> allSequences = new List<Sequence>();
    float timeScale = 1;

    bool overrideByTime = false;
    float timeToEnd = 0;

    public virtual float Timescale
    {
        get
        {
            return timeScale;
        }
        set
        {
            timeScale = Mathf.Max(0, value);
            if (!overrideByTime)
            {
                foreach(Sequence seq in allSequences)
                {
                    seq.timeScale = timeScale;
                }
            }
        }
    }

    public Sequence CreateSequence()
    {
        Sequence seq = DOTween.Sequence();
        allSequences.Add(seq);
        seq.timeScale = timeScale;
        if (overrideByTime)
            seq.timeScale = SceneHelper.Instance.ComputeTimeScale(seq, timeToEnd);

        seq.onKill += () => allSequences.Remove(seq);
        seq.Play();
        return seq;
    }

    public void Purge()
    {
        while (allSequences.Count > 0)
            allSequences[0].Kill();
    }

    public void ForceTimeScaleAt(float mustFinishIn)
    {
        mustFinishIn = Mathf.Max(0.001f, mustFinishIn);
        foreach(Sequence seq in allSequences)
        {
            seq.timeScale = SceneHelper.Instance.ComputeTimeScale(seq, mustFinishIn);
        }
        overrideByTime = true;
        timeToEnd = mustFinishIn;
    }

    public void StopForceTime()
    {
        overrideByTime = false;
        timeToEnd = 0;
    }
}
