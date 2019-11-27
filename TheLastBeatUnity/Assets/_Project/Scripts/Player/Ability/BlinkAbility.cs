using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BlinkParams : AbilityParams
{
    public float BlinkSpeed = 0;
    public ParticleSystem BlinkParticles = null;
    public AK.Wwise.Event Sound = null;
    public float PulseCost = 0;
}

public class BlinkAbility : Ability
{
    float speed = 5;
    ParticleSystem particles = null;
    float pulsationCost;
    AK.Wwise.Event soundBlink;
    Sequence currentSequence = null;

    public BlinkAbility(BlinkParams bp) : base(bp.AttachedPlayer)
    {
        speed = bp.BlinkSpeed;
        particles = bp.BlinkParticles;
        pulsationCost = bp.PulseCost;
        soundBlink = bp.Sound;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero)
            Blink();
    }

    private void Blink()
    {
        Vector3 startSize = player.VisualRepr.localScale;
        Vector3 newPosition = player.transform.position + player.CurrentDirection * speed * 5;

        currentSequence = DOTween.Sequence();
        currentSequence.AppendCallback(() =>
        {
            player.Status.StartBlink();
            if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
            {
                BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
            }
            player.Health.ModifyPulseValue(pulsationCost);
            soundBlink.Post(player.gameObject);
        });
        currentSequence.Append(player.VisualRepr.DOScale(Vector3.zero, 0.05f));
        currentSequence.Append(player.transform.DOMove(newPosition, 0.2f));
        currentSequence.Append(player.VisualRepr.DOScale(startSize, 0.05f));
        currentSequence.AppendCallback(() =>
        {
            player.Status.StopBlink();
        });
        currentSequence.Play();
    }

    public override void End()
    {
        //particles.Stop();

        if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
        {
            BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
        }
    }
}
