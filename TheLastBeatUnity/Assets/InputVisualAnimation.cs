using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class InputVisualAnimation : Beatable
{
    [SerializeField]
    GameObject prefabAnimation;
    Sequence seq;
    Vector3 actualSize;

    public override void Beat()
    {
        GameObject instantiatedPrefab = Instantiate(prefabAnimation,transform);
        RectTransform rect = instantiatedPrefab.GetComponent<RectTransform>();

        float timeLeft = (SoundManager.Instance.LastBeat.lastTimeBeat + SoundManager.Instance.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();

        seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => 0, x => rect.localScale = new Vector3(x, x, 1), 1.0f, timeLeft).SetEase(Ease.Linear))
           .AppendCallback(() => actualSize = Vector3.zero)
           .Append(instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.clear, SoundManager.Instance.Tolerance).SetEase(Ease.Linear))
           .Insert(0,instantiatedPrefab.GetComponent<UnityEngine.UI.Image>().DOColor(Color.white, timeLeft).SetEase(Ease.Linear))
           .onComplete += () => Destroy(instantiatedPrefab);
        seq.onKill += () => Destroy(instantiatedPrefab);
    }

    public void CorrectBeat()
    {
        seq.Kill();
    }
}
