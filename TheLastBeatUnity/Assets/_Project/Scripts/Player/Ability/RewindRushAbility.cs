using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class RewindRushParameters : AbilityParams
{
    public float Duration = 0;
    public float PulsationCost = 0;
    public AK.Wwise.State RewindState;
    public AK.Wwise.State NormalState;
    public float delayBeforeEnd;
}

public class RewindRushAbility : Ability
{
    float duration = 0;
    float pulsationCost;
    float rushChainTimer = 0;
    float delayBeforeEnd = 0;
    AK.Wwise.State rewindState;
    AK.Wwise.State normalState;
    List<Enemy> chainedEnemies = new List<Enemy>();

    public RewindRushAbility(RewindRushParameters rrp) : base(rrp.AttachedPlayer)
    {
        duration = rrp.Duration;
        pulsationCost = rrp.PulsationCost;
        rewindState = rrp.RewindState;
        normalState = rrp.NormalState;
        delayBeforeEnd = rrp.delayBeforeEnd;
    }

    public override void Launch()
    {
        if (chainedEnemies.Count > 0)
            RewindRush();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        if (chainedEnemies.Count > 0 && !player.Status.Dashing)
        {
            rushChainTimer -= Time.deltaTime;

            if (rushChainTimer < 0)
                chainedEnemies.Clear();
        }
    }

    public void AddChainEnemy(Enemy enn)
    {
        rushChainTimer = delayBeforeEnd;
        chainedEnemies.Add(enn);
    }

    void RewindRush()
    {
        rewindState.SetValue();
        player.Status.StartDashing();
        player.FocusZone.overrideControl = true;
        player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");

        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => player.Health.ModifyPulseValue(pulsationCost));
        Vector3 direction;
        Vector3 goalPosition = player.transform.position;

        chainedEnemies.Reverse();

        foreach (Enemy enemy in chainedEnemies)
        {
            if (enemy)
            {
                player.FocusZone.OverrideCurrentEnemy(enemy);

                direction = new Vector3(enemy.transform.position.x, goalPosition.y, enemy.transform.position.z) - goalPosition;
                direction *= 1.3f;

                goalPosition += direction;
                seq.AppendCallback(() => player.Anim.LaunchAnim(EPlayerAnim.RUSHING));
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
