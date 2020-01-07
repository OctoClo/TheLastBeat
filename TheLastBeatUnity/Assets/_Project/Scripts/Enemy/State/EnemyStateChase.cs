using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateChase : EnemyState
{
    public EnemyStateChase(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.CHASE;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (enemy.ComeBack)
        {
            enemy.ComeBack = false;
            return EEnemyState.COME_BACK;
        }
        
        if (!enemy.WeaponHitbox.PlayerInHitbox)
        {
            enemy.Agent.SetDestination(enemy.Player.transform.position);
        }
        else
        {
            return EEnemyState.PREPARE_ATTACK;
        }
        
        return stateEnum;
    }
}
