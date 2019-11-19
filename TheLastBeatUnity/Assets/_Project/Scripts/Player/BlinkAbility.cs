using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkAbility : Ability
{
    float speed = 5;
    ParticleSystem particles = null;

    public BlinkAbility(Player newPlayer, float blinkSpeed, ParticleSystem blinkParticles) : base(newPlayer)
    {
        speed = blinkSpeed;
        particles = blinkParticles;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero)
            Blink();
    }

    private void Blink()
    {
        //particles.Play();
        player.Anim.LaunchAnim(EPlayerAnim.BLINKING);
        player.transform.position = player.transform.position + player.CurrentDirection * speed;
        End();
        
    }

    public override void End()
    {
        //particles.Stop();
    }
}
