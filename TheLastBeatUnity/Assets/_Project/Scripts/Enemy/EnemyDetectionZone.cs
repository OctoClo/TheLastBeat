using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInEnemyZoneEvent : GameEvent { public EnemyDetectionZone zone; }

public class EnemyDetectionZone : MonoBehaviour
{
    public GameObject Player { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player == null)
                Player = other.gameObject;

            EventManager.Instance.Raise(new PlayerInEnemyZoneEvent() { zone = this });
        }
    }
}
