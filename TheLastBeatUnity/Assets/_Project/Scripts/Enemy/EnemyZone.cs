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
    public Player player = null;
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
        EventManager.Instance.AddListener<EnemyInWanderZone>(OnEnemyInWanderZone);
        EventManager.Instance.AddListener<EnemyOutOfWanderZone>(OnEnemyOutOfWanderZone);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<EnemyDeadEvent>(OnEnemyDeadEvent);
        EventManager.Instance.RemoveListener<EnemyInWanderZone>(OnEnemyInWanderZone);
        EventManager.Instance.RemoveListener<EnemyOutOfWanderZone>(OnEnemyOutOfWanderZone);
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
                if (!SceneHelper.Instance.ZonesChasingPlayer.Contains(this))
                    SceneHelper.Instance.ZonesChasingPlayer.Add(this);
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

        comingBack = true;
        chasing = false;
        if (SceneHelper.Instance.ZonesChasingPlayer.Contains(this))
            SceneHelper.Instance.ZonesChasingPlayer.Remove(this);
    }

    private void TellEnemiesToChaseAgain()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.ChaseAgain = true;
        }

        comingBack = false;
        chasing = true;
        if (!SceneHelper.Instance.ZonesChasingPlayer.Contains(this))
            SceneHelper.Instance.ZonesChasingPlayer.Add(this);
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
            if (enemies.Count == 0)
            {
                if (SceneHelper.Instance.ZonesChasingPlayer.Contains(this))
                    SceneHelper.Instance.ZonesChasingPlayer.Remove(this);
                Destroy(gameObject);
            }
        }
    }

    private void OnEnemyInWanderZone(EnemyInWanderZone e)
    {
        Enemy enemy = enemies.Find(x => x.gameObject == e.enemy);
        if (enemy)
        {
            enemy.InWanderZone = true;
        }
    }

    private void OnEnemyOutOfWanderZone(EnemyOutOfWanderZone e)
    {
        Enemy enemy = enemies.Find(x => x.gameObject == e.enemy);
        if (enemy)
        {
            enemy.InWanderZone = false;
        }
    }
}
