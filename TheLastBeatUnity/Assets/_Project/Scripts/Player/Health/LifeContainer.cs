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
    AnimationCurve curveHeal;

    [SerializeField]
    RectTransform rootSlash;

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
        currentPart.position = Vector3.zero;
        currentPart.GetComponent<Image>().color = healColor;
        DOTween.Sequence()
            .Append(currentPart.DOScale(1, 0.5f).SetEase(curveHeal))
            .Insert(0, currentPart.GetComponent<Image>().DOColor(normalColor, 0.5f));
    }

    void HurtCell(int index, bool triggerSlash = false)
    {
        if (triggerSlash)
        {
            Destroy(Instantiate(slashAnimation, rootSlash), 1);
        }

        Color targetColor = hurtColor;
        targetColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0);
        DOTween.Sequence()
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
                HurtCell(0);
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
