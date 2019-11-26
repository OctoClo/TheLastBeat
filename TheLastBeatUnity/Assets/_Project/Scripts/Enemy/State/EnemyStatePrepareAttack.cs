using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStatePrepareAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    bool animationFinished = false;

    public EnemyStatePrepareAttack(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.PREPARE_ATTACK;
        scaleEndValues = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public override void Enter()
    {
        base.Enter();

        animationFinished = false;
        Sequence seq = DOTween.Sequence();

        seq.Append(enemy.transform.DOScale(scaleEndValues, 2));
        seq.AppendCallback(() => animationFinished = true);

        seq.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.ATTACK;
        
        return stateEnum;
    }
}
