using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateComeBack : EnemyState
{
    Sequence currentMove = null;
    Vector3 goal = Vector3.zero;
    bool backInWanderZone = false;

    public EnemyStateComeBack(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.COME_BACK;
    }

    public override void Enter()
    {
        base.Enter();
        backInWanderZone = false;
        
        currentMove = DOTween.Sequence();
        enemy.WanderZone.GetRandomPosition(out goal, enemy.transform.position.y);
        currentMove.Append(enemy.transform.DOLookAt(goal, 1, AxisConstraint.Y));
        currentMove.Append(enemy.transform.DOMove(goal, Vector3.Distance(enemy.transform.position, goal) / enemy.Speed));
        currentMove.AppendCallback(() =>
        {
            backInWanderZone = true;
            currentMove = null;
        });
        currentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (backInWanderZone)
            return EEnemyState.WANDER;

        return stateEnum;
    }
}
