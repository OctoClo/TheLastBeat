using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class RushParams : AbilityParams
{
    public float RushDuration = 0;
    public float distanceAfterDash = 0;
    public float PulseCost = 0;

    [Header("Sound")]
    public AK.Wwise.Event OnBeatSound = null;
    public AK.Wwise.Event OffBeatSound = null;
    public float Cooldown = 0;

    [HideInInspector]
    public BlinkAbility blinkAbility;

    [Header("Rush mark")]
    public GameObject RushMarkPrefab = null;
    public Texture Texture = null;
    public float speedAnimMark = 0;
    public float markPersistDuration = 0;
    public Texture turnVariante1 = null;
    public Texture turnVariante2 = null;

    [Header("Impact")]
    public GameObject frontDash = null;
    public GameObject backDash = null;
    public float intensityScreenShake = 1;
    public float durationScreenShake = 0;
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
        if (SoundManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT))
        {
            parameters.OnBeatSound.Post(player.gameObject);
            player.ModifyPulseValue(-healCorrectBeat);
        }
        else
        {
            //Reset CDA cooldown
            RewindRush.MissInput();
            parameters.OffBeatSound.Post(player.gameObject);
            player.ModifyPulseValue(parameters.PulseCost);
        }

        //To remove after
        CameraManager.Instance.LiveCamera.GetComponent<CameraEffect>().StartScreenShake(parameters.durationScreenShake, parameters.intensityScreenShake);

        parameters.blinkAbility.ResetCooldown();
        currentCooldown = cooldown;
        player.Status.StartDashing();
        player.Anim.SetRushing(true);

        if (RewindRush.IsInCombo)
            CreateTurnMark(player.transform.forward);
        else
            CreateStartMark(player.transform.forward);

        Sequence seq = DOTween.Sequence();
        Vector3 direction = new Vector3(player.CurrentTarget.transform.position.x, player.transform.position.y, player.CurrentTarget.transform.position.z) - player.transform.position;
        GetObstacleOnDash(direction);

        //VFX
        List<Vector3> frontAndBack = SceneHelper.Instance.GetCollisions(player.CurrentTarget.GetComponent<Collider>(), player.transform.position, direction, direction.magnitude);
        if (frontAndBack.Count >= 2)
        {
            GameObject front = GameObject.Instantiate(parameters.frontDash, frontAndBack[0], Quaternion.identity);
            front.transform.forward = -direction;
            GameObject.Destroy(front, 2);

            Sequence seqSpawn = DOTween.Sequence();
            seqSpawn.AppendInterval(0.1f);
            seqSpawn.AppendCallback(() =>
            {
                GameObject back = GameObject.Instantiate(parameters.backDash, frontAndBack[1], Quaternion.identity);
                back.transform.forward = direction;
                GameObject.Destroy(back, 2);
            });
            seqSpawn.Play();
        }

        // Dash towards the target
        if (obstacleAhead)
        {
            direction = new Vector3(obstacle.point.x, player.transform.position.y, obstacle.point.z) - player.transform.position;
        }
        else
        {
            direction += (direction.normalized * parameters.distanceAfterDash);
            player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");
        }

        Vector3 goalPosition = direction + player.transform.position;
        seq.Append(player.transform.DOMove(goalPosition, parameters.RushDuration));

        if (obstacleAhead)
        {
            direction *= -0.5f;
            goalPosition += direction;
            seq.AppendCallback(() => player.Anim.SetRushing(false));
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

            GameObject instanciatedTrail = GameObject.Instantiate(parameters.RushMarkPrefab);
            instanciatedTrail.transform.forward = player.transform.forward;
            instanciatedTrail.transform.position = finalPos;
            Material mat = instanciatedTrail.GetComponent<MeshRenderer>().material;
            mat.SetFloat("_CoeffDissolve", 0);
            mat.SetFloat("_ExtToInt", 0);
            mat.SetTexture("_MainTex", Random.value < 0.5f ? parameters.turnVariante1 : parameters.turnVariante2);
            mat.SetFloat("_BlurRatio", 0.25f);
            mat.SetVector("_CenterUV", new Vector4(0.5f, 0, 0, 0));
            mat.SetVector("_ToBorder", new Vector4(0.5f, 1, 0, 0));
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 1, parameters.speedAnimMark));
            seq.AppendInterval(parameters.markPersistDuration);
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 0, parameters.speedAnimMark));
            seq.AppendCallback(() => GameObject.Destroy(instanciatedTrail));
            seq.Play();
        }
    }

    void CreateTurnMark(Vector3 direction)
    {
        RaycastHit hit;
        //Find nearest ground + can be created on steep
        if (Physics.Raycast(player.CurrentFootOnGround.position, Vector3.down, out hit))
        {
            Vector3 finalPos = hit.point + (hit.normal * 0.001f);

            GameObject instanciatedTrail = GameObject.Instantiate(parameters.RushMarkPrefab);
            instanciatedTrail.transform.localScale *= 0.7f;
            instanciatedTrail.transform.forward = player.transform.forward;
            instanciatedTrail.transform.position = finalPos;
            Material mat = instanciatedTrail.GetComponent<MeshRenderer>().material;
            mat.SetFloat("_CoeffDissolve", 0);
            mat.SetFloat("_ExtToInt", 0);
            mat.SetTexture("_MainTex", parameters.Texture);
            mat.SetFloat("_BlurRatio", 0.25f);
            mat.SetVector("_CenterUV", new Vector4(0.5f, 0, 0, 0));
            mat.SetVector("_ToBorder", new Vector4(0.5f, 1, 0, 0));
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 1, parameters.speedAnimMark));
            seq.AppendInterval(parameters.markPersistDuration);
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 0, parameters.speedAnimMark));
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
        player.Anim.SetRushing(false);

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
