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
    int beatCounter = 0;

    public EnemyStatePrepareAttack(Enemy newEnemy, float waitBefore, int duration) : base(newEnemy)
    {
        stateEnum = EEnemyState.PREPARE_ATTACK;
        scaleEndValues = new Vector3(1.5f, 1.5f, 1.5f);
        waitBeforeAnimDuration = waitBefore;
        animDurationBeats = duration;
        animDurationSeconds = animDurationBeats * 0.78f;
    }

    public override void Enter()
    {
        enemy.SetStateText("prepare");

        beatCounter = 0;
        animationFinished = false;
        animation = DOTween.Sequence();

        animation.Insert(waitBeforeAnimDuration, enemy.transform.DOShakePosition(animDurationSeconds, 0.5f, 100));
        animation.Insert(waitBeforeAnimDuration, enemy.model.transform.DOScale(scaleEndValues, animDurationSeconds));

        animation.Play();
    }

    public override void OnBeat()
    {
        beatCounter++;
        if (beatCounter == animDurationBeats)
            animationFinished = true;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.ATTACK;
        else
            enemy.transform.LookAt(enemy.Player.transform, Vector3.up);
        
        return stateEnum;
    }

    public override void Exit()
    {
        enemy.model.transform.localScale = scaleEndValues;
    }
}
