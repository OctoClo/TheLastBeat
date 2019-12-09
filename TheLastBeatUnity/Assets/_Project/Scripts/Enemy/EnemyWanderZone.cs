using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBackInWanderZone : GameEvent { public EnemyWanderZone wanderZone = null; public GameObject enemy = null; }

public class EnemyWanderZone : MonoBehaviour
{
    float zoneRadius = 0;
    
    private void Start()
    {
        zoneRadius = GetComponent<SphereCollider>().radius;
    }

    public void GetRandomPosition(out Vector3 position, float y)
    {
        position = transform.position + UnityEngine.Random.insideUnitSphere * zoneRadius;
        position.y = y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EventManager.Instance.Raise(new EnemyBackInWanderZone() { wanderZone = this, enemy = other.gameObject });
        }
    }
}
