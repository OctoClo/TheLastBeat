using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWanderZone : MonoBehaviour
{
    float zoneRadius = 0;
    
    private void Start()
    {
        zoneRadius = GetComponent<SphereCollider>().radius;
    }

    public void RandomPosition(out Vector3 position, float y)
    {
        position = transform.position + UnityEngine.Random.insideUnitSphere * zoneRadius;
        position.y = y;
    }
}
