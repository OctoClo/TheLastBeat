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
    public float damageMultiplier = 1;
    public float freezeFrameBeginDuration = 0.05f;

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
    public float rumbleIntensity = 0;
    public float rumbleDuration = 0;
    public float freezeFrameImpactDuration = 0.05f;
}

public class RushAbility : Ability
{
    bool obstacleAhead = false;
    RaycastHit obstacle;
    RushParams parameters;
    Vector3 direction;
    GameObject target;

    float debt;
    bool onRythm = false;
    
    public RewindRushAbility RewindRush { get; set; }

    public RushAbility(RushParams rp, float healCorrect) : base(rp.AttachedPlayer, healCorrect)
    {
        parameters = rp;
    }

    public void AddDebt(float value)
    {
        if (debt == 0)
            debt += value;
    }

    public override void Launch()
    {
        if (currentCooldown == 0 && player.CurrentTarget != null)
        {
            SceneHelper.Instance.FreezeFrameTween(parameters.freezeFrameBeginDuration);
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

    void ImpactEffect(Collider coll)
    {
        if (coll && coll.gameObject == target)
        {
            SceneHelper.Instance.FreezeFrameTween(parameters.freezeFrameImpactDuration);

            foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
            {
                ce.StartScreenShake(parameters.durationScreenShake, parameters.intensityScreenShake);
            }

            target.GetComponent<Enemy>().GetAttacked(onRythm);
            if (!target)
                return;

            if (RewindRush != null)
            {
                RewindRush.AddChainEnemy(target.GetComponent<Enemy>());
            }

            //VFX
            List<Vector3> frontAndBack = SceneHelper.Instance.RayCastBackAndForth(target.GetComponent<Collider>(), player.transform.position, direction, direction.magnitude);
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
        }
    }

    void Rush()
    {
        CameraManager.Instance.SetBoolCamera(true, "FOV");
        target = player.CurrentTarget.gameObject;
        onRythm = SoundManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT);
        player.RushParticles.SetActive(true);
        bool perfect = SoundManager.Instance.IsPerfect(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT);
        if (onRythm)
        {
            parameters.OnBeatSound.Post(player.gameObject);
            SceneHelper.Instance.Rumble(parameters.rumbleIntensity, parameters.rumbleDuration);
            if (perfect)
            {
                player.ModifyPulseValue(-parameters.HealPerCorrectBeat);
                PerfectBeat();
            }
            else
                CorrectBeat();
        }
        else
        {
            //Reset CDA cooldown
            RewindRush.MissInput();
            parameters.OffBeatSound.Post(player.gameObject);
            player.ModifyPulseValue(parameters.PulseCost + debt);
            WrongBeat();
            if (player.LoseLifeOnAbilities)
                player.ModifyPulseValue(parameters.PulseCost);
        }

        debt = 0;
        parameters.blinkAbility.ResetCooldown();
        currentCooldown = cooldown;
        player.Status.StartRushing();
        direction = new Vector3(target.transform.position.x, player.transform.position.y, target.transform.position.z) - player.transform.position;
        player.transform.forward = direction;
        player.DelegateColl.OnTriggerEnterDelegate += ImpactEffect;

        if (RewindRush.IsInCombo)
            CreateTurnMark(player.transform.forward);
        else
            CreateStartMark(player.transform.forward);

        Sequence seq = DOTween.Sequence();
        GetObstacleOnDash(direction);

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

            GameObject instanciatedTrail = GameObject.Instantiate(parameters.RushMarkPrefab, finalPos, Quaternion.identity);
            instanciatedTrail.transform.forward = player.transform.forward;
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

            GameObject instanciatedTrail = GameObject.Instantiate(parameters.RushMarkPrefab, finalPos , Quaternion.identity);
            instanciatedTrail.transform.localScale *= 0.7f;
            instanciatedTrail.transform.forward = player.transform.forward;
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
        onRythm = false;
        player.RushParticles.SetActive(false);
        player.DelegateColl.OnTriggerEnterDelegate -= ImpactEffect;
        CameraManager.Instance.SetBoolCamera(false, "FOV");

        if (obstacleAhead && obstacle.collider != null && obstacle.collider.gameObject.layer == LayerMask.NameToLayer("Stun"))
        {
            player.Status.GetStunned();
        }
        else
        {
            player.ColliderObject.layer = LayerMask.NameToLayer("Default");
            player.Status.StopRushing();
        }
    }
}
