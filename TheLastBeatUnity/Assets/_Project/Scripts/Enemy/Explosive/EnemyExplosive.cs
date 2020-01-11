using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class EnemyExplosive : Enemy
{
    [TabGroup("References")] [SerializeField]
    EnemyExplosionArea explosionArea = null;

    protected override void CreateStates()
    {
        states.Add(EEnemyState.WANDER, new EnemyStateWander(this, waitBeforeNextMove));
        states.Add(EEnemyState.CHASE, new EnemyStateChase(this));
        states.Add(EEnemyState.PREPARE_ATTACK, new EnemyStatePrepareAttack(this, waitBeforePrepareAnim, prepareAnimDuration));
        states.Add(EEnemyState.ATTACK, new EnemyStateExplode(this, waitBeforeAttackAnim, attackForce, explosionArea));
        states.Add(EEnemyState.RECOVER_ATTACK, new EnemyStateRecoverAttack(this, recoverAnimDuration));
        states.Add(EEnemyState.COME_BACK, new EnemyStateComeBack(this));
        states.Add(EEnemyState.STUN, new EnemyStateStun(this));
    }
}
