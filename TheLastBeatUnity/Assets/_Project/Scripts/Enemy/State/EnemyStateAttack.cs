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

    public EnemyStateAttack(Enemy newEnemy, float waitBefore, float duration, float impulse) : base(newEnemy)
    {
        stateEnum = EEnemyState.ATTACK;
        scaleEndValues = new Vector3(1, 1, 1);
        waitBeforeAnimDuration = waitBefore;
        animDuration = duration;
        impulseForce = impulse;
        enemy.EnemyKilled += () => { if (animation != null) animation.Kill(); };
    }

    public override void Enter()
    {
        base.Enter();

        animationFinished = false;
        enemy.Agent.enabled = false;

        Vector3 direction = enemy.transform.forward * impulseForce;
        Vector3 goalPos = enemy.transform.position + direction;

        RaycastHit[] hits = Physics.RaycastAll(enemy.transform.position, direction, direction.magnitude, Physics.AllLayers);
        foreach (RaycastHit hit in hits)
        {
            Collider collid;
            if (hit.collider.TryGetComponent<Collider>(out collid) && !collid.isTrigger && !collid.gameObject.CompareTag("Player") && collid.gameObject.layer != LayerMask.NameToLayer("Enemies"))
            {
                Vector3 newGoalPos = hit.point - Vector3.ClampMagnitude(direction, 1);
                goalPos = newGoalPos;
                break;
            }
        }

        animation = enemy.CreateSequence();
        animation.InsertCallback(waitBeforeAnimDuration, () =>
        {
            enemy.StartAttacking();
            enemy.Animator.SetTrigger("attack");
        });
        animation.Insert(waitBeforeAnimDuration, enemy.transform.DOMove(goalPos, animDuration).SetEase(Ease.OutBounce));
        animation.AppendCallback(() => 
        {
            enemy.Animator.SetTrigger("attackEnd");
            animationFinished = true;
        });
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
        SceneHelper.Instance.MainPlayer.InDanger = false;
        enemy.StopAttacking();
    }
}
