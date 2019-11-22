using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkAbility : Ability
{
    float speed = 5;
    ParticleSystem particles = null;
    float pulsationCost;
    AK.Wwise.Event soundBlink;

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
        soundBlink.Post(player.gameObject);
        //particles.Play();
        player.Health.ModifyPulseValue(pulsationCost);
        player.transform.position = player.transform.position + player.CurrentDirection * speed;
        //particles.Stop();

        if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
        {
            BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
        }
    }
}
