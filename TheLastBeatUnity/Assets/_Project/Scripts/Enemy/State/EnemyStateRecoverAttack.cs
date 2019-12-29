using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateRecoverAttack : EnemyState
{
    float recoverDuration = 0;
    float recoverTimer = 0;

    public EnemyStateRecoverAttack(Enemy newEnemy, float duration) : base(newEnemy)
    {
        stateEnum = EEnemyState.RECOVER_ATTACK;
        recoverDuration = duration;
    }

    public override void Enter()
    {
        enemy.SetStateText("recover");

        //recoverTimer = recoverDuration;
        float timeLeft = (SoundManager.Instance.LastBeat.lastTimeBeat + SoundManager.Instance.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();
        recoverTimer = timeLeft;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        recoverTimer -= deltaTime;    
        if (recoverTimer <= 0)
        {
            if (!enemy.WeaponHitbox.PlayerInHitbox)
                return EEnemyState.CHASE;
            else
                return EEnemyState.PREPARE_ATTACK;
        }

        return stateEnum;
    }
}
