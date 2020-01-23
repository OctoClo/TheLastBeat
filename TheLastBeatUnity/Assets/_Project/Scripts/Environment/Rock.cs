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

    [SerializeField]
    int countBeforeSetup = 2;
    Material mat;
    Color col;

    protected override void Start()
    {
        base.Start();
        mat = GetComponent<MeshRenderer>().material;
        originValue = 0;
        col = mat.GetVector("_EmissionColor");
    }

    public override void Beat()
    {
        countBeforeSetup--;
        if (countBeforeSetup == 0)
        {
            DOTween.Sequence()
                .Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", col * x), targetValue, sequenceDuration)
                    .SetEase(curve))
                .AppendInterval(SoundManager.Instance.TimePerBeat - sequenceDuration)
                .SetLoops(-1)
                .SetUpdate(true);
        }
    }
}
