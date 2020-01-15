using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyExplosive : Enemy
{
    [TabGroup("Behaviour")] [Header("Explode")] [SerializeField]
    float waitBeforeExplosionAnim = 0;
    [TabGroup("Behaviour")] [SerializeField]
    float explosionBlastForce = 0;
    [TabGroup("Behaviour")] [SerializeField]
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
