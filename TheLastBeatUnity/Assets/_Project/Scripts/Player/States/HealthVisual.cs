﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class VisualParams
{
    public Image flameImage;
    public Image barImage;
    public RectTransform flameTransform;
    public Animator riftAnimator;
    public float sequenceDuration;
    public Color leftMostColor;
    public Color rightMostColor;
    public float ratioRiftStep1 = 0.7f;
    public float ratioRiftStep2 = 0.5f;
    public float ratioRiftStep3 = 0.2f;
    public float pulseSize = 1.05f;
    public float sizeCritic = 1.15f;
    public Color hurtColor = Color.clear;
    public AnimationCurve curveTransition;
}

public class HealthVisual
{
    VisualParams visualParams;
    Sequence seq;
    Sequence criticSequence;
    Vector3 normalSize = Vector3.zero;
    bool isChangingColor = false;
    Color flameColor = Color.black;

    public HealthVisual(VisualParams vp)
    {
        visualParams = vp;
        normalSize = visualParams.flameTransform.localScale;
        flameColor = visualParams.flameImage.color;
    }

    public void HurtAnimationUI()
    {
        if (!isChangingColor)
        {
            isChangingColor = true;
            Sequence seq = DOTween.Sequence();
            seq.Append(visualParams.flameImage.DOColor(visualParams.hurtColor, 0.25f).SetEase(Ease.InOutSine));
            seq.Append(visualParams.flameImage.DOColor(flameColor, 0.25f).SetEase(Ease.InOutSine));
            seq.SetLoops(3);
            seq.AppendCallback(() => isChangingColor = false);
        }       
    }

    public void RegularBeat()
    {
        seq = DOTween.Sequence();
        seq.Append(visualParams.flameTransform.DOScale(normalSize * visualParams.pulseSize, visualParams.sequenceDuration));
        seq.Append(visualParams.flameTransform.DOScale(normalSize,  visualParams.sequenceDuration));
        seq.Play();
    }

    public void EnterCriticState()
    {
        visualParams.flameTransform.DOScale(normalSize * visualParams.sizeCritic, 0.1f);
        criticSequence = DOTween.Sequence();
        criticSequence.Append(DOTween.To(() => visualParams.flameImage.color, x => visualParams.flameImage.color = x, Color.white, 0.1f));
        criticSequence.Append(DOTween.To(() => visualParams.flameImage.color, x => visualParams.flameImage.color = x, Color.red, 0.1f));
        criticSequence.SetLoops(-1);
        criticSequence.Play();
    }

    public void ExitCriticState()
    {
        if (criticSequence != null)
        {
            criticSequence.Kill();
            visualParams.flameTransform.DOScale(normalSize, 0.2f);
        }
    }

    public void SetRiftAnimation(int index)
    {
        visualParams.riftAnimator.SetInteger("indexState", index);
    }

    public void UpdateColor(float ratio)
    {
        Color currentColor = Color.Lerp(visualParams.leftMostColor, visualParams.rightMostColor, ratio);
        visualParams.barImage.DOColor(currentColor, 0.25f);
        DOTween.To(() => visualParams.barImage.fillAmount, x => visualParams.barImage.fillAmount = x, ratio, 0.1f).SetEase(visualParams.curveTransition);

        if (ratio < visualParams.ratioRiftStep1 && ratio > visualParams.ratioRiftStep2)
            SetRiftAnimation(1);

        if (ratio < visualParams.ratioRiftStep2 && ratio > visualParams.ratioRiftStep3)
            SetRiftAnimation(2);

        if (ratio < visualParams.ratioRiftStep3)
            SetRiftAnimation(3);
    }
}
