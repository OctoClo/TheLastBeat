using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Rock : Beatable
{
    [SerializeField]
    Light light;

    [SerializeField]
    AnimationCurve curveIn;

    [SerializeField]
    AnimationCurve curveOut;

    public override void Beat()
    {
        
    }
}
