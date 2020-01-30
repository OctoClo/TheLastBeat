using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CorruptionPulse : Beatable
{
    [SerializeField]
    AnimationCurve curve = null;
    [SerializeField]
    float targetValue = 10;
    float originValue = 0;
    [SerializeField]
    int countBeforeSetup = 2;

    Material[] materials;
    Color col = Color.white;

    protected override void Start()
    {
        base.Start();
        materials = GetComponent<SkinnedMeshRenderer>().materials;

        if (materials.Length > 0)
            col = materials[0].HasProperty("_EmissionColor") ? materials[0].GetVector("_EmissionColor") : new Vector4(1, 1, 1, 1);
    }

    public override void Beat()
    {
        countBeforeSetup--;
        if (countBeforeSetup <= 0)
        {
            foreach (Material mat in materials)
            {
                DOTween.Sequence()
                .Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", col * x), targetValue, sequenceDuration)
                    .SetEase(curve))
                .SetUpdate(true);
            }
        }
    }
}
