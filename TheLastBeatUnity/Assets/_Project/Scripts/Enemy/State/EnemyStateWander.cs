using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateWander : EnemyState
{
    Vector3 nextPosition = Vector3.zero;

    Vector2 waitDurationMinMax = Vector2.zero;
    float waitTimer = 0;

    public EnemyStateWander(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.WANDER;
        waitDurationMinMax = new Vector2(2, 5);
    }

    public override void Enter()
    {
        base.Enter();

        waitTimer = 0;
    }

    void StartNewMove()
    {
        enemy.CurrentMove = DOTween.Sequence();
        enemy.WanderZone.GetRandomPosition(out nextPosition, enemy.transform.position.y);
        enemy.CurrentMove.Append(enemy.transform.DOLookAt(nextPosition, 1, AxisConstraint.Y));
        enemy.CurrentMove.Append(enemy.transform.DOMove(nextPosition, Vector3.Distance(enemy.transform.position, nextPosition) / enemy.Speed));
        enemy.CurrentMove.AppendCallback(() => { enemy.CurrentMove = null; });
        enemy.CurrentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (enemy.ChaseAgain)
        {
            enemy.KillCurrentTween();
            enemy.ChaseAgain = false;
            return EEnemyState.CHASE;
        }
        
        if (waitTimer > 0)
        {
            waitTimer -= deltaTime;

            if (waitTimer <= 0)
                StartNewMove();
        }

        if (waitTimer <= 0 && enemy.CurrentMove == null)
        {
            waitTimer = RandomHelper.GetRandom(waitDurationMinMax.x, waitDurationMinMax.y);
        }

        if (enemy.DetectionZone.PlayerInZone)
        {
            enemy.KillCurrentTween();
            return EEnemyState.CHASE;
        }
        
        return stateEnum;
    }
}
