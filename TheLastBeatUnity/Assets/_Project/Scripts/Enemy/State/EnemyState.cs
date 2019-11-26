using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EEnemyState
{
    WANDER,
    CHASE,
    PREPARE_ATTACK,
    ATTACK,
    RECOVER_ATTACK,
    COME_BACK
}

public abstract class EnemyState
{
    protected Enemy enemy;
    protected EEnemyState stateEnum;

    public EnemyState(Enemy newEnemy)
    {
        enemy = newEnemy;
    }

    public virtual void Enter()
    {
        enemy.SetStateText(stateEnum.ToString().ToLower());
    }

    public virtual void FixedUpdateState() {}

    public virtual EEnemyState UpdateState(float deltaTime)
    {
        return stateEnum;
    }

    public virtual void Exit() {}
}
