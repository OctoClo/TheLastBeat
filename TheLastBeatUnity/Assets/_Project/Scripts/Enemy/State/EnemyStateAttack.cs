using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    Sequence animation = null;
    bool animationFinished = false;
    float waitBeforeAnimDuration = 0;
    float animDuration = 0;
    float impulseForce = 0;
    float blastForce = 0;

    public EnemyStateAttack(Enemy newEnemy, float waitBefore, float duration, float impulse, float force) : base(newEnemy)
    {
        stateEnum = EEnemyState.ATTACK;
        scaleEndValues = new Vector3(1, 1, 1);
        waitBeforeAnimDuration = waitBefore;
        animDuration = duration;
        impulseForce = impulse;
        blastForce = force;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.StartAttacking();
        animationFinished = false;

        Vector3 goalPos = enemy.transform.forward * impulseForce;
        goalPos += enemy.transform.position;

        animation = DOTween.Sequence();

        animation.Insert(waitBeforeAnimDuration, enemy.Model.transform.DOScale(scaleEndValues, animDuration).SetEase(Ease.OutBounce));
        animation.Insert(waitBeforeAnimDuration, enemy.transform.DOMove(goalPos, animDuration).SetEase(Ease.OutBounce));
        animation.AppendCallback(() => animationFinished = true);

        animation.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (enemy.HasAttackedPlayer)
        {
            animation.Kill();

            Rigidbody rb = enemy.Player.GetComponentInChildren<Rigidbody>();
            if (rb)
            {
                Vector3 force = enemy.transform.forward;
                force *= blastForce;
                rb.AddForce(force, ForceMode.VelocityChange);
            }
            
            return EEnemyState.RECOVER_ATTACK;
        }

        if (animationFinished)
            return EEnemyState.RECOVER_ATTACK;

        return stateEnum;
    }

    public override void Exit()
    {
        enemy.Model.transform.localScale = scaleEndValues;
        enemy.StopAttacking();
    }
}
