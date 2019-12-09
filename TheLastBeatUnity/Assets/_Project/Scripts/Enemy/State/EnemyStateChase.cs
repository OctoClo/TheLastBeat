using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateChase : EnemyState
{
    bool playerInHitbox = false;

    public EnemyStateChase(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.CHASE;
    }

    public override void Enter()
    {
        base.Enter();

        playerInHitbox = false;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        Vector3 toPlayer = enemy.Player.position - enemy.transform.position;
        toPlayer.y = 0;
        toPlayer.Normalize();
        enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, Quaternion.LookRotation(toPlayer), 10);

        if (enemy.ComeBack)
        {
            enemy.ComeBack = false;
            return EEnemyState.COME_BACK;
        }
        
        if (!enemy.WeaponHitbox.PlayerInHitbox)
        {
            enemy.transform.position += toPlayer * deltaTime * enemy.Speed;
        }
        else
        {
            return EEnemyState.PREPARE_ATTACK;
        }
        
        return stateEnum;
    }
}
