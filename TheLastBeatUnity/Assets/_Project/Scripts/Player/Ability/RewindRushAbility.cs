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
    public float chainEnnemy;
}

public class RewindRushAbility : Ability
{
    float duration = 0;
    float pulsationCost;
    AK.Wwise.State rewindState;
    AK.Wwise.State normalState;

    public RewindRushAbility(RewindRushParameters rrp) : base(rrp.AttachedPlayer)
    {
        duration = rrp.Duration;
        pulsationCost = rrp.PulsationCost;
        rewindState = rrp.RewindState;
        normalState = rrp.NormalState;
    }

    public override void Launch()
    {
        RewindRush();
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

        List<Enemy> chainedEnemies = player.GetChainedEnemies();
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
        player.ResetChainedEnemies();
        normalState.SetValue();
    }
}
