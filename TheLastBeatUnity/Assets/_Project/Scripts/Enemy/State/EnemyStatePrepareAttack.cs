using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStatePrepareAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    bool animationFinished = false;
    float waitBeforeAnimDuration = 0;
    float animDuration = 0;

    public EnemyStatePrepareAttack(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.PREPARE_ATTACK;
        scaleEndValues = new Vector3(1.5f, 1.5f, 1.5f);
        waitBeforeAnimDuration = 0.5f;
        animDuration = 2;
    }

    public override void Enter()
    {
        enemy.SetStateText("prepare");

        animationFinished = false;
        enemy.CurrentMove = DOTween.Sequence();

        enemy.CurrentMove.Insert(waitBeforeAnimDuration, enemy.transform.DOShakePosition(animDuration, 0.5f, 100));
        enemy.CurrentMove.Insert(waitBeforeAnimDuration, enemy.transform.DOScale(scaleEndValues, animDuration));
        enemy.CurrentMove.AppendCallback(() =>
        {
            animationFinished = true;
            enemy.CurrentMove = null;
        });

        enemy.CurrentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.ATTACK;
        
        return stateEnum;
    }
}
