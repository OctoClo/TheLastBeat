using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Slowable : MonoBehaviour
{
    List<Sequence> allSequences = new List<Sequence>();

    [Header("Individual timeScale")]
    float minimalTimeScale = 0.0001f;

    [SerializeField]
    float maximalTimeScale = 10;
    float personalTimeScale = 1;
    public float PersonalTimeScale
    {
        get
        {
            return personalTimeScale;
        }
        set
        {
            personalTimeScale = Mathf.Clamp(value, minimalTimeScale, maximalTimeScale);
            foreach(Sequence seq in allSequences)
            {
                seq.timeScale = personalTimeScale;
            }

            Animator anim = GetComponent<Animator>();
            if (anim)
            {
                anim.speed = personalTimeScale;
            }
        }
    }

    float endAt = 0;
    public float EndAt
    {
        get
        {
            return endAt;
        }
        set
        {
            endAt = Mathf.Max(0, value);
        }
    }

    protected virtual Sequence BuildSequence()
    {
        Sequence output = DOTween.Sequence();
        allSequences.Add(output);

        if (endAt == 0)
            output.OnStart(() => output.timeScale = personalTimeScale);
        else
            output.OnStart(() => output.timeScale = SceneHelper.Instance.ComputeTimeScale(output, endAt));

        return output;
    }

    public virtual void FinishAllSequencesAt(float finishAt, bool forceMinMax = true)
    {
        foreach (Sequence seq in allSequences)
        {
            float timeScale = SceneHelper.Instance.ComputeTimeScale(seq, finishAt);
            seq.timeScale = forceMinMax ? Mathf.Clamp(timeScale, minimalTimeScale, maximalTimeScale) : timeScale;
        }

        Animator anim = GetComponent<Animator>();
        if (anim)
        {
            float timeScale = SceneHelper.Instance.ComputeTimeScale(anim, finishAt);
            anim.speed = forceMinMax ? Mathf.Clamp(timeScale, minimalTimeScale, maximalTimeScale) : timeScale;
        }
    }

    protected virtual void DeleteSequence(Sequence seq)
    {
        if (seq != null)
        {
            seq.Kill();
        }

        allSequences.Remove(seq);
        seq = null;
    }
}
