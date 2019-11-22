using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RushAbility : Ability
{
    float duration = 0;

    float zoomDuration = 0;
    float zoomValue = 0;

    float slowMoDuration = 0;
    float slowMoTimer = 0;

    float impactBeatDelay = 0;

    float pulsationCost;

    Enemy target = null;
    bool obstacleAhead = false;
    RaycastHit obstacle;
    AK.Wwise.Event soundOffBeat = null;
    AK.Wwise.Event soundOnBeat = null;

    public RushAbility(Player newPlayer, float rushDuration, float newZoomDuration, float newZoomValue,
                    float newSlowMoDuration, float newImpactBeatDelay, float newCost, AK.Wwise.Event onBeat , AK.Wwise.Event offBeat) : base(newPlayer)
    {
        duration = rushDuration;
        impactBeatDelay = newImpactBeatDelay;
        zoomDuration = newZoomDuration;
        zoomValue = newZoomValue;
        slowMoDuration = newSlowMoDuration;
        pulsationCost = newCost;
        soundOffBeat = offBeat;
        soundOnBeat = onBeat;
    }

    public override void Launch()
    {
        target = player.GetCurrentTarget();
        if (target)
            Rush();
    }

    public override void Update(float deltaTime)
    {
        if (slowMoTimer > 0)
        {
            slowMoTimer -= deltaTime;

            if (slowMoTimer < 0)
            {
                slowMoTimer = 0;
            }
        }
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
                Debug.Log(hit.collider.gameObject.name, hit.collider.gameObject);
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
            player.AddChainedEnemy(target);
            player.ColliderObject.layer = LayerMask.NameToLayer("Default");
            slowMoTimer = slowMoDuration;
        }
    }
}
