using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class RushParams : AbilityParams
{
    public float RushDuration = 0;
    public float PulseCost = 0;
    public AK.Wwise.Event OnBeatSound = null;
    public AK.Wwise.Event OffBeatSound = null;
    public float Cooldown = 0;
}

public class RushAbility : Ability
{
    float duration = 0;
    float pulseCost = 0;
    float cooldown = 0;
    float currentCooldown = 0;

    bool obstacleAhead = false;
    bool attackOnRythm = false;
    RaycastHit obstacle;

    AK.Wwise.Event soundOffBeat = null;
    AK.Wwise.Event soundOnBeat = null;
    
    public RewindRushAbility RewindRush { get; set; }

    public RushAbility(RushParams rp) : base(rp.AttachedPlayer)
    {
        duration = rp.RushDuration;
        pulseCost = rp.PulseCost;
        soundOffBeat = rp.OffBeatSound;
        soundOnBeat = rp.OnBeatSound;
        cooldown = rp.Cooldown;
    }

    public override void Launch()
    {
        if (!player.Status.Dashing && currentCooldown == 0 && player.CurrentTarget != null)
        {
            Rush();
        }
    }

    public override void Update(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(0, currentCooldown - deltaTime);
        }
    }

    void Rush()
    {
        attackOnRythm = BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT);

        if (attackOnRythm)
        {
            soundOnBeat.Post(player.gameObject);
        }
        //Only cost if off-rythm
        else
        {

            soundOffBeat.Post(player.gameObject);
            if (player.Health.InCriticMode)
            {
                player.Die();
            }
            player.Health.ModifyPulseValue(pulseCost);
        }

        currentCooldown = cooldown;
        player.Status.StartDashing();
        player.Anim.LaunchAnim(EPlayerAnim.RUSHING);

        Sequence seq = DOTween.Sequence();
        Vector3 direction = new Vector3(player.CurrentTarget.transform.position.x, player.transform.position.y, player.CurrentTarget.transform.position.z) - player.transform.position;
        GetObstacleOnDash(direction);

        // Dash towards the target
        if (obstacleAhead)
        {
            direction = new Vector3(obstacle.point.x, player.transform.position.y, obstacle.point.z) - player.transform.position;
        }
        else
        {
            direction *= 1.3f;
            player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");
        }

        Vector3 goalPosition = direction + player.transform.position;
        seq.Append(player.transform.DOMove(goalPosition, duration));

        if (obstacleAhead)
        {
            direction *= -0.5f;
            goalPosition += direction;
            seq.Append(player.transform.DOMove(goalPosition, duration / 2.0f));
        }

        seq.AppendCallback(() => End());
        seq.Play();
    }

    void GetObstacleOnDash(Vector3 direction)
    {
        RaycastHit[] hits = Physics.RaycastAll(player.transform.position, direction, direction.magnitude);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemies") && !hit.collider.isTrigger)
            {
                obstacleAhead = true;
                obstacle = hit;
                return;
            }
        }

        obstacleAhead = false;
    }

    public override void End()
    {
        player.Status.StopDashing();

        if (obstacleAhead && obstacle.collider.gameObject.layer == LayerMask.NameToLayer("Stun"))
            player.Status.Stun();
        else
        {
            player.CurrentTarget.GetAttacked(attackOnRythm);
            if (RewindRush != null)
            {
                RewindRush.AddChainEnemy(player.CurrentTarget);
            }
            player.ColliderObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
