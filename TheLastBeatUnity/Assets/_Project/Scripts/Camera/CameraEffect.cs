using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Cinemachine;
using DG.Tweening;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraEffect : MonoBehaviour
{
    public CinemachineVirtualCamera VirtualCam { get; private set; }

    private void Start()
    {
        LoadRefs();
    }

    void LoadRefs()
    {
        VirtualCam = GetComponent<CinemachineVirtualCamera>();
        perlin = VirtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        transposer = VirtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    #region ScreenShake

    [TabGroup("ScreenShake")] [SerializeField]
    AnimationCurve shakeIntensityOverTime = null;

    CinemachineBasicMultiChannelPerlin perlin;

    public void StartScreenShake(float duration, float intensity, float frequency = 1)
    {
        perlin = VirtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_FrequencyGain = frequency;
        if (perlin)
        {
            screenShakeSequence = DOTween.Sequence();
            screenShakeSequence.Append(DOTween.To(() => perlin.m_AmplitudeGain, x => perlin.m_AmplitudeGain = x, intensity, duration).SetEase(shakeIntensityOverTime));
            screenShakeSequence.Play();
        }
    }

    Sequence screenShakeSequence;
    #endregion

    #region Zoom

    public enum ZoomType
    {
        FOV,
        Distance
    };

    public enum ValueType
    {
        Relative,
        Absolute
    }

    [TabGroup("Zoom")] [SerializeField]
    AnimationCurve zoomIntensityOverTime = null;

    CinemachineFramingTransposer transposer;
    Sequence zoomingSequence;

    public void StartZoom(float modifier, float duration, ZoomType zoomType, ValueType vt)
    {
        //No need to set zoom
        if (modifier == 0 && vt == ValueType.Absolute)
            return;

        // If we multiply, multiply by a positive number
        if (vt == ValueType.Relative)
            modifier = Mathf.Abs(modifier);

        zoomingSequence = DOTween.Sequence();
        Tweener tweener;
        if (zoomType == ZoomType.FOV)
        {
            if (vt == ValueType.Relative)
            {
                tweener = DOTween.To(() => VirtualCam.m_Lens.FieldOfView, x => VirtualCam.m_Lens.FieldOfView = x, VirtualCam.m_Lens.FieldOfView * modifier, duration);
            }
            else
            {
                tweener = DOTween.To(() => VirtualCam.m_Lens.FieldOfView, x => VirtualCam.m_Lens.FieldOfView = x, modifier, duration);
            }
        }
        else
        {
            transposer = VirtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (vt == ValueType.Relative)
            {
                tweener = DOTween.To(() => transposer.m_CameraDistance, x => transposer.m_CameraDistance = x, transposer.m_CameraDistance * modifier, duration);
            }
            else
            {
                tweener = DOTween.To(() => transposer.m_CameraDistance, x => transposer.m_CameraDistance = x, modifier, duration);
            }
        }
        tweener.SetEase(zoomIntensityOverTime);
        zoomingSequence.Append(tweener);
        zoomingSequence.Play();
    }
    #endregion
}
