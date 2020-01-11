using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateExplode : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    float waitBeforeAnimDuration = 0;
    float animDuration = 0;
    float waitBeforeBlast = 0;
    EnemyExplosionArea explosionArea = null;
    float blastForce = 0;

    public EnemyStateExplode(Enemy newEnemy, float waitBefore, float force, EnemyExplosionArea explosion) : base(newEnemy)
    {
        stateEnum = EEnemyState.ATTACK;
        scaleEndValues = new Vector3(7, 7, 7);
        waitBeforeAnimDuration = waitBefore;
        explosionArea = explosion;
        blastForce = force;
    }

    public override void Enter()
    {
        enemy.SetStateText("explode");
        enemy.StartAttacking();

        Sequence animation = DOTween.Sequence();

        animation.InsertCallback(waitBeforeAnimDuration, () =>
        {
            enemy.Model.SetActive(false);
            explosionArea.Explode(blastForce);
        });

        animation.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (explosionArea.ExplosionFinished)
        {
            enemy.Die();
        }

        return stateEnum;
    }
}
