using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BlinkParams : AbilityParams
{
    public float Speed = 0;
    public float PulseCost = 0;
    public float Cooldown = 0;
    public AK.Wwise.Event Sound = null;
}

public class BlinkAbility : Ability
{
    float speed = 5;
    float pulseCost = 0;

    float currentCooldown = 0;
    float cooldown = 0;
    AK.Wwise.Event soundBlink = null;

    Sequence currentSequence = null;

    public BlinkAbility(BlinkParams bp) : base(bp.AttachedPlayer)
    {
        speed = bp.Speed;
        pulseCost = bp.PulseCost;
        soundBlink = bp.Sound;
        cooldown = bp.Cooldown;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero && currentCooldown == 0)
            Blink();
    }

    public override void Update(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(0, currentCooldown - deltaTime);
        }
    }

    private void Blink()
    {
        Vector3 startSize = player.VisualPart.localScale;
        Vector3 newPosition = player.transform.position + player.CurrentDirection * speed;

        currentSequence = DOTween.Sequence();
        currentSequence.AppendCallback(() =>
        {
            currentCooldown = cooldown;
            player.Status.StartBlink();
            if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
            {
                BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
            }
            else
            {
                player.Health.ModifyPulseValue(pulseCost);
                if (player.Health.InCriticMode)
                {
                    player.Die();
                }
            }
            soundBlink.Post(player.gameObject);
        });
        currentSequence.Append(player.VisualPart.DOScale(Vector3.zero, 0.05f));
        currentSequence.Append(player.transform.DOMove(newPosition, 0.2f));
        currentSequence.Append(player.VisualPart.DOScale(startSize, 0.05f));
        currentSequence.AppendCallback(() =>
        {
            player.Status.StopBlink();
        });
        currentSequence.Play();
    }
}
