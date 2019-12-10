using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateComeBack : EnemyState
{
    Vector3 goal = Vector3.zero;

    public EnemyStateComeBack(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.COME_BACK;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.CurrentMove = DOTween.Sequence();
        enemy.WanderZone.GetRandomPosition(out goal, enemy.transform.position.y);
        enemy.CurrentMove.Append(enemy.transform.DOLookAt(goal, 1, AxisConstraint.Y));
        enemy.CurrentMove.Append(enemy.transform.DOMove(goal, Vector3.Distance(enemy.transform.position, goal) / enemy.Speed));
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

        if (enemy.InWanderZone)
        {
            return EEnemyState.WANDER;
        }

        return stateEnum;
    }
}
