using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosionArea : MonoBehaviour
{
    [HideInInspector]
    public bool ExplosionFinished = false;

    float colliderRadius = 0;

    private void Start()
    {
        colliderRadius = GetComponent<SphereCollider>().radius;
    }

    public void Explode(float blastForce)
    {
        Rigidbody rb = null;

        Collider[] overlapColliders = Physics.OverlapSphere(transform.position, colliderRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        foreach (Collider collid in overlapColliders)
        {
            if (collid.gameObject.CompareTag("Enemy") || collid.gameObject.CompareTag("Player"))
            {
                rb = collid.GetComponentInChildren<Rigidbody>();
                if (rb)
                {
                    Debug.Log("Pushing this rigidbody", rb.gameObject);
                    rb.AddExplosionForce(blastForce, transform.position, 0, 0, ForceMode.VelocityChange);
                }
            }
        }

        ExplosionFinished = true;
        Debug.Log("Explosion over");
    }
}
