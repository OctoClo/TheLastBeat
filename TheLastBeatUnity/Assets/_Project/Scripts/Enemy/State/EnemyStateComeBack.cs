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

        enemy.WanderZone.GetRandomPosition(out goal, enemy.transform.position.y);
        enemy.Agent.enabled = true;
        enemy.Agent.SetDestination(goal);
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (enemy.ChaseAgain)
        {
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
