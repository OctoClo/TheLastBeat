using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class VisualParams
{
    public Image flameImage;
    public RectTransform flameTransform;
    public Color leftMostColor;
    public Color rightMostColor;
    public float riftStep1 = 0.7f;
    public float riftStep2 = 0.5f;
    public float riftStep3 = 0.2f;
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
    public float screeningFadeDuration = 0;
    public Image screeningBorders = null;
    public Image screeningVeins = null;
    public Image screeningGreen = null;
    public Image screeningShield = null;
    public List<LifeContainer> allContainers = new List<LifeContainer>();
}

public class HealthVisual
{
    VisualParams visualParams;
    Sequence criticSequence;
    Sequence sizeSequence;
    bool isChangingColor = false;
    Color originColor = Color.black;
    Vector3 originSize = Vector3.one;

    Sequence criticScreeningSeq = null;
    Color transparentWhite = new Color(1, 1, 1, 0);

    public HealthVisual(VisualParams vp)
    {
        visualParams = vp;
        originColor = visualParams.flameImage.color;
        originSize = visualParams.flameTransform.localScale;
    }

    public void HurtAnimationUI(bool fromEnemy)
    {
        if (!isChangingColor)
        {
            isChangingColor = true;
            
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() => visualParams.flameImage.color = visualParams.hurtColor);
            seq.AppendInterval(0.05f);
            seq.AppendCallback(() => visualParams.flameImage.color = originColor);
            seq.AppendInterval(0.05f);
            seq.SetLoops(10);
            seq.AppendCallback(() => isChangingColor = false);
            seq.Play();

            if (criticScreeningSeq == null)
                LaunchScreening(fromEnemy);
        }       
    }

    public void LaunchScreening(bool fromEnemy)
    {
        Sequence seq = DOTween.Sequence();
        seq.InsertCallback(0, () =>
        {
            visualParams.screeningBorders.color = Color.white;
            if (!fromEnemy)
                visualParams.screeningVeins.color = Color.white;
        });
        seq.Insert(0, visualParams.screeningBorders.DOFade(0, visualParams.screeningFadeDuration));
        seq.Insert(0, visualParams.screeningVeins.DOFade(0, visualParams.screeningFadeDuration));
    }

    public void LaunchScreeningHeal()
    {
        Sequence seq = DOTween.Sequence()
            .InsertCallback(0, () => visualParams.screeningGreen.color = Color.white)
            .Insert(0, visualParams.screeningGreen.DOFade(0, visualParams.screeningFadeDuration));
    }

    public void LaunchScreeningShield()
    {
        Sequence seq = DOTween.Sequence()
            .InsertCallback(0, () => visualParams.screeningShield.color = Color.white)
            .Insert(0, visualParams.screeningShield.DOFade(0, visualParams.screeningFadeDuration));
    }

    public void ScreenShake()
    {
        foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
            ce.StartScreenShake(visualParams.screenShakeDuration, visualParams.screenShakeIntensity);
    }

    public void UIScreenShake()
    {
        SceneHelper.Instance.ScreenshakeGameObject(visualParams.flameImage.transform, visualParams.screenShakeDurationUI, visualParams.screenShakeIntensityUI);
    }

    public void RestartScreeningSeq()
    {
        float timePerDemiBeat = SoundManager.Instance.TimePerBeat * 0.5f;
        criticScreeningSeq = DOTween.Sequence()
                                        .Insert(0, visualParams.screeningBorders.DOFade(0.5f, timePerDemiBeat))
                                        .Insert(0, visualParams.screeningVeins.DOFade(0.5f, timePerDemiBeat))
                                        .Insert(timePerDemiBeat, visualParams.screeningBorders.DOFade(1, timePerDemiBeat))
                                        .Insert(timePerDemiBeat, visualParams.screeningVeins.DOFade(1, timePerDemiBeat));
    }

    public void EnterCriticState()
    {
        if (sizeSequence != null)
        {
            sizeSequence.Kill();
        }

        sizeSequence = DOTween.Sequence()
            .Append(visualParams.flameTransform.DOScale(originSize * visualParams.sizeCritic, 0.1f));

        criticSequence = DOTween.Sequence();
        criticSequence.Append(DOTween.To(() => visualParams.flameImage.color, x => visualParams.flameImage.color = x, Color.white, 0.1f));
        criticSequence.Append(DOTween.To(() => visualParams.flameImage.color, x => visualParams.flameImage.color = x, Color.red, 0.1f));
        criticSequence.SetLoops(-1);
        criticSequence.Play();

        visualParams.screeningBorders.color = Color.white;
        visualParams.screeningVeins.color = Color.white;
    }

    public void ExitCriticState()
    {
        if (criticSequence != null)
        {
            visualParams.flameImage.DOColor(originColor, 0.2f);
            criticSequence.Kill();

            if (sizeSequence != null)
                sizeSequence.Kill();

            sizeSequence = DOTween.Sequence()
                .Append(visualParams.flameTransform.DOScale(originSize, 0.2f));
        }

        if (criticScreeningSeq != null)
        {
            criticScreeningSeq.Kill();
            visualParams.screeningBorders.color = transparentWhite;
            visualParams.screeningVeins.color = transparentWhite;
        }
    }

    public void SetRiftAnimation(int index)
    {
        if (index == 0)
        {
            visualParams.riftAnimator.GetComponent<Image>().enabled = false;
            return;
        }

        visualParams.riftAnimator.GetComponent<Image>().enabled = true;
        visualParams.riftAnimator.SetInteger("indexState", index);
    }

    public void UpdateColor(int life)
    {
        if (life >= visualParams.riftStep1)
            SetRiftAnimation(0);

        if (life < visualParams.riftStep1 && life >= visualParams.riftStep2)
            SetRiftAnimation(1);

        if (life < visualParams.riftStep2 && life >= visualParams.riftStep3)
            SetRiftAnimation(2);

        if (life < visualParams.riftStep3)
            SetRiftAnimation(3);
    }

    public void UpdateContainer(int lifeLeft)
    {
        for (int i = 0; i < visualParams.allContainers.Count; i++)
        {
            LifeContainer.StateHealthCell status = LifeContainer.StateHealthCell.EMPTY;
            if (lifeLeft == (i * 2) + 1)
            {
                status = LifeContainer.StateHealthCell.HALF_EMPTY;
            }
            else if (lifeLeft > i * 2)
            {
                status = LifeContainer.StateHealthCell.FULL;
            }
            visualParams.allContainers[i].CurrentState = status;
        }
    }
}
