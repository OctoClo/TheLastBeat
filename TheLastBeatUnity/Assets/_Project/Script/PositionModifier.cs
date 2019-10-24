using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PositionModifier
{
    protected bool isActive;
    public bool IsActive => isActive;
    public abstract void ApplyDelta(Transform input, float elapsed);
    public abstract void StartModifier();
}

public class Dash : PositionModifier
{
    protected float beginTime;
    protected float endTime;
    protected float duration;
    protected float strength;
    protected float actualTime;
    protected AnimationCurve curve;

    public Dash(float dur, float strth, AnimationCurve crv)
    {
        duration = dur;
        strength = strth;
        curve = crv;
    }

    public override void StartModifier()
    {
        isActive = true;
        beginTime = Time.timeSinceLevelLoad;
        endTime = beginTime + duration;
        actualTime = beginTime;
    }

    public override void ApplyDelta(Transform input , float elapsed)
    {
        actualTime += elapsed;
        float ratioSample = Mathf.Clamp(Mathf.Abs(actualTime - beginTime) / duration, 0, 1);
        float value = curve.Evaluate(ratioSample);
        Debug.Log(ratioSample + " / " + value);
        input.Translate(input.forward * strength * elapsed * value);
        if (ratioSample == 1)
        {
            isActive = false;
        }
    }
}
