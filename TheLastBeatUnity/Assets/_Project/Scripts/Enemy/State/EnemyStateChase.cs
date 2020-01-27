using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateChase : EnemyState
{
    bool launchAttack = false;
    float distanceFollowMax = 0;

    public EnemyStateChase(Enemy newEnemy, float distanceFollow) : base(newEnemy)
    {
        stateEnum = EEnemyState.CHASE;
        distanceFollowMax = distanceFollow;
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
        else if (Vector3.SqrMagnitude(enemy.transform.position - enemy.Player.transform.position) < distanceFollowMax)
        {
            // Player is too close, only look at him
            enemy.Agent.ResetPath();
            Vector3 targetDirection = enemy.Player.transform.position - enemy.transform.position;
            float singleStep = 2 * deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(enemy.transform.forward, targetDirection, singleStep, 0.0f);
            enemy.transform.rotation = Quaternion.LookRotation(newDirection);
        }
        else
        {
            // Follow player
            enemy.Agent.SetDestination(enemy.Player.transform.position);
        }

        launchAttack = false;
        return stateEnum;
    }
}
