using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateAttack : EnemyState
{
    Transform player = null;
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
        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(0.5f);
        seq.Append(enemy.transform.DOScale(scaleEndValues, 0.75f).SetEase(Ease.OutBounce));
        seq.AppendCallback(() =>
        {
            animationFinished = true;
            // TODO: Modify player pulse
        });

        seq.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.RECOVER_ATTACK;

        return stateEnum;
    }
    
    
}
