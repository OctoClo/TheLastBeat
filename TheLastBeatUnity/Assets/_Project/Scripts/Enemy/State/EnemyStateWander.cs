using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateWander : EnemyState
{
    Vector3 nextPosition = Vector3.zero;
    Vector2 waitDurationMinMax = Vector2.zero;
    float waitTimer = 0;

    public EnemyStateWander(Enemy newEnemy, Vector2 waitMinMax) : base(newEnemy)
    {
        stateEnum = EEnemyState.WANDER;
        waitDurationMinMax = waitMinMax;
    }

    public override void Enter()
    {
        base.Enter();

        waitTimer = 0;
        nextPosition = enemy.transform.position;
    }

    void StartNewMove()
    {
        enemy.WanderZone.GetRandomPosition(out nextPosition, enemy.transform.position.y);
        enemy.Agent.SetDestination(nextPosition);
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (enemy.ChaseAgain)
        {
            enemy.ChaseAgain = false;
            return EEnemyState.CHASE;
        }
        
        if (waitTimer > 0)
        {
            waitTimer -= deltaTime;

            if (waitTimer <= 0)
                StartNewMove();
        }

        if (waitTimer <= 0 && Vector3.Distance(enemy.transform.position, nextPosition) < 2.5f)
        {
            enemy.Agent.ResetPath();
            waitTimer = RandomHelper.GetRandom(waitDurationMinMax.x, waitDurationMinMax.y);
        }

        if (enemy.DetectionZone.PlayerInZone)
        {
            return EEnemyState.CHASE;
        }
        
        return stateEnum;
    }
}
