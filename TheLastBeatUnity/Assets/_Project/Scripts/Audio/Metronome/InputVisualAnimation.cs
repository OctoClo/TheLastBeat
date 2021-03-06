﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

struct SequenceAndTarget
{
    public GameObject target;
    public Sequence sequence;
}

public class InputVisualAnimation : Beatable
{
    [SerializeField]
    GameObject prefabAnimation = null;

    [SerializeField]
    GameObject prefabAnimationCorrect = null;

    [SerializeField]
    GameObject prefabAnimationPerfect = null;

    [SerializeField]
    RectTransform rootPerfectGood = null;

    [SerializeField]
    Color wrong = Color.white;

    [SerializeField]
    Color goodColor = Color.white;

    [SerializeField]
    Color perfectColor = Color.white;

    Queue<SequenceAndTarget> allInstances = new Queue<SequenceAndTarget>();

    public override void Beat()
    {
        GameObject instantiatedPrefab = Instantiate(prefabAnimation,transform);
        RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();

        float timeLeft = SoundManager.Instance.LastBeat.beatInterval;

        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => 0, x => rect.localScale = new Vector3(x, x, 1), 1.0f, timeLeft).SetEase(Ease.Linear))
            .Append(instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, SoundManager.Instance.Tolerance).SetEase(Ease.Linear))
            .AppendCallback(() =>
            {
                if (allInstances.Count > 0)
                {
                    SequenceAndTarget seqAndTar = allInstances.Dequeue();
                    seqAndTar.sequence.Kill();
                    Destroy(seqAndTar.target);
                }
            })
            .Insert(0, instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.white, timeLeft).SetEase(Ease.Linear))
            .Insert(timeLeft, DOTween.To(() => 1, x => rect.localScale = new Vector3(x, x, 1), 1, SoundManager.Instance.Tolerance).SetEase(Ease.Linear))
            .SetUpdate(true);

        SequenceAndTarget seqTar = new SequenceAndTarget();
        seqTar.sequence = seq;
        seqTar.target = instantiatedPrefab;

        allInstances.Enqueue(seqTar);
    }
    public void CorrectBeat()
    {
        if (allInstances.Count == 0)
            return;

        SequenceAndTarget seqTar = allInstances.Dequeue();
        GameObject gob = seqTar.target;
        Vector3 scale = gob.GetComponent<RectTransform>().localScale;
        Destroy(gob);
        seqTar.sequence.Kill();

        GameObject instanceAnim = Instantiate(prefabAnimationCorrect, rootPerfectGood);
        instanceAnim.GetComponent<Image>().color = goodColor;
        instanceAnim.GetComponent<RectTransform>().localScale = scale * 1.2f;
        Destroy(instanceAnim, 1);
    }
    public void WrongBeat()
    {
        if (allInstances.Count == 0)
            return;

        SequenceAndTarget seqTar = allInstances.Dequeue();
        GameObject gob = seqTar.target;
        seqTar.sequence.Kill();

        gob.GetComponent<UnityEngine.UI.Image>().color = wrong;
        gob.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, 0.3f);
        Destroy(gob, 1);
    }

    public void PerfectBeat()
    {
        if (allInstances.Count == 0)
            return;

        SequenceAndTarget seqTar = allInstances.Dequeue();
        GameObject gob = seqTar.target;
        Vector3 scale = gob.GetComponent<RectTransform>().localScale;
        Destroy(gob);
        seqTar.sequence.Kill();

        GameObject instanceAnim = Instantiate(prefabAnimationPerfect, rootPerfectGood);
        instanceAnim.GetComponent<Image>().color = perfectColor;
        instanceAnim.GetComponent<RectTransform>().localScale = scale * 1.2f;
        Destroy(instanceAnim, 1);
    }
}
