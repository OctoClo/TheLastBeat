using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInEnemyZoneEvent : GameEvent { public EnemyDetectionZone zone; }

public class EnemyDetectionZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.Instance.Raise(new PlayerInEnemyZoneEvent() { zone = this });
        }
    }
}
