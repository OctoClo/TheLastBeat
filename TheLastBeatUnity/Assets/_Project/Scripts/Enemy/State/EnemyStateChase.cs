using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateChase : EnemyState
{
    bool launchAttack = false;

    public EnemyStateChase(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.CHASE;
    }

    public override void Enter()
    {
        base.Enter();
        launchAttack = false;
    }

    public override void OnBeat()
    {
        if (enemy.WeaponHitbox.PlayerInHitbox)
        {
            launchAttack = true;
        }
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (launchAttack)
        {
            return EEnemyState.PREPARE_ATTACK;
        }
        else if (enemy.ComeBack)
        {
            enemy.ComeBack = false;
            return EEnemyState.COME_BACK;
        }
        else
        {
            enemy.Agent.SetDestination(enemy.Player.transform.position);
            launchAttack = false;
            return stateEnum;
        }
    }
}
