using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateChase : EnemyState
{
    public EnemyStateChase(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.CHASE;
    }
}
