using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ERockState
{
    PULSE_ON_BEAT,
    ILLUMINATE,
    NOTHING
}

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

    ERockState state = ERockState.PULSE_ON_BEAT;
    Sequence currentSeq = null;
    bool initialized = false;

    protected override void Start()
    {
        base.Start();
        Init();
    }

    private void Init()
    {
        if (!initialized)
        {
            mat = GetComponent<MeshRenderer>().material;
            col = mat.GetVector("_EmissionColor");
            originValue = 0;
            initialized = true;
        }
    }

    public void ChangeState(ERockState newState)
    {
        state = newState;
        if (currentSeq != null)
            currentSeq.Kill();
        if (state == ERockState.ILLUMINATE)
        {
            Init();
            mat.SetVector("_EmissionColor", col * targetValue);
        }
    }

    public override void Beat()
    {
        countBeforeSetup--;
        if (countBeforeSetup <= 0 && state == ERockState.PULSE_ON_BEAT)
        {
            currentSeq = DOTween.Sequence()
                            .Append(DOTween.To(() => originValue, x => mat.SetVector("_EmissionColor", col * x), targetValue, sequenceDuration)
                                .SetEase(curve))
                            .SetUpdate(true)
                            .Play();
        }
    }
}
