using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EnemyStatePrepareAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    Sequence animation = null;
    bool animationFinished = false;
    float waitBeforeAnimDuration = 0;
    int animDurationBeats = 0;
    float animDurationSeconds = 0;

    public EnemyStatePrepareAttack(Enemy newEnemy, float waitBefore, int durationBeats) : base(newEnemy)
    {
        stateEnum = EEnemyState.PREPARE_ATTACK;
        scaleEndValues = new Vector3(1.5f, 1.5f, 1.5f);
        waitBeforeAnimDuration = waitBefore;
        animDurationBeats = durationBeats;
    }

    public override void Enter()
    {
        enemy.SetStateText("prepare");

        animationFinished = false;
        animDurationSeconds = animDurationBeats * SoundManager.Instance.TimePerBeat - 0.2f;
        animation = DOTween.Sequence();

        animation.Insert(waitBeforeAnimDuration, enemy.transform.DOShakePosition(animDurationSeconds, 0.5f, 100));
        animation.Insert(waitBeforeAnimDuration, enemy.model.transform.DOScale(scaleEndValues, animDurationSeconds));
        animation.AppendCallback(() => animationFinished = true);

        animation.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.ATTACK;
        else
        {
            Vector3 lookAt = enemy.Player.transform.position;
            lookAt.y = enemy.transform.position.y;
            enemy.transform.LookAt(lookAt, Vector3.up);
        }
            
        
        return stateEnum;
    }

    public override void Exit()
    {
        enemy.model.transform.localScale = scaleEndValues;
    }
}
