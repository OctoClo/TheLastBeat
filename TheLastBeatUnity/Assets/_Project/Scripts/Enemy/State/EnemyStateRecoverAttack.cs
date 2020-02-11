using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStateRecoverAttack : EnemyState
{
    float recoverDuration = 0;
    float recoverTimer = 0;
    Sequence animation = null;
    float defaultAgentRadius = 0;

    public EnemyStateRecoverAttack(Enemy newEnemy, float duration) : base(newEnemy)
    {
        stateEnum = EEnemyState.RECOVER_ATTACK;
        recoverDuration = duration;
        defaultAgentRadius = enemy.Agent.radius;
        enemy.EnemyKilled += () => { if (animation != null) animation.Kill(); };
    }

    public override void Enter()
    {
        recoverTimer = recoverDuration;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        recoverTimer -= deltaTime;

        if (recoverTimer <= 0)
            return EEnemyState.CHASE;
        
        return stateEnum;
    }

    public override void Exit()
    {
        enemy.Agent.radius = 0.1f;
        enemy.Agent.enabled = true;
        DOTween.To(() => enemy.Agent.radius, x => enemy.Agent.radius = x, defaultAgentRadius, 0.8f);
    }
}
