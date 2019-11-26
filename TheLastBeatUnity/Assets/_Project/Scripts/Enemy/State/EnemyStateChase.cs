using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateChase : EnemyState
{
    Transform player = null;
    Rigidbody rb = null;
    bool playerInHitbox = false;

    public EnemyStateChase(Enemy newEnemy, Transform newPlayer) : base(newEnemy)
    {
        player = newPlayer;
        rb = enemy.GetComponent<Rigidbody>();
        stateEnum = EEnemyState.CHASE;
    }

    public override void Enter()
    {
        base.Enter();

        playerInHitbox = false;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        enemy.transform.DOLookAt(player.position, 1, AxisConstraint.Y);

        if (playerInHitbox)
            return EEnemyState.PREPARE_ATTACK;
        
        return stateEnum;
    }

    public override void FixedUpdateState()
    {
        Vector3 movement = (player.position - enemy.transform.position);
        movement.y = 0;

        if (!enemy.WeaponHitbox.PlayerInHitbox)
        {
            movement.Normalize();
            rb.velocity = movement * enemy.Speed;
        }
        else
        {
            playerInHitbox = true;
        }
    }

    public override void Exit()
    {
        rb.velocity = Vector3.zero;
    }
}
