using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        soundBlink.Post(player.gameObject);
        //particles.Play();
        player.Health.ModifyPulseValue(pulsationCost);
        player.Anim.LaunchAnim(EPlayerAnim.BLINKING);
        player.transform.position = player.transform.position + player.CurrentDirection * speed;
        End();
        
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
