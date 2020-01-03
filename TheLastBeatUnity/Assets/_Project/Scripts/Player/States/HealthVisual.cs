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
    public Animator riftAnimator = null;
    public float sequenceDuration = 0;
    public float screenShakeDuration = 0;
    public float screenShakeIntensity = 0;
    public float screenShakeDurationUI = 0;
    public float screenShakeIntensityUI = 0;
}

public class HealthVisual
{
    VisualParams visualParams;
    Sequence seq;
    Sequence criticSequence;
    Vector3 normalSize = Vector3.zero;

    public HealthVisual(VisualParams vp)
    {
        visualParams = vp;
        normalSize = visualParams.flameTransform.localScale;
    }

    public void RegularBeat(PulseZone zone)
    {
        seq = DOTween.Sequence();
        seq.Append(visualParams.flameTransform.DOScale(normalSize * zone.ScaleModifier, visualParams.sequenceDuration));
        seq.Append(visualParams.flameTransform.DOScale(normalSize,  visualParams.sequenceDuration));
        seq.Play();
    }

    public void ScreenShake()
    {
        CameraManager.Instance.LiveCamera.GetComponent<CameraEffect>().StartScreenShake(visualParams.screenShakeDuration, visualParams.screenShakeIntensity);
    }

    public void UIScreenShake()
    {
        SceneHelper.Instance.ScreenshakeGameObject(visualParams.flameImage.transform, visualParams.screenShakeDurationUI, visualParams.screenShakeIntensityUI);
    }

    public void EnterCriticState(PulseZone zone)
    {
        visualParams.flameTransform.DOScale(normalSize * zone.ScaleModifier, 0.1f);
        criticSequence = DOTween.Sequence();
        criticSequence.Append(DOTween.To(() => visualParams.flameImage.color, x => visualParams.flameImage.color = x, Color.white, 0.1f));
        criticSequence.Append(DOTween.To(() => visualParams.flameImage.color, x => visualParams.flameImage.color = x, zone.colorRepr, 0.1f));
        criticSequence.SetLoops(-1);
        criticSequence.Play();
        visualParams.riftAnimator.SetInteger("indexState", 3);
    }

    public void ExitCriticState()
    {
        if (criticSequence != null)
        {
            criticSequence.Kill();
            visualParams.flameTransform.DOScale(normalSize, 0.2f);
        }
    }

    public void SetRiftAnimation(int index, int listLength)
    {
        if (index == listLength - 2)
        {
            visualParams.riftAnimator.SetInteger("indexState", 2);
        }
        else if (index == listLength - 3)
        {
            visualParams.riftAnimator.SetInteger("indexState", 1);
        }
        else
        {
            visualParams.riftAnimator.SetInteger("indexState", 0);
        }
    }

    public void TransitionColor(Color newColor)
    {
        DOTween.To(() => visualParams.flameImage.color, x => visualParams.flameImage.color = x, newColor, 0.5f);
    }
}
