using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class BeatAtFeet : Beatable
{
    [SerializeField]
    Transform rootParent = null;

    [SerializeField]
    GameObject arrow = null;

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

    bool mustBeDisplayed = false;

    Queue<SequenceAndTarget> allInstances = new Queue<SequenceAndTarget>();
    Sequence transitionSequence = null;

    [SerializeField]
    List<Texture> animationTexture = new List<Texture>();

    [SerializeField]
    Color colorMetronome = Color.black;

    protected override void Start()
    {
        base.Start();
        CombatStatusChange(false);
        SceneHelper.Instance.OnCombatStatusChange += CombatStatusChange;
    }

    private void OnDisable()
    {
        SceneHelper.Instance.OnCombatStatusChange -= CombatStatusChange;
    }

    public override void Beat()
    {
        if (!mustBeDisplayed)
            return;
        SoundManager sm = SoundManager.Instance;
        float timeLeft = sm.GetTimeLeftNextBeat();
        GameObject instantiated = Instantiate(prefab, rootParent);
        instantiated.transform.localPosition = Vector3.up * 0.01f;
        instantiated.transform.localScale = Vector3.one * 0.1f;
        Color col = instantiated.GetComponent<MeshRenderer>().material.color;
        col.a = 0.3f;
        instantiated.GetComponent<MeshRenderer>().material.color = col;
        Sequence seq = DOTween.Sequence()
            .Append(instantiated.transform.DOScale(finalSize, timeLeft).SetEase(curve))
            .InsertCallback(SoundManager.Instance.LastBeat.beatInterval * 0.85f, () => AnimationThick(0.05f))
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
            })
            .SetUpdate(true)
            .Play();

        SequenceAndTarget seqAndTar = new SequenceAndTarget();
        seqAndTar.target = instantiated;
        seqAndTar.sequence = seq;
        allInstances.Enqueue(seqAndTar);
    }

    void AnimationThick(float delta)
    {
        Sequence seq = DOTween.Sequence();
        Material mat = GetComponent<MeshRenderer>().material;
        mat.color = colorMetronome;

        foreach(Texture text in animationTexture)
        {
            seq.AppendCallback(() => mat.mainTexture = text);
            seq.AppendInterval(delta);
        }

        animationTexture.Reverse();

        foreach (Texture text in animationTexture)
        {
            seq.AppendCallback(() => mat.mainTexture = text);
            seq.AppendInterval(delta);
        }

        animationTexture.Reverse();

        seq.Play();
    }

    public void CorrectInput()
    {
        if (!mustBeDisplayed)
            return;

        ChangeFirstCircleColor(goodInput);
        GameObject instantiated = Instantiate(goodPrefab, transform);
        instantiated.transform.localPosition = Vector3.up * 0.45f;
        Destroy(instantiated, 2);
    }

    public void PerfectInput()
    {
        if (!mustBeDisplayed)
            return;

        ChangeFirstCircleColor(perfectInput);
        GameObject instantiated = Instantiate(perfectPrefab, transform);
        instantiated.transform.localScale = Vector3.one * 5.5f;
        instantiated.transform.localPosition = Vector3.up * 0.45f;
        Destroy(instantiated, 2);
    }

    public void WrongInput()
    {
        if (!mustBeDisplayed)
            return;

        ChangeFirstCircleColor(wrongInput);
    }

    void ChangeFirstCircleColor(Color col)
    {
        if (allInstances.Count > 0)
        {
            SequenceAndTarget seqTar = allInstances.Peek();
            if (seqTar.target != null)
            {
                Color tempColor = new Color(col.r, col.g, col.b, 0);
                seqTar.target.GetComponent<MeshRenderer>().material.color = col;
            }
        }
    }

    void CombatStatusChange(bool value)
    {
        if (transitionSequence != null)
            transitionSequence.Kill();

        mustBeDisplayed = value;

        if (!mustBeDisplayed)
        {
            while (allInstances.Count != 0)
            {
                SequenceAndTarget seqTar = allInstances.Dequeue();
                seqTar.sequence.Kill();
                Destroy(seqTar.target);
            }
        }

        transitionSequence = DOTween.Sequence()
                    .Append(rootParent.GetComponent<MeshRenderer>().material.DOColor(mustBeDisplayed ? Color.white : Color.clear, 1))
                    .Insert(0, arrow.GetComponent<MeshRenderer>().material.DOColor(mustBeDisplayed ? new Color(0, 0, 0, 1) : Color.clear, 1))
                    .Play();
    }
}
