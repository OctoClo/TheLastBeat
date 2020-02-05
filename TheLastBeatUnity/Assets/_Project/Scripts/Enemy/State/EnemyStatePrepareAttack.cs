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
    float waitAfterAnimDuration = 0;
    float inDangerSince = 0;

    float waitBeforeCallSound = 0;
    AK.Wwise.Event callAttackEvent = null;

    public EnemyStatePrepareAttack(Enemy newEnemy, float waitBefore, int durationBeats, float waitAfter, float danger) : base(newEnemy)
    {
        stateEnum = EEnemyState.PREPARE_ATTACK;
        scaleEndValues = new Vector3(1.5f, 1.5f, 1.5f);
        waitBeforeAnimDuration = waitBefore;
        animDurationBeats = durationBeats;
        waitAfterAnimDuration = waitAfter;
        inDangerSince = Mathf.Clamp(danger, 0, 1);

        enemy.EnemyKilled += () => { if (animation != null) animation.Kill(); };
    }

    public override void Enter()
    {
        DOTween.Sequence()
            .AppendInterval(animDurationSeconds * (1 - inDangerSince))
            .AppendCallback(() => SceneHelper.Instance.MainPlayer.InDanger = true);

        animationFinished = false;
        animDurationSeconds = animDurationBeats * SoundManager.Instance.TimePerBeat - waitAfterAnimDuration;

        animation = enemy.CreateSequence();

        animation.Insert(waitBeforeAnimDuration, enemy.transform.DOShakePosition(animDurationSeconds, 0.5f, 100));
        animation.InsertCallback(waitBeforeAnimDuration, () => enemy.Animator.SetTrigger("prepareAttack"));
        animation.InsertCallback(waitBeforeAnimDuration + animDurationSeconds, () => enemy.Animator.SetTrigger("prepareAttackPose"));
        animation.InsertCallback(waitBeforeAnimDuration + animDurationSeconds + waitAfterAnimDuration, () => animationFinished = true);

        animation.Play();
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.ATTACK;
        
        enemy.LookAtPlayer(deltaTime);
        return stateEnum;
    }

    public override void Exit()
    {
        SceneHelper.Instance.MainPlayer.InDanger = false;
    }
}
