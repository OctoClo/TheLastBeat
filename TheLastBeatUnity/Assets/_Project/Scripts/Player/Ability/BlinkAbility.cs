using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[System.Serializable]
public class BlinkParams : AbilityParams
{
    public float Speed = 0;
    public float PulseCost = 0;
    public float Cooldown = 0;
    public AK.Wwise.Event Sound = null;
    public float timeWait = 0;
    public GameObject prefabMark;
}

public class BlinkAbility : Ability
{
    float speed = 5;
    float pulseCost = 0;
    float timeMomentum = 0;
    float currentCooldown = 0;
    float cooldown = 0;
    AK.Wwise.Event soundBlink = null;

    Sequence currentSequence = null;
    GameObject prefabMark;

    public BlinkAbility(BlinkParams bp) : base(bp.AttachedPlayer)
    {
        speed = bp.Speed;
        pulseCost = bp.PulseCost;
        soundBlink = bp.Sound;
        cooldown = bp.Cooldown;
        timeMomentum = bp.timeWait;
        this.prefabMark = bp.prefabMark;
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

        currentSequence = DOTween.Sequence();
        currentSequence.AppendCallback(() =>
        {
            currentCooldown = cooldown;
            player.Status.StartBlink();
            if (BeatManager.Instance.IsInRythm(TimeManager.Instance.SampleCurrentTime(), BeatManager.TypeBeat.BEAT))
            {
                BeatManager.Instance.ValidateLastBeat(BeatManager.TypeBeat.BEAT);
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
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 1, 0.5f));
            seq.AppendInterval(0.5f);
            seq.Append(DOTween.To(() => mat.GetFloat("_CoeffDissolve"), x => mat.SetFloat("_CoeffDissolve", x), 0, 0.5f));
            seq.AppendCallback(() => GameObject.Destroy(markInstanciated));
            seq.Play();
        }

    }

    void MarkAnimation(bool reversed)
    {

    }
}
