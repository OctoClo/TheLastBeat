using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlinkAbility : Ability
{
    float speed = 5;
    ParticleSystem particles = null;
    float pulsationCost;
    AK.Wwise.Event soundBlink;
    Sequence currentSequence = null;

    public BlinkAbility(Player newPlayer, float blinkSpeed, ParticleSystem blinkParticles, AK.Wwise.Event sound, float newCost) : base(newPlayer)
    {
        speed = blinkSpeed;
        particles = blinkParticles;
        pulsationCost = newCost;
        soundBlink = sound;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero)
            Blink();
    }

    private void Blink()
    {
        //soundBlink.Post(player.gameObject);
        //particles.Play();
        //player.Health.ModifyPulseValue(pulsationCost);
        //player.Anim.LaunchAnim(EPlayerAnim.BLINKING);
        //player.transform.position = player.transform.position + player.CurrentDirection * speed;
        Vector3 startSize = player.VisualRepr.localScale;
        Vector3 newPosition = player.transform.position + player.CurrentDirection * speed * 10;

        currentSequence = DOTween.Sequence();
        currentSequence.AppendCallback(() =>
        {
            if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
            {
                BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
            }
            player.Health.ModifyPulseValue(pulsationCost);
            soundBlink.Post(player.gameObject);
        });
        currentSequence.Append(player.VisualRepr.DOScale(Vector3.zero, 0.1f));
        currentSequence.Append(player.transform.DOMove(newPosition, 1.5f));
        currentSequence.Append(player.VisualRepr.DOScale(startSize, 0.1f));
        currentSequence.AppendCallback(() =>
        {
            
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
