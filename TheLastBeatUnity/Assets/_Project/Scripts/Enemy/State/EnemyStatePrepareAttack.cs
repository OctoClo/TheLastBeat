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
    float animDuration = 0;
    float inDangerSince = 0;

    public EnemyStatePrepareAttack(Enemy newEnemy, float waitBefore, float duration, float danger) : base(newEnemy)
    {
        stateEnum = EEnemyState.PREPARE_ATTACK;
        scaleEndValues = new Vector3(1.5f, 1.5f, 1.5f);
        waitBeforeAnimDuration = waitBefore;
        animDuration = duration;
        inDangerSince = Mathf.Clamp(danger, 0, 1);
    }

    public override void Enter()
    {
        DOTween.Sequence()
            .AppendInterval(animDuration * (1 - inDangerSince))
            .AppendCallback(() => SceneHelper.Instance.mainPlayer.InDanger = true);

        enemy.SetStateText("prepare");

        animationFinished = false;
        animation = enemy.CreateSequence();

        animation.Insert(waitBeforeAnimDuration, enemy.transform.DOShakePosition(animDuration, 0.5f, 100));
        animation.Insert(waitBeforeAnimDuration, enemy.model.transform.DOScale(scaleEndValues, animDuration));
        animation.AppendCallback(() => animationFinished = true);

        animation.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.ATTACK;
        
        return stateEnum;
    }

    public override void Exit()
    {
        SceneHelper.Instance.mainPlayer.InDanger = false;
        enemy.model.transform.localScale = scaleEndValues;
    }
}
