using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyPulse : Beatable
{
    enum EPulseType
    {
        PULSE_ON_BEAT,
        PULSE_FASTER_AND_FASTER
    }

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

    EPulseType pulseType = EPulseType.PULSE_ON_BEAT;

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
            if (pulseType == EPulseType.PULSE_ON_BEAT)
                LaunchEmission(sequenceDuration);
        }
    }

    private void LaunchEmission(float duration)
    {
        sequences.Clear();
        foreach (Material mat in materials)
        {
            sequences.Add(DOTween.Sequence()
                            .Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", color * x), targetValue, duration)
                                .SetEase(curve))
                            .SetUpdate(true));
        }
    }

    public void PulseFasterAndFaster(float duration)
    {
        pulseType = EPulseType.PULSE_FASTER_AND_FASTER;
        KillSequences();
        StartCoroutine(PulseFasterAndFasterCoroutine());
    }

    private IEnumerator PulseFasterAndFasterCoroutine()
    {
        float intervalTime = 0.4f;
        bool continueToPulse = true;
        while (continueToPulse)
        {
            LaunchEmission(intervalTime);
            
            yield return new WaitForSeconds(intervalTime);
            
            intervalTime -= 0.04f;

            if (intervalTime <= 0)
                continueToPulse = false;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        KillSequences();
    }

    private void KillSequences()
    {
        foreach (Sequence sequence in sequences)
        {
            if (sequence != null)
                sequence.Kill();
        }
    }
}

