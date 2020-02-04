using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyPulse : Beatable
{
    [SerializeField]
    AnimationCurve curve = null;
    [SerializeField]
    float targetValue = 10;
    float originValue = 0;
    [SerializeField]
    int countBeforeSetup = 2;

    [SerializeField]
    Color color = Color.white;
    List<Material> materials = new List<Material>();
    List<Sequence> sequences = new List<Sequence>();

    protected override void Start()
    {
        base.Start();

        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
            materials.AddRange(renderer.materials);
    }

    public override void Beat()
    {
        countBeforeSetup--;
        if (countBeforeSetup <= 0)
        {
            sequences.Clear();
            foreach (Material mat in materials)
            {
                sequences.Add(DOTween.Sequence()
                                .Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", color * x), targetValue, sequenceDuration)
                                    .SetEase(curve))
                                .SetUpdate(true));
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Sequence sequence in sequences)
        {
            if (sequence != null)
                sequence.Kill();
        }
    }
}

