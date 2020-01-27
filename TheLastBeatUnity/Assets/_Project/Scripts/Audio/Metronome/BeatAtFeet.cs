using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BeatAtFeet : Beatable
{
    [SerializeField]
    Transform rootParent = null;

    [SerializeField]
    GameObject prefab = null;

    [SerializeField]
    Vector3 finalSize = Vector3.one;

    [SerializeField]
    AnimationCurve curve = null;

    [SerializeField]
    Color goodInput = Color.white;

    [SerializeField]
    Color wrongInput = Color.white;

    [SerializeField]
    Color perfectInput = Color.white;

    [SerializeField]
    GameObject perfectPrefab = null;

    [SerializeField]
    float speedFadeWhite = 0;

    [SerializeField]
    GameObject goodPrefab = null;

    Queue<SequenceAndTarget> allInstances = new Queue<SequenceAndTarget>();

    public override void Beat()
    {
        SoundManager sm = SoundManager.Instance;
        float timeLeft = (sm.LastBeat.lastTimeBeat + sm.LastBeat.beatInterval) - TimeManager.Instance.SampleCurrentTime();
        timeLeft += sm.LastBeat.beatInterval;
        GameObject instantiated = Instantiate(prefab, rootParent);
        instantiated.transform.localPosition = Vector3.up * 0.01f;
        instantiated.transform.localScale = Vector3.one * 0.1f;
        Color col = instantiated.GetComponent<MeshRenderer>().material.color;
        col.a = 0.3f;
        instantiated.GetComponent<MeshRenderer>().material.color = col;
        Sequence seq = DOTween.Sequence()
            .Append(instantiated.transform.DOScale(finalSize, timeLeft).SetEase(curve))
            .Insert(0, DOTween.To(() => instantiated.GetComponent<MeshRenderer>().material.color, x =>
            {
                instantiated.GetComponent<MeshRenderer>().material.color += new Color(speedFadeWhite, speedFadeWhite, speedFadeWhite, 0) * Time.deltaTime;
                Color colorTemp = instantiated.GetComponent<MeshRenderer>().material.color;
                instantiated.GetComponent<MeshRenderer>().material.color = new Color(colorTemp.r, colorTemp.g, colorTemp.b, x.a);
            }, Color.white, timeLeft))
            .Append(DOTween.To(() => instantiated.GetComponent<MeshRenderer>().material.color, x => instantiated.GetComponent<MeshRenderer>().material.color = x, Color.clear, 0.2f))
            .AppendCallback(() => Destroy(instantiated))
            .AppendCallback(() =>
            {
                SequenceAndTarget seqTar = allInstances.Dequeue();
                Destroy(seqTar.target);
                seqTar.sequence.Kill();
            });

        SequenceAndTarget seqAndTar = new SequenceAndTarget();
        seqAndTar.target = instantiated;
        seqAndTar.sequence = seq;
        allInstances.Enqueue(seqAndTar);
    }

    public void CorrectInput()
    {
        CircleDisappear(goodInput);
        GameObject instantiated = Instantiate(goodPrefab, transform);
        instantiated.transform.localPosition = Vector3.up * 0.45f;
        Destroy(instantiated, 2);
    }

    public void PerfectInput()
    {
        CircleDisappear(perfectInput);
        GameObject instantiated = Instantiate(perfectPrefab, transform);
        instantiated.transform.localScale = Vector3.one * 5.5f;
        instantiated.transform.localPosition = Vector3.up * 0.45f;
        Destroy(instantiated, 2);
    }

    public void WrongInput()
    {
        CircleDisappear(wrongInput);
    }

    void CircleDisappear(Color col)
    {
        SequenceAndTarget seqTar = allInstances.Peek();
        Color tempColor = new Color(col.r, col.g, col.b, 0);
        seqTar.target.GetComponent<MeshRenderer>().material.color = col;
    }
}
