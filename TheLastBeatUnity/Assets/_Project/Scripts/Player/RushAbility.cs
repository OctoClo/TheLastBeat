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

    Enemy target = null;

    public RushAbility(Player newPlayer, float rushDuration, float newZoomDuration, float newZoomValue,
                    float newSlowMoDuration, float newImpactBeatDelay) : base(newPlayer)
    {
        duration = rushDuration;
        impactBeatDelay = newImpactBeatDelay;
        zoomDuration = newZoomDuration;
        zoomValue = newZoomValue;
        slowMoDuration = newSlowMoDuration;
    }

    public override void Launch()
    {
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
                TimeManager.Instance.SetTimeScale(1);
                //cameraEffect.StartZoom(-zoomValue, zoomDuration, CameraEffect.ZoomType.Distance, CameraEffect.ValueType.Absolute);
            }
        }
    }

    void Rush()
    {
        player.Status.StartDashing();
        //health.NewAction(1.5f, impactBeatDelay);
        //cameraEffect.StartZoom(zoomValue, zoomDuration, CameraEffect.ZoomType.Distance, CameraEffect.ValueType.Absolute);
        TimeManager.Instance.SlowEnemies();

        Sequence seq = DOTween.Sequence();

        target = player.GetCurrentTarget();
        Vector3 direction = new Vector3(target.transform.position.x, player.transform.position.y, target.transform.position.z) - player.transform.position;

        RaycastHit hit = GetObstacleOnDash(direction);

        // Dash towards the target
        if (hit.collider)
            direction = new Vector3(hit.point.x, player.transform.position.y, hit.point.z) - player.transform.position;
        else
        {
            direction *= 1.3f;
            player.gameObject.layer = LayerMask.NameToLayer("Player Dashing");
        }

        Vector3 goalPosition = direction + player.transform.position;
        seq.Append(player.transform.DOMove(goalPosition, duration));

        if (hit.collider)
        {
            direction *= -0.5f;
            goalPosition += direction;
            seq.Append(player.transform.DOMove(goalPosition, duration / 2.0f));
        }

        seq.AppendCallback(() => EndRush(hit));
        seq.Play();
    }

    void EndRush(RaycastHit hit)
    {
        player.Status.StopDashing();
        TimeManager.Instance.ResetEnemies();

        if (hit.collider)
            player.Status.Stun();
        else
        {
            target.GetAttacked();
            player.AddChainedEnemy(target);
            player.gameObject.layer = LayerMask.NameToLayer("Default");
            //TimeManager.Instance.SetTimeScale(0.1f);
            slowMoTimer = slowMoDuration;
        }
    }

    RaycastHit GetObstacleOnDash(Vector3 direction)
    {
        RaycastHit[] hits = Physics.RaycastAll(player.transform.position, direction, direction.magnitude);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Enemies"))
                return hit;
        }

        return new RaycastHit();
    }
}
