using UnityEngine;
using DG.Tweening;

public class EnemyStateAttack : EnemyState
{
    Vector3 scaleEndValues = Vector3.zero;
    Sequence animation = null;
    bool animationFinished = false;
    float waitBeforeAnimDuration = 0;
    float animDuration = 0;
    float impulseForce = 0;
    AK.Wwise.Event callAttackEvent = null;

    public EnemyStateAttack(Enemy newEnemy, float waitBefore, float duration, float impulse, AK.Wwise.Event newCallAttackEvent) : base(newEnemy)
    {
        stateEnum = EEnemyState.ATTACK;
        scaleEndValues = new Vector3(1, 1, 1);
        waitBeforeAnimDuration = waitBefore;
        animDuration = duration;
        impulseForce = impulse;
        callAttackEvent = newCallAttackEvent;
    }

    public override void Enter()
    {
        base.Enter();

        animationFinished = false;
        callAttackEvent.Post(enemy.gameObject);
        
        Vector3 goalPos = enemy.transform.forward * impulseForce;
        goalPos += enemy.transform.position;

        animation = enemy.CreateSequence();

        animation.Insert(waitBeforeAnimDuration, enemy.model.transform.DOScale(scaleEndValues, animDuration).SetEase(Ease.OutBounce));
        animation.Insert(waitBeforeAnimDuration, enemy.transform.DOMove(goalPos, animDuration).SetEase(Ease.OutBounce));
        animation.InsertCallback(waitBeforeAnimDuration, () => enemy.StartAttacking());
        animation.AppendCallback(() => animationFinished = true);

        animation.Play();

        SceneHelper.Instance.MainPlayer.InDanger = true;
    }

    public override EEnemyState UpdateState(float deltaTime)
    {
        if (animationFinished)
            return EEnemyState.RECOVER_ATTACK;

        return stateEnum;
    }

    public override void Exit()
    {
        enemy.model.transform.localScale = scaleEndValues;
        SceneHelper.Instance.MainPlayer.InDanger = false;
        enemy.StopAttacking();
    }
}
