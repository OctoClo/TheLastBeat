﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BeatAtFeet : Beatable
{
    [SerializeField]
    Transform rootParent;

    [SerializeField]
    GameObject prefab;

    [SerializeField]
    Vector3 finalSize;

    [SerializeField]
    AnimationCurve curve;

    [SerializeField]
    Color goodInput;

    [SerializeField]
    Color wrongInput;

    Queue<SequenceAndTarget> allInstances = new Queue<SequenceAndTarget>();

    public override void Beat()
    {
        SoundManager sm = SoundManager.Instance;
        float timeLeft = (sm.LastBeat.lastTimeBeat + sm.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();
        timeLeft += sm.LastBeat.beatInterval;
        GameObject instantiated = Instantiate(prefab, rootParent);
        instantiated.transform.localPosition = Vector3.zero;
        instantiated.transform.localScale = Vector3.one * 0.1f;
        Sequence seq = DOTween.Sequence()
            .Append(instantiated.transform.DOScale(finalSize, timeLeft).SetEase(curve))
            .Insert(0, DOTween.To(() => instantiated.GetComponent<MeshRenderer>().material.color, x => instantiated.GetComponent<MeshRenderer>().material.color = x, Color.white, timeLeft))
            .Append(DOTween.To(() => instantiated.GetComponent<MeshRenderer>().material.color, x => instantiated.GetComponent<MeshRenderer>().material.color = x, Color.white, 0.2f))
            .AppendCallback(() => Destroy(instantiated))
            .AppendCallback(() => allInstances.Dequeue());

        SequenceAndTarget seqAndTar = new SequenceAndTarget();
        seqAndTar.target = instantiated;
        seqAndTar.sequence = seq;
        allInstances.Enqueue(seqAndTar);
    }

    public void CorrectInput()
    {
        SequenceAndTarget seqTar = allInstances.Dequeue();
        seqTar.sequence.Kill();
        seqTar.target.GetComponent<MeshRenderer>().material.color = goodInput;
        Color tempColor = new Color(goodInput.r, goodInput.g, goodInput.b, 0);
        Sequence seq = DOTween.Sequence()
            .Append(DOTween.To(() => seqTar.target.GetComponent<MeshRenderer>().material.color, x => seqTar.target.GetComponent<MeshRenderer>().material.color = x, Color.white, 0.2f))
            .AppendCallback(() => Destroy(seqTar.target));
    }

    public void PerfectInput()
    {
        CorrectInput();
    }

    public void WrongInput()
    {
        SequenceAndTarget seqTar = allInstances.Dequeue();
        seqTar.sequence.Kill();
        seqTar.target.GetComponent<MeshRenderer>().material.color = wrongInput;
        Debug.Log(seqTar.target, seqTar.target);
        Debug.Break();
        Color tempColor = new Color(wrongInput.r, wrongInput.g, wrongInput.b, 0);
        Sequence seq = DOTween.Sequence()
            .Append(DOTween.To(() => seqTar.target.GetComponent<MeshRenderer>().material.color, x => seqTar.target.GetComponent<MeshRenderer>().material.color = x, tempColor, 0.2f))
            .AppendCallback(() => Destroy(seqTar.target));
    }
}
