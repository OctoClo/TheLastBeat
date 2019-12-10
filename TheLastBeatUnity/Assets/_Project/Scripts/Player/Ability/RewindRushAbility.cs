using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class RewindRushParameters : AbilityParams
{
    public float Duration = 0;
    public float PulseCost = 0;
    public float Cooldown = 0;
    public float MaxTimeBeforeResetMarks = 0;
    public AK.Wwise.State RewindState = null;
    public AK.Wwise.State NormalState = null;
    public int MaxChained = 5;
}

public class RewindRushAbility : Ability
{
    float duration = 0;
    float pulseCost = 0;
    int missedInput = 0;
    int maxChained = 0;
    Queue<Enemy> chainedEnemies = new Queue<Enemy>();
    public bool IsInCombo => chainedEnemies.Count > 0;
    float maxTimeBeforeResetMarks = 0;
    float rushChainTimer = 0;
    AK.Wwise.State rewindState = null;
    AK.Wwise.State normalState = null;
    bool attackOnRythm = false;

    public RewindRushAbility(RewindRushParameters rrp) : base(rrp.AttachedPlayer)
    {
        duration = rrp.Duration;
        pulseCost = rrp.PulseCost;
        maxTimeBeforeResetMarks = rrp.MaxTimeBeforeResetMarks;
        rewindState = rrp.RewindState;
        normalState = rrp.NormalState;
        cooldown = rrp.Cooldown;
        healCorrectBeat = rrp.HealPerCorrectBeat;
        maxChained = rrp.MaxChained;
    }

    public void ResetCombo()
    {
        chainedEnemies.Clear();
        missedInput = 0;
    }

    public void MissInput()
    {
        missedInput++;
        if (missedInput >= 2)
        {
            ResetCombo();
        }
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
                ResetCombo();
        }

        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(currentCooldown - deltaTime, 0.0f);
        }
    }

    public void AddChainEnemy(Enemy enn)
    {
        rushChainTimer = maxTimeBeforeResetMarks;
        if (chainedEnemies.Count >= maxChained)
        {
            chainedEnemies.Dequeue();
        }
        chainedEnemies.Enqueue(enn);
    }

    void RewindRush()
    {
        currentCooldown = cooldown;
        rewindState.SetValue();
        player.Status.StartDashing();
        player.FocusZone.overrideControl = true;
        player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");

        attackOnRythm = BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT);
        if (attackOnRythm)
        {
            player.Health.ModifyPulseValue(-healCorrectBeat);
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

        foreach (Enemy enemy in chainedEnemies.Reverse())
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
                seq.AppendCallback(() => { enemy.GetAttacked(attackOnRythm); });
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
        ResetCombo();
        normalState.SetValue();
    }
}
