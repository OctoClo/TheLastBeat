using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;
using DG.Tweening;

public class CombatArea : MonoBehaviour
{
    [SerializeField]
    CinemachineTargetGroup groupTarget = null;

    [SerializeField]
    float maxWeight = 0;

    [SerializeField]
    float timeTransition = 0;

    [SerializeField]
    AK.Wwise.State inCombat = null;

    [SerializeField]
    AK.Wwise.State outCombat = null;

    int nbTargetsAtThisMoment = 0;
    bool pipelineLock = false;

    Dictionary<Transform, Sequence> runningSequences = new Dictionary<Transform, Sequence>();

    void SetWeight(float weight, Transform trsf)
    {
        groupTarget.RemoveMember(trsf);
        groupTarget.AddMember(trsf, weight, 8);
    }

    float GetWeight(Transform trsf, float defaultValue)
    {
        List<CinemachineTargetGroup.Target> allTarg = groupTarget.m_Targets.Where(x => x.target == trsf).ToList();
        if (allTarg.Count > 0)
        {
            return allTarg[0].weight;
        }
        return defaultValue;
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Enemy"))
        {
            //Can only have one sequence at the same time
            if (runningSequences.ContainsKey(coll.transform) && runningSequences[coll.transform] != null)
            {
                runningSequences[coll.transform].Kill();
                runningSequences[coll.transform] = null;
            }
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                groupTarget.AddMember(coll.transform, 0, 8);
                if (!pipelineLock)
                {
                    nbTargetsAtThisMoment = groupTarget.m_Targets.Count();
                    pipelineLock = true;
                }
            });
            seq.Append(DOTween.To(() => GetWeight(coll.transform, 0), x => SetWeight(x, coll.transform), maxWeight, timeTransition));
            seq.AppendCallback(() =>
            {
                if (nbTargetsAtThisMoment == 2)
                {
                    EnterCombat();
                }
                pipelineLock = false;
            });
            seq.Play();
            runningSequences[coll.transform] = seq;
        }
    }

    void OnTriggerExit(Collider coll)
    {
        if (coll.CompareTag("Enemy"))
        {
            if (runningSequences.ContainsKey(coll.transform) && runningSequences[coll.transform] != null)
            {
                runningSequences[coll.transform].Kill();
                runningSequences[coll.transform] = null;
            }
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => GetWeight(coll.transform, maxWeight), x => SetWeight(x, coll.transform), 0, timeTransition));
            seq.AppendCallback(() => runningSequences.Remove(coll.transform));
            seq.AppendCallback(() => groupTarget.RemoveMember(coll.transform));
            seq.AppendCallback(() => CheckGroupTargetEmpty());
            seq.Play();
        }
    }

    public void CheckGroupTargetEmpty()
    {
        if (groupTarget.m_Targets.Count() == 1)
        {
            ExitCombat();
        }
    }

    void EnterCombat()
    {
        inCombat.SetValue();
    }

    void ExitCombat()
    {
        outCombat.SetValue();
    }
}
