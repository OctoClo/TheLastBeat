using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    [SerializeField]
    Transform player = null;
    
    List<Enemy> enemies = new List<Enemy>();
    EnemyWanderZone wanderZone = null;
    EnemyDetectionZone detectionZone = null;

    private void Start()
    {
        wanderZone = GetComponentInChildren<EnemyWanderZone>();
        detectionZone = GetComponentInChildren<EnemyDetectionZone>();

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

    private void OnEnemyDeadEvent(EnemyDeadEvent e)
    {
        if (enemies.Contains(e.enemy))
        {
            enemies.Remove(e.enemy);
        }
    }
}
