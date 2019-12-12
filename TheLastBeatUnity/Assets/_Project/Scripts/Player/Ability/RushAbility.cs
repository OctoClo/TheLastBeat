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

    [HideInInspector]
    public BlinkAbility blinkAbility;
    public GameObject RushMark;
    public Texture Texture;
}

public class RushAbility : Ability
{
    bool obstacleAhead = false;
    bool attackOnRythm = false;
    RaycastHit obstacle;
    RushParams parameters;
    
    public RewindRushAbility RewindRush { get; set; }

    public RushAbility(RushParams rp) : base(rp.AttachedPlayer)
    {
        parameters = rp;
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
        parameters.blinkAbility.ResetCooldown();
        currentCooldown = cooldown;
        player.Status.StartDashing();
        player.Anim.LaunchAnim(EPlayerAnim.RUSHING);
        CreateStartMark(player.transform.forward);

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
        seq.Append(player.transform.DOMove(goalPosition, parameters.RushDuration));

        if (obstacleAhead)
        {
            direction *= -0.5f;
            goalPosition += direction;
            seq.Append(player.transform.DOMove(goalPosition, parameters.RushDuration / 2.0f));
        }

        seq.AppendCallback(() => End());
        seq.Play();
    }

    void CreateStartMark(Vector3 direction)
    {
        RaycastHit hit;
        //Find nearest ground + can be created on steep
        if (Physics.Raycast(player.CurrentFootOnGround.position, Vector3.down, out hit))
        {
            Vector3 finalPos = hit.point + (hit.normal * 0.001f);

            GameObject instanciatedTrail = GameObject.Instantiate(parameters.RushMark);
            instanciatedTrail.transform.forward = player.transform.forward;
            instanciatedTrail.transform.position = finalPos;
            Material mat = instanciatedTrail.GetComponent<MeshRenderer>().material;
            mat.SetFloat("_SwitchY", 1);
            mat.SetTexture("_MainTex", parameters.Texture);
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => mat.GetFloat("_DistanceX"), x => mat.SetFloat("_DistanceX", x), 1, 0.25f));
            seq.AppendInterval(0.25f);
            seq.AppendCallback(() => mat.SetFloat("_ReferenceX", 1));
            seq.Append(DOTween.To(() => mat.GetFloat("_DistanceX"), x => mat.SetFloat("_DistanceX", x), 0, 0.25f));
            seq.AppendCallback(() => GameObject.Destroy(instanciatedTrail));
            seq.Play();
        }
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

        if (attackOnRythm)
        {
            parameters.OnBeatSound.Post(player.gameObject);
            player.ModifyPulseValue(-healCorrectBeat);
        }
        else
        {
            RewindRush.MissInput();
            parameters.OffBeatSound.Post(player.gameObject);
            player.ModifyPulseValue(parameters.PulseCost);
        }
    }
}
