using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Rock : Beatable
{
    [SerializeField]
    AnimationCurve curve = null;

    [SerializeField]
    float targetValue = 0;
    float originValue = 0;

    Sequence currentSequence;
    Material mat;
    Color col;

    protected override void Start()
    {
        base.Start();
        mat = GetComponent<MeshRenderer>().material;
        originValue = 1.5f;
        col = mat.GetVector("_EmissionColor");
    }

    public override void Beat()
    {
        currentSequence = DOTween.Sequence();
        currentSequence.Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", col * x), targetValue, sequenceDuration / 2.0f).SetEase(curve));
        currentSequence.Append(DOTween.To(() => targetValue, x => mat.SetVector("_EmissionColor", col * x), originValue, sequenceDuration / 2.0f).SetEase(curve));
        currentSequence.Play();
    }
}
