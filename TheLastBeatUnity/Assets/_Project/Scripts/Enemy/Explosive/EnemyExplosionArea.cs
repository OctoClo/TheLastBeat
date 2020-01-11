using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosionArea : MonoBehaviour
{
    [HideInInspector]
    public bool ExplosionFinished = false;

    float colliderRadius = 0;
    float blastForce = 0;
    Rigidbody rb = null;

    private void Start()
    {
        colliderRadius = GetComponent<SphereCollider>().radius;
    }

    public void Explode(float force, int damage, Player player)
    {
        blastForce = force;

        Collider[] overlapColliders = Physics.OverlapSphere(transform.position, colliderRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        foreach (Collider collid in overlapColliders)
        {
            if (collid.gameObject.CompareTag("Enemy"))
            {
                PushRigidbody(collid);
            }

            if (collid.gameObject.CompareTag("Player"))
            {
                PushRigidbody(collid);
                player.ModifyPulseValue(damage, true);
            }
        }

        ExplosionFinished = true;
    }

    private void PushRigidbody(Collider collid)
    {
        rb = collid.GetComponentInChildren<Rigidbody>();
        if (rb)
            rb.AddExplosionForce(blastForce, transform.position, 0, 0, ForceMode.VelocityChange);
    }
}
