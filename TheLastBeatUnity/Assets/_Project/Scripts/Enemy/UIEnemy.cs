using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIEnemy : Slowable
{
    [SerializeField]
    Color colorOn = Color.green;

    [SerializeField]
    Color colorOff = Color.black;

    [SerializeField]
    Color colorHurt = Color.red;

    [SerializeField]
    GameObject prefabUILife = null;

    [SerializeField]
    RectTransform rootLife = null;

    [SerializeField]
    RectTransform rootRewind = null;

    [SerializeField]
    GameObject prefabRewind = null;

    [SerializeField]
    AnimationCurve curveRewindMark = null;
    Sequence currentSequence = null;
    List<Sequence> hitSequences = new List<Sequence>();

    int maxHP = 0;
    int life = 0;
    public int Life
    {
        get
        {
            return life;
        }
        set
        {
            int prevValue = life;
            life = Mathf.Max(0, value);
            RecomputeSprite(prevValue);
        }
    }

    List<Image> allLifeImages = new List<Image>();
    Stack<Image> allRewindMark = new Stack<Image>();

    private void Start()
    {
        DisappearHud(true);
    }

    void RecomputeSprite(int previousValue)
    {
        if (life < previousValue)
        {
            for (int i = life; i < previousValue; i++)
            {
                HurtAnimation(allLifeImages[i]);
            }
        }
        else
        {
            for (int i = previousValue; i < life; i++)
            {
                allLifeImages[i].color = colorOn;
            }
        }
    }

    void HurtAnimation(Image img)
    {
        img.color = colorHurt;
        hitSequences.Add(CreateSequence().Append(img.DOColor(colorOff, 1.5f)));
    }

    public void Init(int lifeAmount = 3)
    {
        maxHP = lifeAmount;
        float maxX = (0.11f * lifeAmount) + 0.15f;
        rootLife.parent.GetComponent<RectTransform>().anchorMax = new Vector2(maxX, 1);
        rootRewind.GetComponent<RectTransform>().anchorMin = new Vector2(maxX - (maxX * 0.14f), 0);
        for (int i = 0; i < lifeAmount; i++)
        {
            allLifeImages.Add(Instantiate(prefabUILife, rootLife).GetComponent<Image>());
        }

        float width = 0.42f * ((float)lifeAmount / 5.0f);
        for (int i = 0; i < 3; i++)
        {
            rootRewind.GetChild(i).GetComponent<RectTransform>().anchorMin = new Vector2(0.5f * width * i, 0);
            rootRewind.GetChild(i).GetComponent<RectTransform>().anchorMax = new Vector2((0.5f * width * i) + width, 1);
        }

        Life = lifeAmount;
    }

    public void Appear()
    {
        AppearHud();
    }

    public void TryDisappear()
    {
        if (Life == maxHP)
            DisappearHud();
    }

    public void DisappearHud(bool instant = false)
    {
        if (currentSequence != null)
            currentSequence.Kill();
        foreach (Sequence seq in hitSequences)
            seq.Kill();

        currentSequence = CreateSequence();
        currentSequence.Append(
            DOTween.To(() => 1.0f, x =>
            {
                foreach (Image img in transform.GetComponentsInChildren<Image>())
                {
                    img.color = new Color(img.color.r, img.color.g, img.color.b, x);
                }
            }, 0, instant ? 0 : 0.4f).SetEase(Ease.Linear));
    }

    void AppearHud()
    {
        if (currentSequence != null)
            currentSequence.Kill();
        foreach(Sequence seq in hitSequences)
            seq.Kill();
        
        currentSequence = CreateSequence();
        currentSequence.Append(
            DOTween.To(() => 0, x =>
            {
                foreach (Image img in transform.GetComponentsInChildren<Image>())
                {
                    img.color = new Color(img.color.r, img.color.g, img.color.b, x);
                }
            }, 1.0f, 0.2f).SetEase(Ease.Linear));
    }

    public void AddRewindMark()
    {
        GameObject newMark = Instantiate(prefabRewind, rootRewind.GetChild(Mathf.Min(2,allRewindMark.Count)).transform);
        allRewindMark.Push(newMark.GetComponent<Image>());
        CreateSequence().Append(newMark.transform.DOScale(3.0f, 0.45f).SetEase(curveRewindMark));
    }

    public void RemoveRewindMark()
    {
        if (allRewindMark.Count > 0)
        {
            Image img = allRewindMark.Pop();
            if (img)
            {
                CreateSequence().Append(img.transform.DOScale(0, 0.3f).SetEase(curveRewindMark))
                    .AppendCallback(() => Destroy(img.gameObject));
            }
        }
    }

    private void OnDestroy()
    {
        Purge();
    }
}
