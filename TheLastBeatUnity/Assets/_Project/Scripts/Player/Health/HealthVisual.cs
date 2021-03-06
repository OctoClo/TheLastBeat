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
    public Image backgroundImage;
    public RectTransform flameTransform;
    public Color leftMostColor;
    public Color rightMostColor;
    public float ratioRiftStep1 = 0.7f;
    public float ratioRiftStep2 = 0.5f;
    public float ratioRiftStep3 = 0.2f;
    public float pulseSize = 1.05f;
    public float sizeCritic = 1.15f;
    public Color hurtColor = Color.clear;
    public AnimationCurve curveTransition;
    public float timeLifeTransition = 0.25f;
    public float screenShakeIntensityUI = 0;
    public float screenShakeDurationUI = 0;
    public float screenShakeIntensity = 0;
    public float screenShakeDuration = 0;
    public float sequenceDuration = 0;
    public Animator riftAnimator;
}

public class HealthVisual
{
    VisualParams visualParams;
    Sequence seq;
    Sequence criticSequence;
    Vector3 normalSize = Vector3.zero;
    bool isChangingColor = false;
    Color flameColor = Color.black;
    Sprite sprtStart = null;

    public HealthVisual(VisualParams vp)
    {
        visualParams = vp;
        normalSize = visualParams.flameTransform.localScale;
        flameColor = visualParams.flameImage.color;
        sprtStart = visualParams.riftAnimator.GetComponent<UnityEngine.UI.Image>().sprite;
    }

    public void HurtAnimationUI()
    {
        if (!isChangingColor)
        {
            isChangingColor = true;
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() => visualParams.flameImage.color = visualParams.hurtColor);
            seq.AppendInterval(0.05f);
            seq.AppendCallback(() => visualParams.flameImage.color = flameColor);
            seq.AppendInterval(0.05f);
            seq.SetLoops(10);
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

    public void ScreenShake()
    {
        foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
        {
            ce.StartScreenShake(visualParams.screenShakeDuration, visualParams.screenShakeIntensity);
        }
    }

    public void UIScreenShake()
    {
        SceneHelper.Instance.ScreenshakeGameObject(visualParams.flameImage.transform, visualParams.screenShakeDurationUI, visualParams.screenShakeIntensityUI);
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
        if (index == 0)
        {
            visualParams.riftAnimator.enabled = false;
            visualParams.riftAnimator.GetComponent<UnityEngine.UI.Image>().sprite = sprtStart;
            return;
        }

        visualParams.riftAnimator.enabled = true;
        visualParams.riftAnimator.SetInteger("indexState", index);
    }

    public void UpdateColor(float ratio)
    {
        Color currentColor = Color.Lerp(visualParams.leftMostColor, visualParams.rightMostColor, ratio);
        visualParams.barImage.DOColor(currentColor, 0.25f);
        visualParams.backgroundImage.DOColor(currentColor, 0.25f);
        DOTween.To(() => visualParams.barImage.fillAmount, x => visualParams.barImage.fillAmount = x, ratio, visualParams.timeLifeTransition).SetEase(visualParams.curveTransition);

        if (ratio > visualParams.ratioRiftStep1)
            SetRiftAnimation(0);

        if (ratio < visualParams.ratioRiftStep1 && ratio > visualParams.ratioRiftStep2)
            SetRiftAnimation(1);

        if (ratio < visualParams.ratioRiftStep2 && ratio > visualParams.ratioRiftStep3)
            SetRiftAnimation(2);

        if (ratio < visualParams.ratioRiftStep3)
            SetRiftAnimation(3);
    }
}
