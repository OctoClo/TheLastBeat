using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    bool animationFinished = false;

    public EnemyStateAttack(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.ATTACK;
        scaleEndValues = new Vector3(1, 1, 1);
    }

    public override void Enter()
    {
        base.Enter();

        animationFinished = false;

        enemy.CurrentMove = DOTween.Sequence();

        enemy.CurrentMove.AppendInterval(0.25f);
        enemy.CurrentMove.Append(enemy.transform.DOScale(scaleEndValues, 0.5f).SetEase(Ease.OutBounce));
        enemy.CurrentMove.AppendCallback(() =>
        {
            if (enemy.WeaponHitbox.PlayerInHitbox)
            {
                if (enemy.Player.Health.InCriticMode)
                {
                    enemy.Player.Die();
                }
                enemy.Player.Health.ModifyPulseValue(enemy.PulseDamage);
            }
            
            animationFinished = true;
            enemy.CurrentMove = null;
        });

        enemy.CurrentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.RECOVER_ATTACK;

        return stateEnum;
    }
}
