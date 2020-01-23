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
            col = materials[0].GetVector("_EmissionColor");
    }

    public override void Beat()
    {
        countBeforeSetup--;
        if (countBeforeSetup == 0)
        {
            foreach (Material mat in materials)
            {
                DOTween.Sequence()
                .Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", col * x), targetValue, sequenceDuration / 2.0f).SetEase(curve))
                .Append(DOTween.To(() => targetValue, x => mat.SetVector("_EmissionColor", col * x), originValue, sequenceDuration / 2.0f).SetEase(curve))
                .AppendInterval(SoundManager.Instance.LastBeat.beatInterval - sequenceDuration)
                .SetLoops(-1)
                .SetUpdate(true);
            }
        }
    }
}
