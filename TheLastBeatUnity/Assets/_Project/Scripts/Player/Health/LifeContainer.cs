using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LifeContainer : MonoBehaviour
{
    [SerializeField]
    GameObject slashAnimation = null;

    [SerializeField]
    List<RectTransform> allParts;

    [SerializeField]
    Color hurtColor = Color.red;

    [SerializeField]
    Color healColor = Color.green;

    [SerializeField]
    Color normalColor = Color.black;

    [SerializeField]
    RectTransform rootSlash;

    Dictionary<RectTransform, Sequence> allSequences = new Dictionary<RectTransform, Sequence>();

    public enum StateHealthCell
    {
        EMPTY,
        HALF_EMPTY,
        FULL
    }

    StateHealthCell currentState = StateHealthCell.FULL;
    public StateHealthCell CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            ComputeNewState(currentState, value);
            currentState = value;
        }
    }

    void HealCell(int index)
    {
        RectTransform currentPart = allParts[index];
        currentPart.GetComponent<Image>().color = healColor;
        currentPart.localPosition = Vector3.zero;
        currentPart.localScale = Vector3.zero;

        if (allSequences.ContainsKey(currentPart) && allSequences[currentPart] != null)
            allSequences[currentPart].Kill();

        allSequences[currentPart] = DOTween.Sequence()
            .Append(currentPart.DOScale(1.2f, 0.1f))
            .AppendInterval(0.7f)
            .Append(currentPart.DOScale(1, 0.3f))
            .Insert(0.8f, currentPart.GetComponent<Image>().DOColor(normalColor , 0.3f));
    }

    void HurtCell(int index, bool triggerSlash = false)
    {
        allParts[index].GetComponent<Image>().color = Color.white;
        allParts[index].localPosition = Vector3.zero;
        allParts[index].localScale = Vector3.one;

        if (triggerSlash)
        {
            Destroy(Instantiate(slashAnimation, rootSlash), 1);
        }

        GameObject copyHurt = Instantiate(allParts[index].gameObject, transform);
        copyHurt.GetComponent<Image>().color = hurtColor;
        copyHurt.GetComponent<Image>().DOColor(new Color(hurtColor.r, hurtColor.g, hurtColor.b, 0), 1.5f);
        Destroy(copyHurt, 1.6f);

        Color targetColor = hurtColor;
        targetColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0);

        if (allSequences.ContainsKey(allParts[index]) && allSequences[allParts[index]] != null)
            allSequences[allParts[index]].Kill();

        allSequences[allParts[index]] = DOTween.Sequence()
            .Append(allParts[index].DOMove(new Vector3(0,-100.0f, 0), 0.5f).SetRelative(true))
            .Insert(0, allParts[index].GetComponent<Image>().DOColor(targetColor, 0.5f));
    }

    void ComputeNewState(StateHealthCell previous , StateHealthCell actual)
    {
        if (previous == actual)
        {
            return;
        }

        if (previous == StateHealthCell.EMPTY)
        {
            HealCell(0);
            if (actual == StateHealthCell.FULL)
                HealCell(1);
        }

        if (previous == StateHealthCell.HALF_EMPTY)
        {
            if (actual == StateHealthCell.EMPTY)
                HurtCell(0, true);
            else
                HealCell(1);
        }

        if (previous == StateHealthCell.FULL)
        {
            HurtCell(1, true);
            if (actual == StateHealthCell.EMPTY)
            {
                HurtCell(0);
            }
        }
    }
}
