using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateStun : EnemyState
{
    float stunDuration = 0;
    bool animationFinished = false;

    public EnemyStateStun(Enemy newEnemy, float duration) : base(newEnemy)
    {
        stateEnum = EEnemyState.STUN;
        stunDuration = duration;
    }

    public override void Enter()
    {
        base.Enter();

        animationFinished = false;

        enemy.BeginStun();
        enemy.Animator.SetTrigger("stun");

        Sequence animation = DOTween.Sequence();
        animation.Append(enemy.transform.DOShakeRotation(stunDuration, 10, 90));
        animation.AppendCallback(() => animationFinished = true);
        animation.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.CHASE;

        return stateEnum;
    }

    public override void Exit()
    {
        enemy.EndStun();
    }
}
