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
        float timeLeft = sm.LastBeat.beatInterval * 2.0f;
        GameObject instantiated = Instantiate(prefab, rootParent);
        instantiated.transform.localPosition = Vector3.zero;
        Sequence seq = DOTween.Sequence()
            .Append(instantiated.transform.DOScale(finalSize, timeLeft)).SetEase(Ease.Linear)
            .Append(DOTween.To(() => instantiated.GetComponent<MeshRenderer>().material.color, x => instantiated.GetComponent<MeshRenderer>().material.color = x, Color.clear, 0.2f).SetEase(Ease.Linear))
            .Insert(0, DOTween.To(() => instantiated.GetComponent<MeshRenderer>().material.color, x => instantiated.GetComponent<MeshRenderer>().material.color = x, Color.white, timeLeft).SetEase(Ease.Linear))
            .AppendCallback(() => Destroy(instantiated));
    }
}
