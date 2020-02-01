using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyExplosive : Enemy
{
    [TabGroup("Behaviour")] [Header("Explode")] [SerializeField] [Tooltip("How fast the enemy will follow the player before exploding")]
    float followSpeed = 0;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How many beats the enemy will wait before exploding")]
    int beatsBeforeExplosion = 0;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How much the explosion will push the player / enemies away if hit")]
    float explosionBlastForce = 0;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How many HP the player will lose if hit")]
    int explosionDamageToPlayer = 0;
    [TabGroup("Behaviour")] [SerializeField] [Tooltip("How many HP the enemies will lose if hit")]
    int explosionDamageToEnemies = 0;
    [TabGroup("Behaviour")] [SerializeField]
    AK.Wwise.Event explosionEvent = null;


    [TabGroup("References")] [SerializeField]
    EnemyExplosionArea explosionArea = null;
    [TabGroup("References")] [SerializeField]
    GameObject explosionPrefab = null;

    protected override void Awake()
    {
        base.Awake();
        minLives = 1;
    }

    protected override void CreateStates()
    {
        base.CreateStates();
        states.Add(EEnemyState.EXPLODE, new EnemyStateExplode(this, beatsBeforeExplosion, chaseDistance, followSpeed, explosionBlastForce, explosionDamageToPlayer, explosionDamageToEnemies, explosionPrefab, explosionArea, explosionEvent));
    }

    public override void StartDying()
    {
        ChangeState(EEnemyState.EXPLODE);
    }
}
