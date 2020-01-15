using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyExplosive : Enemy
{
    [TabGroup("Behaviour")] [Header("Explode")] [SerializeField] [Tooltip("How much time the enemy will wait before exploding")]
    float waitBeforeExplosionAnim = 0;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How much the explosion will push the player / enemies away if hit")]
    float explosionBlastForce = 0;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How many HP the player will lose if hit")]
    int explosionDamage = 0;

    [TabGroup("References")] [SerializeField]
    EnemyExplosionArea explosionArea = null;
    [TabGroup("References")] [SerializeField]
    GameObject explosionPrefab = null;

    protected override void CreateStates()
    {
        base.CreateStates();
        states.Add(EEnemyState.EXPLODE, new EnemyStateExplode(this, waitBeforeExplosionAnim, explosionBlastForce, explosionDamage, explosionPrefab, explosionArea));
    }

    public override void StartDying()
    {
        ChangeState(EEnemyState.EXPLODE);
    }
}
