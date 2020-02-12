using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpecialMonolithPulse : Beatable
{
    [SerializeField]
    AnimationCurve curve = null;

    [SerializeField]
    float targetValue = 5;
    [SerializeField]
    float newTargetValue = 10;
    float originValue = 0;
    
    [SerializeField]
    Color newEmissiveColor = Color.white;
    Color emissiveColor = Color.white;
    Sequence currentSeq = null;

    int countBeforeSetup = 2;
    Material mat = null;

    protected override void Start()
    {
        base.Start();
        mat = GetComponent<MeshRenderer>().material;
        originValue = 0;
        emissiveColor = mat.GetVector("_EmissionColor");
    }

    public override void Beat()
    {
        countBeforeSetup--;
        if (countBeforeSetup <= 0)
        {
            currentSeq = DOTween.Sequence()
                                    .Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", emissiveColor * x), targetValue, sequenceDuration)
                                    .SetEase(curve)).SetUpdate(true).Play();
        }
    }

    public void ChangeColor()
    {
        currentSeq.Kill();
        emissiveColor = newEmissiveColor;
        targetValue = newTargetValue;
        mat.SetVector("_EmissionColor", emissiveColor * newTargetValue);
    }
}
