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

    Sequence seq;

    GameObject instanceFront = null;
    GameObject instanceBack = null;

    public override void Beat()
    {
        GameObject instantiatedPrefab = Instantiate(prefabAnimation,transform);
        RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();

        if (instanceFront == null)
        {
            instanceFront = instantiatedPrefab;
        }
        else
        {
            instanceBack = instantiatedPrefab;
        }

        float timeLeft = (SoundManager.Instance.LastBeat.lastTimeBeat + SoundManager.Instance.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();

        seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => 0, x => rect.localScale = new Vector3(x, x, 1), 1.0f, timeLeft).SetEase(Ease.Linear))
           .Append(instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, SoundManager.Instance.Tolerance).SetEase(Ease.Linear))
           .Insert(0,instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.white, timeLeft).SetEase(Ease.Linear))
           .Insert(timeLeft, DOTween.To(() => 1, x => rect.localScale = new Vector3(x, x, 1), 1.4f, SoundManager.Instance.Tolerance).SetEase(Ease.Linear))
           .onComplete += () => Purge(instantiatedPrefab);
        seq.onKill += () => Purge(instantiatedPrefab);
    }

    void Purge(GameObject obj)
    {
        Destroy(obj);
        instanceFront = instanceBack;
        instanceBack = null;
    }

    public void CorrectBeat()
    {
        seq.Kill();
        Vector3 scale = instanceFront.GetComponent<RectTransform>().localScale;
        Purge(instanceFront);

        GameObject instanceAnim = Instantiate(prefabAnimationCorrect, transform);
        instanceAnim.GetComponent<RectTransform>().localScale = scale;
    }
}
