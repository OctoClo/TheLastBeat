using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Rock : Beatable
{
    Vector3 scale;

    [SerializeField]
    float coeff = 0;

    private void Start()
    {
        scale = transform.localScale;
    }

    public override void Beat()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(coeff, SequenceDuration));
        seq.Append(transform.DOScale(1, SequenceDuration));
        seq.Play();
    }
}
