using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Rock : Beatable
{
    [SerializeField]
    AnimationCurve curve;

    [SerializeField]
    Color color;

    Sequence currentSequence;

    public override void Beat()
    {
        currentSequence = DOTween.Sequence();
        currentSequence.Play();
    }
}
