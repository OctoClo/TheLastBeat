using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BeatAtFeet : Beatable
{
    [SerializeField]
    Transform rootParent;

    [SerializeField]
    GameObject prefab;

    [SerializeField]
    Vector3 finalSize;

    public override void Beat()
    {
        SoundManager sm = SoundManager.Instance;
        //float timeLeft = ((sm.LastBeat.lastTimeBeat + sm.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime()) + sm.LastBeat.beatInterval;
        float timeLeft = (SoundManager.Instance.LastBeat.lastTimeBeat + SoundManager.Instance.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();
        Debug.Log(timeLeft);
        GameObject instantiated = Instantiate(prefab, rootParent);
        instantiated.transform.localPosition = Vector3.zero;
        Sequence seq = DOTween.Sequence()
            .Append(instantiated.transform.DOScale(finalSize, timeLeft)).SetEase(Ease.Linear)
            .Insert(0, DOTween.To(() => instantiated.GetComponent<MeshRenderer>().material.color, x => instantiated.GetComponent<MeshRenderer>().material.color = x, Color.white, timeLeft).SetEase(Ease.Linear))
            .AppendCallback(() => Destroy(instantiated));
    }
}
