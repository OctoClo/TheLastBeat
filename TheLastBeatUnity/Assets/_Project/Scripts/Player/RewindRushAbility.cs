using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RewindRushAbility : Ability
{
    float duration = 0;
    float pulsationCost;

    public RewindRushAbility(Player newPlayer, float rewindRushDuration, float newCost) : base(newPlayer)
    {
        duration = rewindRushDuration;
        pulsationCost = newCost;
    }

    public override void Launch()
    {
        RewindRush();
    }

    void RewindRush()
    {
        player.Status.StartDashing();
        player.FocusZone.overrideControl = true;
        player.gameObject.layer = LayerMask.NameToLayer("Player Dashing");

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
                seq.Append(player.transform.DOMove(goalPosition, duration));
                seq.AppendCallback(() => { enemy.GetAttacked(); });
            }
        }

        seq.Play();

        player.Status.StopDashing();
        player.FocusZone.overrideControl = false;
        player.gameObject.layer = LayerMask.NameToLayer("Default");
        player.ResetChainedEnemies();
    }
}
