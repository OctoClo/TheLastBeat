using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateRecoverAttack : EnemyState
{
    float recoverDuration = 0;
    float recoverTimer = 0;

    public EnemyStateRecoverAttack(Enemy newEnemy, float duration) : base(newEnemy)
    {
        stateEnum = EEnemyState.RECOVER_ATTACK;
        recoverDuration = duration;
    }

    public override void Enter()
    {
        recoverTimer = recoverDuration;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        recoverTimer -= deltaTime;

        if (recoverTimer <= 0)
            return EEnemyState.CHASE;
        
        return stateEnum;
    }
}
