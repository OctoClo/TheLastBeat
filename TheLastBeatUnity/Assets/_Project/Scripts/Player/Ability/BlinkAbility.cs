using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BlinkParams : AbilityParams
{
    public float Speed = 0;
    public float PulseCost = 0;
    public ParticleSystem Particles = null;
    public AK.Wwise.Event Sound = null;
}

public class BlinkAbility : Ability
{
    float speed = 5;
    float pulseCost = 0;

    ParticleSystem particles = null;
    AK.Wwise.Event soundBlink = null;

    Sequence currentSequence = null;

    public BlinkAbility(BlinkParams bp) : base(bp.AttachedPlayer)
    {
        speed = bp.Speed;
        particles = bp.Particles;
        pulseCost = bp.PulseCost;
        soundBlink = bp.Sound;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero)
            Blink();
    }

    private void Blink()
    {
        Vector3 startSize = player.Model.localScale;
        Vector3 newPosition = player.transform.position + player.CurrentDirection * speed * 5;

        currentSequence = DOTween.Sequence();
        currentSequence.AppendCallback(() =>
        {
            player.Status.StartBlink();
            if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
            {
                BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
            }
            player.Health.ModifyPulseValue(pulseCost);
            soundBlink.Post(player.gameObject);
        });
        currentSequence.Append(player.Model.DOScale(Vector3.zero, 0.05f));
        currentSequence.Append(player.transform.DOMove(newPosition, 0.2f));
        currentSequence.Append(player.Model.DOScale(startSize, 0.05f));
        currentSequence.AppendCallback(() =>
        {
            player.Status.StopBlink();
        });
        currentSequence.Play();
    }

    public override void End()
    {
        if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
        {
            BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
        }
    }
}
