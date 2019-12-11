using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateStun : EnemyState
{
    float stunDuration = 0;
    float stunTimer = 0;

    public EnemyStateStun(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.STUN;
        stunDuration = 1;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.BeginStun();
        stunTimer = stunDuration;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        stunTimer -= deltaTime;

        if (stunTimer <= 0)
            return EEnemyState.CHASE;

        return stateEnum;
    }

    public override void Exit()
    {
        enemy.EndStun();
    }
}
