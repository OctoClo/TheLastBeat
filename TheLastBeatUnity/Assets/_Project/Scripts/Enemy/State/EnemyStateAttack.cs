using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    bool animationFinished = false;
    float waitBeforeAnimDuration = 0;
    float animDuration = 0;
    float impulseForce = 0;

    public EnemyStateAttack(Enemy newEnemy, float waitBefore, float duration, float impulse) : base(newEnemy)
    {
        stateEnum = EEnemyState.ATTACK;
        scaleEndValues = new Vector3(1, 1, 1);
        waitBeforeAnimDuration = waitBefore;
        animDuration = duration;
        impulseForce = impulse;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.StartAttacking();
        animationFinished = false;

        Vector3 goalPos = enemy.Player.transform.position - enemy.transform.position;
        goalPos.y = 0;
        goalPos.Normalize();
        goalPos *= impulseForce;
        goalPos += enemy.transform.position;

        enemy.CurrentMove = DOTween.Sequence();

        enemy.CurrentMove.Insert(waitBeforeAnimDuration, enemy.transform.DOScale(scaleEndValues, animDuration).SetEase(Ease.OutBounce));
        enemy.CurrentMove.Insert(waitBeforeAnimDuration, enemy.transform.DOMove(goalPos, animDuration).SetEase(Ease.OutBounce));
        enemy.CurrentMove.AppendCallback(() => animationFinished = true);

        enemy.CurrentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.RECOVER_ATTACK;

        return stateEnum;
    }

    public override void Exit()
    {
        enemy.CurrentMove = null;
        enemy.StopAttacking();
    }
}
