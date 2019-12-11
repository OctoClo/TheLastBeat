﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BlinkParams : AbilityParams
{
    public float Speed = 0;
    public float PulseCost = 0;
    public AK.Wwise.Event Sound = null;
    public float timeWait = 0;
    public GameObject prefabMark;
    public GameObject prefabTrail;
}

public class BlinkAbility : Ability
{
    float speed = 5;
    float pulseCost = 0;
    float timeMomentum = 0;
    AK.Wwise.Event soundBlink = null;

    Sequence currentSequence = null;
    GameObject prefabMark;
    GameObject prefabTrail;

    public BlinkAbility(BlinkParams bp) : base(bp.AttachedPlayer)
    {
        speed = bp.Speed;
        pulseCost = bp.PulseCost;
        soundBlink = bp.Sound;

        //Because we can't know the delta time at first beat
        cooldown = 3.6f;
        timeMomentum = bp.timeWait;
        this.prefabMark = bp.prefabMark;
        healCorrectBeat = bp.HealPerCorrectBeat;
        prefabTrail = bp.prefabTrail;
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
        Vector3 newPosition = player.transform.position + direction * speed;

        CreateMark(player.transform.position);
        CreateMark(newPosition);
        CreateTrail(player.transform.position + (direction.normalized), newPosition - direction.normalized);

        currentSequence = DOTween.Sequence();
        currentSequence.AppendCallback(() =>
        {
            currentCooldown = SoundManager.Instance.TimePerBar;
            player.Status.StartBlink();
            if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
            {
                BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
                player.Health.ModifyPulseValue(-healCorrectBeat);
            }
            else
            {
                if (player.Health.InCriticMode)
                {
                    player.Die();
                }
                player.Health.ModifyPulseValue(pulseCost);
            }
            soundBlink.Post(player.gameObject);
        });
        currentSequence.AppendInterval(timeMomentum);
        currentSequence.Append(player.VisualPart.DOScale(Vector3.zero, 0.05f));
        currentSequence.Append(player.transform.DOMove(newPosition, 0.2f));
        currentSequence.Append(player.VisualPart.DOScale(startSize, 0.05f));
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
        if (Physics.Raycast(positionCast , Vector3.down, out hit))
        {
            GameObject markInstanciated = GameObject.Instantiate(prefabMark);
            markInstanciated.transform.position = hit.point + (hit.normal * 0.1f);
            markInstanciated.transform.up = hit.normal;
            Material mat = markInstanciated.GetComponent<MeshRenderer>().material;
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 0, 0.25f));
            seq.AppendInterval(0.25f);
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 1, 0.25f));
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

            GameObject instanciatedTrail = GameObject.Instantiate(prefabTrail);
            instanciatedTrail.transform.position = (pos1 + pos2) / 2.0f;
            instanciatedTrail.transform.right = pos1 - pos2;

            Material mat = instanciatedTrail.GetComponent<MeshRenderer>().material;
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => mat.GetFloat("_DistanceX"), x => mat.SetFloat("_DistanceX", x), 1, 0.25f));
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() => mat.SetFloat("_ReferenceX", 1));
            seq.Append(DOTween.To(() => mat.GetFloat("_DistanceX"), x => mat.SetFloat("_DistanceX", x), 0, 0.25f));
            seq.AppendCallback(() => GameObject.Destroy(instanciatedTrail));
            seq.Play();

            float dist = Vector3.Distance(pos1, pos2);
            Vector3 scale = instanciatedTrail.transform.localScale;
            float nextValue = dist * scale.x / 8.0f;
            instanciatedTrail.transform.localScale = new Vector3(nextValue, instanciatedTrail.transform.localScale.y, instanciatedTrail.transform.localScale.z);
        }

    }

    void MarkAnimation(bool reversed)
    {

    }
}
