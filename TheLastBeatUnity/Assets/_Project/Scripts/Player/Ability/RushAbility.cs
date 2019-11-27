using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class RushParams : AbilityParams
{
    public float RushDuration = 0;
    public float PulsationCost = 0;
    public AK.Wwise.Event OnBeatSound = null;
    public AK.Wwise.Event OffBeatSound = null;
}

public class RushAbility : Ability
{
    float duration = 0;
    float pulsationCost;

    Enemy target = null;
    bool obstacleAhead = false;
    RaycastHit obstacle;
    AK.Wwise.Event soundOffBeat = null;
    AK.Wwise.Event soundOnBeat = null;
    public RewindRushAbility RewindRush { get; set; }

    public RushAbility(RushParams rp) : base(rp.AttachedPlayer)
    {
        duration = rp.RushDuration;
        pulsationCost = rp.PulsationCost;
        soundOffBeat = rp.OffBeatSound;
        soundOnBeat = rp.OnBeatSound;
    }

    public override void Launch()
    {
        target = player.GetCurrentTarget();
        if (target)
            Rush();
    }

    void Rush()
    {
        if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime() , BeatManager.TypeBeat.BEAT))
        {
            BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
            soundOnBeat.Post(player.gameObject);
        }
        else
        {
            soundOffBeat.Post(player.gameObject);
        }


        player.Status.StartDashing();
        player.Anim.LaunchAnim(EPlayerAnim.RUSHING);

        Sequence seq = DOTween.Sequence();
        Vector3 direction = new Vector3(target.transform.position.x, player.transform.position.y, target.transform.position.z) - player.transform.position;
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
        seq.AppendCallback(() => player.Health.ModifyPulseValue(pulsationCost));
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
            target.GetAttacked();
            if (RewindRush != null)
            {
                RewindRush.AddChainEnemy(target);
            }
            player.ColliderObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}
