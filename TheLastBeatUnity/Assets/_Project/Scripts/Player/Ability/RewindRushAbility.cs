using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class RewindRushParameters : AbilityParams
{
    public float Duration = 0;
    public float PulseCost = 0;
    public float Cooldown = 0;
    public float MaxTimeBeforeResetMarks = 0;
    public AK.Wwise.State RewindState = null;
    public AK.Wwise.State NormalState = null;
    public int MaxChained = 5;
    public float screenShakeDuration = 0;
    public float screenShakeIntensity = 0;
    public float rumbleIntensity = 0;
    public float rumbleDuration = 0;
}

public class RewindRushAbility : Ability
{
    int missedInput = 0;
    Queue<Enemy> chainedEnemies = new Queue<Enemy>();
    public bool IsInCombo => chainedEnemies.Count > 0;
    float rushChainTimer = 0;
    bool attackOnRythm = false;
    RewindRushParameters parameters;

    public RewindRushAbility(RewindRushParameters rrp) : base(rrp.AttachedPlayer)
    {
        parameters = rrp;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (chainedEnemies.Count > 0 && !player.Status.Dashing && currentCooldown == 0)
        {
            rushChainTimer -= Time.deltaTime;

            if (rushChainTimer < 0)
                ResetCombo();
        }

        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(currentCooldown - deltaTime, 0.0f);
        }
    }

    public void AddChainEnemy(Enemy enn)
    {
        rushChainTimer = parameters.MaxTimeBeforeResetMarks;
        CameraManager.Instance.SetBoolCamera(true, "FOV");

        if (chainedEnemies.Count >= parameters.MaxChained)
        {
            chainedEnemies.Dequeue();
        }
        chainedEnemies.Enqueue(enn);
    }

    public void MissInput()
    {
        missedInput++;
        if (missedInput >= 2)
        {
            ResetCombo();
        }
    }

    public void ResetCombo()
    {
        chainedEnemies.Clear();
        missedInput = 0;
        CameraManager.Instance.SetBoolCamera(false, "FOV");
    }

    public override void Launch()
    {
        if (chainedEnemies.Count > 0 && currentCooldown == 0)
            RewindRush();
    }

    void RewindRush()
    {
        currentCooldown = cooldown;
        parameters.RewindState.SetValue();
        player.Status.StartDashing();
        player.FocusZone.overrideControl = true;
        player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");

        attackOnRythm = SoundManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT);
        if (attackOnRythm)
        {
            player.ModifyPulseValue(-healCorrectBeat);
            SceneHelper.Instance.Rumble(parameters.rumbleIntensity, parameters.rumbleDuration);
        }
        else
        {
            player.ModifyPulseValue(parameters.PulseCost);
        }

        Sequence seq = DOTween.Sequence();
        Vector3 direction;
        Vector3 goalPosition = player.transform.position;

        foreach (Enemy enemy in chainedEnemies.Reverse())
        {
            if (enemy)
            {
                direction = new Vector3(enemy.transform.position.x, goalPosition.y, enemy.transform.position.z) - goalPosition;
                direction *= 1.3f;

                goalPosition += direction;
                seq.AppendCallback(() =>
                {
                    player.FocusZone.OverrideCurrentEnemy(enemy);
                    player.LookAtCurrentTarget();
                    player.Anim.SetRushing(true);
                    SceneHelper.Instance.FreezeFrame(0.1f);
                });
                seq.Append(player.transform.DOMove(goalPosition, parameters.Duration));
                seq.AppendCallback(() => { enemy.GetAttacked(attackOnRythm); });
            }
        }

        seq.AppendCallback(() => End());
        seq.Play();
    }

    public override void End()
    {
        CameraManager.Instance.LiveCamera.GetComponent<CameraEffect>().StartScreenShake(parameters.screenShakeDuration, parameters.screenShakeIntensity);
        player.Anim.SetRushing(false);
        player.Status.StopDashing();
        player.FocusZone.overrideControl = false;
        player.gameObject.layer = LayerMask.NameToLayer("Default");
        ResetCombo();
        parameters.NormalState.SetValue();
    }
}
