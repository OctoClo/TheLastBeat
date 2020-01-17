using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BlinkParams : AbilityParams
{
    public float Speed = 0;
    public float PulseCost = 0;
    public float timeWait = 0;
    public GameObject prefabMark;
    public GameObject prefabTrail;
    public float SpeedAnimShrink = 0.25f;
    public float marksSpeedAnimation = 0.25f;
    public float markPersist = 0.25f;
    public float rumbleIntensity = 0;
    public float rumbleDuration = 0;

    [Header("Sound")]
    public AK.Wwise.Event OnBeatSound = null;
    public AK.Wwise.Event OffBeatSound = null;
}

public class BlinkAbility : Ability
{
    BlinkParams parameters;
    Sequence currentSequence;

    public BlinkAbility(BlinkParams bp) : base(bp.AttachedPlayer)
    {
        parameters = bp;
    }

    public override void Launch()
    {
        if (player.CurrentDirection != Vector3.zero && currentCooldown == 0)
            Blink();
    }

    public override void Update(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown = Mathf.Max(0, currentCooldown - deltaTime);
        }
    }

    private void Blink()
    {
        Vector3 startSize = player.VisualPart.localScale;
        Vector3 direction = player.CurrentDirection;
        direction.Normalize();
        Vector3 newPosition = player.transform.position + direction * parameters.Speed;

        CreateMark(player.transform.position);
        CreateMark(newPosition);
        CreateTrail(player.transform.position + (direction.normalized), newPosition - direction.normalized);

        currentSequence = DOTween.Sequence();
        currentSequence.AppendCallback(() =>
        {
            player.Status.StartBlink();
            if (SoundManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT))
            {
                if (SoundManager.Instance.IsPerfect(TimeManager.Instance.SampleCurrentTime(), SoundManager.TypeBeat.BEAT))
                {
                    player.ModifyPulseValue(-healCorrectBeat);
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
                currentCooldown = SoundManager.Instance.TimePerBar;
            }
        });
        currentSequence.AppendInterval(parameters.timeWait);
        currentSequence.Append(player.VisualPart.DOScale(Vector3.zero, parameters.SpeedAnimShrink));
        currentSequence.Append(player.transform.DOMove(newPosition, 0.2f));
        currentSequence.Append(player.VisualPart.DOScale(startSize, parameters.SpeedAnimShrink));
        currentSequence.AppendCallback(() =>
        {
            player.Status.StopBlink();
        });
        currentSequence.Play();
    }

    void CreateMark(Vector3 positionCast)
    {
        RaycastHit hit;
        //Find nearest ground + can be created on steep
        if (Physics.Raycast(positionCast, Vector3.down, out hit))
        {
            GameObject markInstanciated = GameObject.Instantiate(parameters.prefabMark);
            markInstanciated.transform.position = hit.point + (hit.normal * 0.1f);
            markInstanciated.transform.up = hit.normal;
            Material mat = markInstanciated.GetComponent<MeshRenderer>().material;
            mat.SetFloat("_ExtToInt", 1);
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 0, parameters.marksSpeedAnimation));
            seq.AppendInterval(parameters.markPersist);
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 1, parameters.marksSpeedAnimation));
            seq.AppendCallback(() => GameObject.Destroy(markInstanciated));
            seq.Play();
        }
    }

    void CreateTrail(Vector3 pos1 , Vector3 pos2)
    {
        RaycastHit hit;
        RaycastHit hit2;
        float length = Vector3.Distance(pos1, pos2);
        //Find nearest ground + can be created on steep
        if (Physics.Raycast(pos1, Vector3.down, out hit) && Physics.Raycast(pos2, Vector3.down, out hit2))
        {
            pos1 = hit.point + (hit.normal * 0.001f);
            pos2 = hit2.point + (hit2.normal * 0.001f);

            GameObject instanciatedTrail = GameObject.Instantiate(parameters.prefabTrail);
            instanciatedTrail.transform.position = (pos1 + pos2) / 2.0f;
            instanciatedTrail.transform.right = pos1 - pos2;

            Material mat = instanciatedTrail.GetComponent<MeshRenderer>().material;
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
            instanciatedTrail.transform.localScale = new Vector3(nextValue, instanciatedTrail.transform.localScale.y, instanciatedTrail.transform.localScale.z);
        }
    }
}
