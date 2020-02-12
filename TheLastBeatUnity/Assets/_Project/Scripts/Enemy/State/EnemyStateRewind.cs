using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateRewind : EnemyState
{
    public EnemyStateRewind(Enemy newEnemy) : base(newEnemy)
    {
        stateEnum = EEnemyState.REWIND;
    }
}
