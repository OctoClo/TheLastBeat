using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateWander : EnemyState
{
    Sequence currentMove = null;
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

        currentMove = null;
        waitTimer = RandomHelper.GetRandom(waitDurationMinMax.x, waitDurationMinMax.y);
    }

    void StartNewMove()
    {
        currentMove = DOTween.Sequence();
        enemy.WanderZone.GetRandomPosition(out nextPosition, enemy.transform.position.y);
        currentMove.Append(enemy.transform.DOLookAt(nextPosition, 1, AxisConstraint.Y));
        currentMove.Append(enemy.transform.DOMove(nextPosition, Vector3.Distance(enemy.transform.position, nextPosition) / enemy.Speed));
        currentMove.AppendCallback(() =>
        {
            waitTimer = RandomHelper.GetRandom(waitDurationMinMax.x, waitDurationMinMax.y);
            currentMove = null;
        });
        currentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (waitTimer > 0)
        {
            waitTimer -= deltaTime;

            if (waitTimer <= 0)
                StartNewMove();
        }

        if (enemy.DetectionZone.PlayerInZone)
        {
            if (currentMove != null)
                currentMove.Kill();

            return EEnemyState.CHASE;
        }
        
        return stateEnum;
    }
}
