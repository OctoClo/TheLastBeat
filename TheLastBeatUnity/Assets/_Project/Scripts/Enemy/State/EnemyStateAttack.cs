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

        enemy.CurrentMove.AppendInterval(0.5f);
        enemy.CurrentMove.Append(enemy.transform.DOScale(scaleEndValues, 0.75f).SetEase(Ease.OutBounce));
        enemy.CurrentMove.AppendCallback(() =>
        {
            animationFinished = true;
            enemy.CurrentMove = null;
            // TODO: Modify player pulse
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
