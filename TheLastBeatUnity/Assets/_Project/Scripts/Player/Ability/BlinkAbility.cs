using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BlinkParams : AbilityParams
{
    public float Distance = 0;
    public float TimeToReach = 0;
    public float PulseCost = 0;
    public float timeWait = 0;
    public GameObject prefabMark;
    public GameObject prefabTrail;
    public float SpeedAnimShrink = 0.25f;
    public float marksSpeedAnimation = 0.25f;
    public float markPersist = 0.25f;
    public float rumbleIntensity = 0;
    public float rumbleDuration = 0;
    public GameObject TpVfxPrefab = null;
    public Transform TpVfxParent = null;

    [Header("Sound")]
    public AK.Wwise.Event OnBeatSound = null;
    public AK.Wwise.Event OffBeatSound = null;
}

public class BlinkAbility : Ability
{
    BlinkParams parameters;
    Sequence currentSequence;
    float speed;
    Vector3 trueDirection;
    Vector3 newPosition;

    public BlinkAbility(BlinkParams bp, float healCorrect) : base(bp.AttachedPlayer, healCorrect)
    {
        parameters = bp;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero && currentCooldown == 0 && player.Status.CurrentStatus != EPlayerStatus.BLINKING && player.Status.CurrentStatus != EPlayerStatus.STUNNED)
            Blink();
    }

    public override void ResetCooldown()
    {
        base.ResetCooldown();
        parameters.container.UpdateDelegate(1);
    }

    public override void Update(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(0, currentCooldown - deltaTime);
            parameters.container.UpdateProgression(1 - (currentCooldown / cooldown));
        }
    }

    private void Blink()
    {
        cooldown = SoundManager.Instance.LastBar.beatInterval;
        player.CancelRush();
        parameters.container.UpdateProgression(0);

        // Init
        player.Status.StartBlink();
        CheckRhythm();
        player.ColliderObject.layer = LayerMask.NameToLayer("Player Blinking");

        if (player.InDanger)
        {
            DOTween.Sequence()
                .AppendCallback(() =>
                {
                    foreach (Enemy enn in GameObject.FindObjectsOfType<Enemy>())
                    {
                        enn.Timescale = 0.5f;
                    }
                })
                .AppendInterval(SoundManager.Instance.GetTimeLeftNextBeat())
                .AppendCallback(() =>
                {
                    //SceneHelper.Instance.StartFade(() => { }, 0.2f, Color.clear);
                    foreach (Enemy enn in GameObject.FindObjectsOfType<Enemy>())
                    {
                        enn.Timescale = 1;
                    }
                })
                .Play();
        }

        // Spawn TP VFX
        GameObject vfx = GameObject.Instantiate(parameters.TpVfxPrefab);
        vfx.transform.SetParent(parameters.TpVfxParent);
        vfx.transform.localPosition = Vector3.zero;
        vfx.transform.localRotation = Quaternion.identity;

        // Determine direction
        Vector3 startSize = player.VisualPart.localScale;
        Vector3 direction = player.CurrentDirection;
        direction.Normalize();
        direction = new Vector3(direction.x, 0, direction.z);
        float angle = Vector3.Angle(direction, direction + (Vector3.up * player.CurrentDeltaY));
        direction = Quaternion.AngleAxis(player.CurrentDeltaY < 0 ? angle * 0.9f : angle, Vector3.left) * direction;
        speed = parameters.Distance / parameters.TimeToReach;
        BlinkVFX(direction);
        Vector3 velocity = trueDirection.normalized * speed;

        currentSequence = DOTween.Sequence();
        currentSequence.AppendInterval(parameters.timeWait);
        currentSequence.Append(player.VisualPart.DOScale(Vector3.zero, parameters.SpeedAnimShrink));
        currentSequence.Append(DOTween.To(() => 0, x => player.Rb.velocity = velocity, 1, parameters.TimeToReach));
        currentSequence.AppendCallback(() => player.Rb.velocity = Vector3.zero);
        currentSequence.Append(player.VisualPart.DOScale(startSize, parameters.SpeedAnimShrink));
        currentSequence.AppendCallback(() => player.Status.StopBlink());
        currentSequence.AppendCallback(() => player.ColliderObject.layer = LayerMask.NameToLayer("Default"));
        currentSequence.Play();
    }

    void BlinkVFX(Vector3 direction)
    {
        newPosition = player.transform.position + (direction * parameters.Distance);
        CreateMark(player.transform.position);
        CreateMark(newPosition + (Vector3.up * 0.5f), true);
        CreateTrail(player.transform.position + direction.normalized, newPosition - direction.normalized);
    }

    void CheckRhythm()
    {
        if (SoundManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT))
        {
            if (SoundManager.Instance.IsPerfect(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT))
            {
                PerfectBeat();
            }
            else
            {
                CorrectBeat();
            }

            parameters.OnBeatSound.Post(player.gameObject);
            SceneHelper.Instance.Rumble(parameters.rumbleIntensity, parameters.rumbleDuration);
        }
        else
        {
            if (player.LoseLifeOnAbilities)
                player.ModifyPulseValue(parameters.PulseCost);
            parameters.OffBeatSound.Post(player.gameObject);
            WrongBeat();
            player.DebtRush(parameters.PulseCost);
        }

        currentCooldown = cooldown;
    }

    void CreateMark(Vector3 positionCast, bool computeDirection = false)
    {
        RaycastHit hit;
        //Find nearest ground + can be created on steep
        if (Physics.Raycast(positionCast, Vector3.down * 10, out hit))
        {
            GameObject markInstanciated = GameObject.Instantiate(parameters.prefabMark, hit.point + (hit.normal * 0.1f), Quaternion.identity);
            markInstanciated.GetComponent<SlopeAdaptation>().Adapt();
            Material mat = markInstanciated.GetComponent<MeshRenderer>().material;
            mat.SetFloat("_ExtToInt", 1);
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 0, parameters.marksSpeedAnimation));
            seq.AppendInterval(parameters.markPersist);
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 1, parameters.marksSpeedAnimation));
            seq.AppendCallback(() => GameObject.Destroy(markInstanciated));
            seq.Play();

            if (computeDirection)
                trueDirection = (hit.point + Vector3.up) - player.transform.position;
        }
    }

    void CreateTrail(Vector3 pos1 , Vector3 pos2)
    {
        //Find nearest ground + can be created on steep
        GameObject instanciatedTrail = GameObject.Instantiate(parameters.prefabTrail);
        instanciatedTrail.transform.position = (player.transform.position + (trueDirection * 0.5f)) - Vector3.up * 0.9f;
        instanciatedTrail.transform.forward = trueDirection;

        Material mat = instanciatedTrail.transform.GetChild(0).GetComponent<MeshRenderer>().material;
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => mat.GetFloat("_DistanceX"), x => mat.SetFloat("_DistanceX", x), 1, parameters.marksSpeedAnimation));
        seq.AppendInterval(parameters.markPersist);
        seq.AppendCallback(() => mat.SetFloat("_ReferenceX", 1));
        seq.Append(DOTween.To(() => mat.GetFloat("_DistanceX"), x => mat.SetFloat("_DistanceX", x), 0, parameters.marksSpeedAnimation));
        seq.AppendCallback(() => GameObject.Destroy(instanciatedTrail));
        seq.Play();

        float dist = Vector3.Distance(pos1, pos2);
        Vector3 scale = instanciatedTrail.transform.localScale;
        float nextValue = dist * scale.x / 8.0f;
        instanciatedTrail.transform.GetChild(0).localScale = new Vector3(nextValue, instanciatedTrail.transform.localScale.y * 0.3f, instanciatedTrail.transform.localScale.z * 0.3f);
        instanciatedTrail.transform.GetChild(0).GetComponent<SlopeAdaptation>().Adapt();
    }
}
