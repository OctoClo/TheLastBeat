using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateWander : EnemyState
{
    EnemyWanderZone wanderZone = null;
    EnemyDetectionZone detectionZone = null;

    Sequence currentMove = null;
    Vector3 nextPosition = Vector3.zero;

    Vector2 waitDurationMinMax = Vector2.zero;
    float waitTimer = 0;

    bool playerInZone = false;

    public EnemyStateWander(Enemy newEnemy, EnemyWanderZone newWanderZone, EnemyDetectionZone newDetectionZone) : base(newEnemy)
    {
        EventManager.Instance.AddListener<PlayerInEnemyZoneEvent>(OnPlayerInEnemyZoneEvent);

        stateEnum = EEnemyState.WANDER;
        waitDurationMinMax = new Vector2(2, 5);

        wanderZone = newWanderZone;
        detectionZone = newDetectionZone;
    }

    public override void Enter()
    {
        base.Enter();

        currentMove = null;
        waitTimer = 0;
        playerInZone = false;

        StartNewMove();
    }

    void StartNewMove()
    {
        currentMove = DOTween.Sequence();
        wanderZone.RandomPosition(out nextPosition, enemy.transform.position.y);
        currentMove.Append(enemy.transform.DOLookAt(nextPosition, 1, AxisConstraint.Y));
        currentMove.Append(enemy.transform.DOMove(nextPosition, Vector3.Distance(enemy.transform.position, nextPosition) / enemy.Speed));
        currentMove.AppendCallback(() =>
        {
            waitTimer = UnityEngine.Random.Range(waitDurationMinMax.x, waitDurationMinMax.y);
            currentMove = null;
        });
        currentMove.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (waitTimer > 0)
        {
            waitTimer -= deltaTime;

            if (waitTimer <= 0)
            {
                waitTimer = 0;
                StartNewMove();
            }
        }

        if (playerInZone)
            return EEnemyState.CHASE;
        
        return stateEnum;
    }

    private void OnPlayerInEnemyZoneEvent(PlayerInEnemyZoneEvent e)
    {
        if (e.zone == detectionZone)
        {
            if (currentMove != null)
                currentMove.Kill();
            
            playerInZone = true;
        }
    }

    ~EnemyStateWander()
    {
        EventManager.Instance.RemoveListener<PlayerInEnemyZoneEvent>(OnPlayerInEnemyZoneEvent);
    }
}
