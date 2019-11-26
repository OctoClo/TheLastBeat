using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateRecoverAttack : EnemyState
{
    float recoverDuration = 2;
    float recoverTimer = 0;

    public EnemyStateRecoverAttack(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.RECOVER_ATTACK;
    }

    public override void Enter()
    {
        enemy.SetStateText("recover");

        recoverTimer = recoverDuration;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        recoverTimer -= deltaTime;

        if (recoverTimer <= 0)
            return EEnemyState.PREPARE_ATTACK;
        
        if (!enemy.WeaponHitbox.PlayerInHitbox)
            return EEnemyState.CHASE;
        
        return stateEnum;
    }
}
