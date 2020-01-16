using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

struct SequenceAndTarget
{
    public GameObject target;
    public Sequence sequence;
}

public class InputVisualAnimation : Beatable
{
    [SerializeField]
    GameObject prefabAnimation = null;

    [SerializeField]
    GameObject prefabAnimationCorrect = null;

    [SerializeField]
    GameObject prefabAnimationPerfect = null;

    [SerializeField]
    RectTransform rootPerfectGood = null;

    [SerializeField]
    Color wrong = Color.white;

    Queue<SequenceAndTarget> allInstances = new Queue<SequenceAndTarget>();

    public override void Beat()
    {
        GameObject instantiatedPrefab = Instantiate(prefabAnimation,transform);
        RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();

        float timeLeft = (SoundManager.Instance.LastBeat.lastTimeBeat + SoundManager.Instance.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();

        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => 0, x => rect.localScale = new Vector3(x, x, 1), 1.0f, timeLeft).SetEase(Ease.Linear))
           .Append(instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, SoundManager.Instance.Tolerance).SetEase(Ease.Linear))
           .AppendCallback(() =>
           {
               if (allInstances.Count > 0)
                   allInstances.Dequeue();
           })
           .Insert(0, instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.white, timeLeft).SetEase(Ease.Linear))
           .Insert(timeLeft, DOTween.To(() => 1, x => rect.localScale = new Vector3(x, x, 1), 1.4f, SoundManager.Instance.Tolerance).SetEase(Ease.Linear));

        SequenceAndTarget seqTar = new SequenceAndTarget();
        seqTar.sequence = seq;
        seqTar.target = instantiatedPrefab;

        allInstances.Enqueue(seqTar);
    }
    public void CorrectBeat()
    {
        if (allInstances.Count == 0)
            return;

        SequenceAndTarget seqTar = allInstances.Dequeue();
        GameObject gob = seqTar.target;
        Vector3 scale = gob.GetComponent<RectTransform>().localScale;
        Destroy(gob);
        seqTar.sequence.Kill();

        GameObject instanceAnim = Instantiate(prefabAnimationCorrect, rootPerfectGood);
        instanceAnim.GetComponent<RectTransform>().localScale = scale;
        Destroy(instanceAnim, 1);
    }
    public void WrongBeat()
    {
        if (allInstances.Count == 0)
            return;

        SequenceAndTarget seqTar = allInstances.Dequeue();
        GameObject gob = seqTar.target;
        seqTar.sequence.Kill();

        gob.GetComponent<UnityEngine.UI.Image>().color = wrong;
        gob.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, 0.3f);
        Destroy(gob, 1);
    }

    public void PerfectBeat()
    {
        if (allInstances.Count == 0)
            return;

        SequenceAndTarget seqTar = allInstances.Dequeue();
        GameObject gob = seqTar.target;
        Vector3 scale = gob.GetComponent<RectTransform>().localScale;
        Destroy(gob);
        seqTar.sequence.Kill();

        GameObject instanceAnim = Instantiate(prefabAnimationPerfect, rootPerfectGood);
        instanceAnim.GetComponent<RectTransform>().localScale = scale;
        Destroy(instanceAnim, 1);
    }
}
