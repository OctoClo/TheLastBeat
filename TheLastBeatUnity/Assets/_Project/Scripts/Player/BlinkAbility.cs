using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkAbility : Ability
{
    float speed = 5;
    ParticleSystem particles = null;
    float pulsationCost;

    public BlinkAbility(Player newPlayer, float blinkSpeed, ParticleSystem blinkParticles, float newCost) : base(newPlayer)
    {
        speed = blinkSpeed;
        particles = blinkParticles;
        pulsationCost = newCost;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero)
            Blink();
    }

    private void Blink()
    {
        //particles.Play();
        player.Health.ModifyPulseValue(pulsationCost);
        player.transform.position = player.transform.position + player.CurrentDirection * speed;
        //particles.Stop();

        if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
        {
            Debug.Log("rythm");
        }
    }
}
