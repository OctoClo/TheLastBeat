using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    [SerializeField]
    Transform player = null;
    [SerializeField]
    float maxDistanceChase = 6;
    float sqrMaxDistanceChase = 0;
    
    List<Enemy> enemies = new List<Enemy>();
    EnemyWanderZone wanderZone = null;
    EnemyDetectionZone detectionZone = null;

    bool chasing = false;

    private void Start()
    {
        wanderZone = GetComponentInChildren<EnemyWanderZone>();
        detectionZone = GetComponentInChildren<EnemyDetectionZone>();
        sqrMaxDistanceChase = maxDistanceChase * maxDistanceChase;

        GetComponentsInChildren(false, enemies);
        foreach (Enemy enemy in enemies)
        {
            enemy.ZoneInitialize(wanderZone, detectionZone, player);
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
        if (!chasing)
        {
            if (detectionZone.PlayerInZone)
            {
                chasing = true;
            }
        }        
        else
        {
            if (PlayerTooFar() && !detectionZone.PlayerInZone)
            {
                CallEnemiesBack();
            }
        }
    }

    private bool PlayerTooFar()
    {
        float currentDistance = 0;

        foreach (Enemy enemy in enemies)
        {
            currentDistance = (enemy.transform.position - player.position).sqrMagnitude;

            if (currentDistance >= sqrMaxDistanceChase)
            {
                return true;
            }
        }

        return false;
    }

    private void CallEnemiesBack()
    {
        Debug.Log("Back to me!");
        
        foreach (Enemy enemy in enemies)
        {
            enemy.ComeBack = true;
        }

        chasing = false;
    }

    private void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (enemies.Contains(e.enemy))
        {
            enemies.Remove(e.enemy);
        }
    }
}
