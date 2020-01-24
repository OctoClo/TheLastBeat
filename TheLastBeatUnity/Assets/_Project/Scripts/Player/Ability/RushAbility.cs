using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class RushParams : AbilityParams
{
    public float distanceDash = 0;
    public float PulseCost = 0;
    public float damageMultiplier = 1;
    public float freezeFrameBeginDuration = 0.05f;
    public float timeToReachDist = 0.5f;

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
    RushParams parameters;
    Vector3 direction;
    GameObject target;
    float speed = 0;
    Sequence seq;

    float debt = 0;
    bool onRythm = false;
    bool hitShield = false;
    
    public RewindRushAbility RewindRush { get; set; }

    public RushAbility(RushParams rp, float healCorrect) : base(rp.AttachedPlayer, healCorrect)
    {
        parameters = rp;
    }

    public override void Launch()
    {
        if (currentCooldown == 0 && player.CurrentTarget != null && player.Status.CurrentStatus == EPlayerStatus.DEFAULT)
            Rush();
    }

    public override void Update(float deltaTime)
    {
        if (currentCooldown > 0)
            currentCooldown = Mathf.Max(0, currentCooldown - deltaTime);
    }

    void Rush()
    {
        // Init
        debt = 0;
        hitShield = false;
        currentCooldown = cooldown;
        target = player.CurrentTarget.gameObject;
        player.ColliderObject.layer = LayerMask.NameToLayer("Player Dashing");
        player.DelegateColl.OnTriggerEnterDelegate += ImpactEffect;
        parameters.blinkAbility.ResetCooldown();
        player.Status.StartRushing();
        CheckRhythm();

        // Game feel
        SceneHelper.Instance.FreezeFrameTween(parameters.freezeFrameBeginDuration);
        CameraManager.Instance.SetBoolCamera(true, "FOV");
        
        // Determine direction
        direction = new Vector3(target.transform.position.x, player.transform.position.y, target.transform.position.z) - player.transform.position;
        direction = direction.normalized;
        direction = new Vector3(direction.x, player.CurrentDeltaY, direction.z);
        direction *= parameters.distanceDash;
        player.transform.forward = direction;
        RushVFX();

        seq = DOTween.Sequence();
        speed = (direction.magnitude / parameters.timeToReachDist) * 4.0f;
        seq.onUpdate += () => player.Rb.velocity = direction.normalized * speed;
        seq.AppendInterval(parameters.timeToReachDist);
        seq.AppendCallback(() => End());
        seq.Play();
    }

    void RushVFX()
    {
        player.RushParticles.SetActive(true);
        if (RewindRush.IsInCombo)
            CreateTurnMark(player.transform.forward);
        else
            CreateStartMark(player.transform.forward);
    }

    void CheckRhythm()
    {
        onRythm = SoundManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT);
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
            WrongBeat();
            if (player.LoseLifeOnAbilities)
                player.ModifyPulseValue(parameters.PulseCost + debt);
        }
    }

    public void AddDebt(float value)
    {
        if (debt == 0)
            debt += value;
    }

    void ImpactEffect(Collider coll)
    {
        // Found a shield
        if (coll.gameObject.layer == LayerMask.NameToLayer("Stun") && Vector3.Dot(coll.transform.forward, player.transform.forward) < 0)
        {
            if (seq != null)
            {
                seq.Kill();
                hitShield = true;
                player.Status.GetStunned(-direction);
                End();
            }
            return;
        }

        // Found the target
        if (coll && coll.gameObject == target)
        {
            SceneHelper.Instance.FreezeFrameTween(parameters.freezeFrameImpactDuration);

            foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
                ce.StartScreenShake(parameters.durationScreenShake, parameters.intensityScreenShake);

            EnemyHitVFX();
            target.GetComponent<Enemy>().GetAttacked(onRythm);
            
            if (target && RewindRush != null)
                RewindRush.AddChainEnemy(target.GetComponent<Enemy>());

            if (onRythm)
            {
                Sequence seq = DOTween.Sequence();
                seq.AppendCallback(() => coll.gameObject.GetComponent<Enemy>().Timescale = 0.5f)
                    .AppendInterval(0.2f)
                    .AppendCallback(() => coll.gameObject.GetComponent<Enemy>().Timescale = 1);
            }
        }
    }

    void EnemyHitVFX()
    {
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

    public override void End()
    {
        onRythm = false;
        player.RushParticles.SetActive(false);
        player.DelegateColl.OnTriggerEnterDelegate -= ImpactEffect;
        CameraManager.Instance.SetBoolCamera(false, "FOV");
        player.ColliderObject.layer = LayerMask.NameToLayer("Default");
        player.Rb.velocity = Vector3.zero;

        if (!hitShield)
            player.Status.StopRushing();
    }
}
