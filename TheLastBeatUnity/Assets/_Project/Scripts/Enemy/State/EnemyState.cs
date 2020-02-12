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
    COME_BACK,
    STUN,
    EXPLODE,
    REWIND
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
    }

    public virtual EEnemyState UpdateState(float deltaTime)
    {
        return stateEnum;
    }

    public virtual void OnBeat() {}

    public virtual void OnBar() {}

    public virtual void Exit()
    {
        enemy.Agent.ResetPath();
    }
}
