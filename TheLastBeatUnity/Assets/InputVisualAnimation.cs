using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InputVisualAnimation : Beatable
{
    [SerializeField]
    GameObject prefabAnimation;

    [SerializeField]
    GameObject prefabAnimationCorrect;

    [SerializeField]
    GameObject prefabAnimationPerfect;

    [SerializeField]
    Color wrong;

    Sequence seq;

    Queue<GameObject> allInstances = new Queue<GameObject>();

    public override void Beat()
    {
        GameObject instantiatedPrefab = Instantiate(prefabAnimation,transform);
        RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();

        allInstances.Enqueue(instantiatedPrefab);

        float timeLeft = (SoundManager.Instance.LastBeat.lastTimeBeat + SoundManager.Instance.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();

        seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => 0, x => rect.localScale = new Vector3(x, x, 1), 1.0f, timeLeft).SetEase(Ease.Linear))
           .Append(instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, SoundManager.Instance.Tolerance).SetEase(Ease.Linear))
           .AppendCallback(() =>
           {
               if (allInstances.Count > 0)
                   allInstances.Dequeue();
           })
           .Insert(0, instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.white, timeLeft).SetEase(Ease.Linear))
           .Insert(timeLeft, DOTween.To(() => 1, x => rect.localScale = new Vector3(x, x, 1), 1.4f, SoundManager.Instance.Tolerance).SetEase(Ease.Linear));
    }
    public void CorrectBeat()
    {
        if (allInstances.Count == 0)
            return;

        GameObject gob = allInstances.Dequeue();
        Vector3 scale = gob.GetComponent<RectTransform>().localScale;
        Destroy(gob);
        seq.Kill();

        GameObject instanceAnim = Instantiate(prefabAnimationCorrect, transform);
        instanceAnim.GetComponent<RectTransform>().localScale = scale;
        Destroy(instanceAnim, 1);
    }
    public void WrongBeat()
    {
        if (allInstances.Count == 0)
            return;

        GameObject gob = allInstances.Dequeue();
        Vector3 scale = gob.GetComponent<RectTransform>().localScale;
        seq.Kill();
        gob.GetComponent<UnityEngine.UI.Image>().color = wrong;
        gob.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, 0.3f);
        Destroy(gob, 1);
    }

    public void PerfectBeat()
    {
        if (allInstances.Count == 0)
            return;

        GameObject gob = allInstances.Dequeue();
        Vector3 scale = gob.GetComponent<RectTransform>().localScale;
        Destroy(gob);
        seq.Kill();

        GameObject instanceAnim = Instantiate(prefabAnimationPerfect, transform);
        instanceAnim.GetComponent<RectTransform>().localScale = scale;
        Destroy(instanceAnim, 1);
    }
}
