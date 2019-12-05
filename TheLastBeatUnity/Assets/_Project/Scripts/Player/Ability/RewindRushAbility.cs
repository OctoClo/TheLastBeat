using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class RewindRushParameters : AbilityParams
{
    public float Duration = 0;
    public float PulseCost = 0;
    public float Cooldown = 0;
    public float MaxTimeBeforeResetMarks = 0;
    public AK.Wwise.State RewindState = null;
    public AK.Wwise.State NormalState = null;
}

public class RewindRushAbility : Ability
{
    float duration = 0;
    float pulseCost = 0;
    float cooldown = 0;
    float currentCooldown = 0;

    List<Enemy> chainedEnemies = new List<Enemy>();
    float maxTimeBeforeResetMarks = 0;
    float rushChainTimer = 0;

    AK.Wwise.State rewindState = null;
    AK.Wwise.State normalState = null;

    public RewindRushAbility(RewindRushParameters rrp) : base(rrp.AttachedPlayer)
    {
        duration = rrp.Duration;
        pulseCost = rrp.PulseCost;
        maxTimeBeforeResetMarks = rrp.MaxTimeBeforeResetMarks;
        rewindState = rrp.RewindState;
        normalState = rrp.NormalState;
        cooldown = rrp.Cooldown;
    }

    public override void Launch()
    {
        if (chainedEnemies.Count > 0 && currentCooldown == 0)
            RewindRush();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (chainedEnemies.Count > 0 && !player.Status.Dashing && currentCooldown == 0)
        {
            rushChainTimer -= Time.deltaTime;

            if (rushChainTimer < 0)
                chainedEnemies.Clear();
        }

        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(currentCooldown - deltaTime, 0.0f);
        }
    }

    public void AddChainEnemy(Enemy enn)
    {
        rushChainTimer = maxTimeBeforeResetMarks;
        chainedEnemies.Add(enn);
    }

    void RewindRush()
    {
        currentCooldown = cooldown;
        rewindState.SetValue();
        player.Status.StartDashing();
        player.FocusZone.overrideControl = true;
        player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");

        if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
        {
            BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
        }
        else
        {
            if (player.Health.InCriticMode)
            {
                player.Die();
            }
            player.Health.ModifyPulseValue(pulseCost);
        }

        Sequence seq = DOTween.Sequence();
        Vector3 direction;
        Vector3 goalPosition = player.transform.position;

        chainedEnemies.Reverse();

        foreach (Enemy enemy in chainedEnemies)
        {
            if (enemy)
            {
                direction = new Vector3(enemy.transform.position.x, goalPosition.y, enemy.transform.position.z) - goalPosition;
                direction *= 1.3f;

                goalPosition += direction;
                seq.AppendCallback(() =>
                {
                    player.FocusZone.OverrideCurrentEnemy(enemy);
                    player.LookAtCurrentTarget();
                    player.Anim.LaunchAnim(EPlayerAnim.RUSHING);
                });
                seq.Append(player.transform.DOMove(goalPosition, duration));
                seq.AppendCallback(() => { enemy.GetAttacked(); });
            }
        }

        seq.AppendCallback(() => End());

        seq.Play();
    }

    public override void End()
    {
        player.Status.StopDashing();
        player.FocusZone.overrideControl = false;
        player.gameObject.layer = LayerMask.NameToLayer("Default");
        chainedEnemies.Clear();
        normalState.SetValue();
    }
}
