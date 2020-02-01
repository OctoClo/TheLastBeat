using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosionArea : MonoBehaviour
{
    [HideInInspector]
    public bool ExplosionFinished = false;

    Enemy myself = null;
    float colliderRadius = 0;
    float blastForce = 0;
    Rigidbody rb = null;

    private void Start()
    {
        myself = GetComponentInParent<Enemy>();
        colliderRadius = GetComponent<SphereCollider>().radius;
    }

    public void Explode(float force, int damageToPlayer, int damageToEnemies, Player player)
    {
        blastForce = force;
        Enemy enemy = null;

        Collider[] overlapColliders = Physics.OverlapSphere(transform.position, colliderRadius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        foreach (Collider collid in overlapColliders)
        {
            if (collid.gameObject.CompareTag("Enemy"))
            {
                if (collid.TryGetComponent<Enemy>(out enemy) && enemy != myself)
                {
                    enemy.GetPushedBack();
                    PushRigidbody(collid);
                    enemy.GetAttacked(false, damageToEnemies);
                }
            }

            if (collid.gameObject.CompareTag("Player"))
            {
                PushRigidbody(collid);
                player.ModifyPulseValue(damageToPlayer, true);
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
