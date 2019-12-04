using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Rock : Beatable
{
    [SerializeField]
    AnimationCurve curve = null;

    [SerializeField]
    float targeValue = 0;
    float originValue = 0;

    Sequence currentSequence;
    Material mat;

    private void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        originValue = mat.GetFloat("_Bias");
    }

    public override void Beat()
    {
        currentSequence = DOTween.Sequence();
        currentSequence.Append(DOTween.To(() => originValue, x => mat.SetFloat("_Bias", x), targeValue, sequenceDuration / 2.0f).SetEase(curve));
        currentSequence.Append(DOTween.To(() => mat.GetFloat("_Bias"), x => mat.SetFloat("_Bias", x), originValue, sequenceDuration / 2.0f).SetEase(curve));
        currentSequence.Play();
    }
}
