using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EEnemyType
{
    STATIONARY,
    DEFAULT
}

enum ECheckPlayerPosition
{
    TOO_CLOSE,
    TOO_FAR
}

public class EnemyZone : MonoBehaviour
{
    [SerializeField]
    EEnemyType enemiesType = EEnemyType.DEFAULT;
    [SerializeField]
    Player player = null;
    [SerializeField]
    float maxDistanceChase = 6;
    float sqrMaxDistanceChase = 0;
    
    List<Enemy> enemies = new List<Enemy>();
    EnemyWanderZone wanderZone = null;
    EnemyDetectionZone detectionZone = null;

    bool chasing = false;
    bool comingBack = false;

    private void Start()
    {
        wanderZone = GetComponentInChildren<EnemyWanderZone>();
        detectionZone = GetComponentInChildren<EnemyDetectionZone>();
        sqrMaxDistanceChase = maxDistanceChase * maxDistanceChase;

        GetComponentsInChildren(false, enemies);
        foreach (Enemy enemy in enemies)
        {
            enemy.ZoneInitialize(enemiesType, wanderZone, detectionZone, player);
        }
    }

    private void OnEnable()
    {
        EventManager.Instance.AddListener<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<EnemyDeadEvent>(OnEnemyDeadEvent);
    }

    private void Update()
    {
        if (comingBack)
        {
            if (IsEveryoneBack())
            {
                comingBack = false;
            }
            
            if (CheckPlayerPosition(ECheckPlayerPosition.TOO_CLOSE))
            {
                TellEnemiesToChaseAgain();
            }
        }
        else if (chasing)
        {
            if (CheckPlayerPosition(ECheckPlayerPosition.TOO_FAR) && !detectionZone.PlayerInZone)
            {
                CallEnemiesBack();
            }
        }
        else
        {
            if (enemies.Count > 0 && enemies[0].CurrentStateEnum == EEnemyState.CHASE && detectionZone.PlayerInZone)
            {
                chasing = true;
            }
        }
    }

    private bool CheckPlayerPosition(ECheckPlayerPosition type)
    {
        float currentDistance = 0;
        float minDistance = 9999;

        foreach (Enemy enemy in enemies)
        {
            currentDistance = (enemy.transform.position - player.transform.position).sqrMagnitude;

            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
            }
        }

        if (type == ECheckPlayerPosition.TOO_CLOSE && PlayerTooClose(minDistance))
            return true;

        if (type == ECheckPlayerPosition.TOO_FAR && PlayerTooFar(minDistance))
            return true;

        return false;
    }

    private bool PlayerTooClose(float distance)
    {
        return (distance < sqrMaxDistanceChase);
    }

    private bool PlayerTooFar(float distance)
    {
        return (distance > sqrMaxDistanceChase);
    }

    private void CallEnemiesBack()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.ComeBack = true;
        }

        chasing = false;
        comingBack = true;
    }

    private void TellEnemiesToChaseAgain()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.ChaseAgain = true;
        }

        comingBack = false;
        chasing = true;
    }

    private bool IsEveryoneBack()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.CurrentStateEnum != EEnemyState.WANDER)
            {
                return false;
            }
        }

        return true;
    }

    private void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (enemies.Contains(e.enemy))
        {
            enemies.Remove(e.enemy);
        }
    }
}
