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
    [InfoBox("Pensez à activer le mode Solo en haut du Component Cinemachine Virtual Camera")] [SerializeField]
    string hello = "Pensez-yyyy";

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
    float shakeDuration = 0;

    [TabGroup("ScreenShake")] [SerializeField]
    float shakeIntensity = 0;

    [TabGroup("ScreenShake")] [SerializeField]
    AnimationCurve shakeIntensityOverTime = null;

    CinemachineBasicMultiChannelPerlin perlin;

    [TabGroup("ScreenShake")] [Button(ButtonSizes.Medium)]
    void TestScreenShake()
    {
        LoadRefs();
        StartScreenShake(shakeDuration, shakeIntensity);
    }

    public void StartScreenShake(float duration, float intensity)
    {
        StartCoroutine(ScreenShakeSequence(duration, intensity));
    }

    IEnumerator ScreenShakeSequence(float duration, float intensity)
    {
        Vector3 originPosition = transform.position;
        Debug.Assert(duration > 0);
        float normalizedTime = 0;

        while (normalizedTime < 1)
        {
            normalizedTime += Time.deltaTime / duration;
            perlin.m_AmplitudeGain = intensity * shakeIntensityOverTime.Evaluate(normalizedTime);
            yield return null;
        }
    }
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

    [TabGroup("Zoom")] [SerializeField] [Tooltip("Absolute = Value + Modif ; Relative = Value * Modif")]
    ValueType valueType = ValueType.Absolute;

    [TabGroup("Zoom")] [SerializeField]
    ZoomType modifierType = ZoomType.Distance;

    [TabGroup("Zoom")] [SerializeField]
    float zoomDuration = 0;

    [TabGroup("Zoom")] [SerializeField]
    float zoomIntensity = 0;

    [TabGroup("Zoom")] [SerializeField]
    AnimationCurve zoomIntensityOverTime = null;

    [TabGroup("Zoom")] [Button(ButtonSizes.Medium)]
    void TestZoom()
    {
        DOTween.Init();
        LoadRefs();
        StartZoom(shakeIntensity, zoomDuration, modifierType, valueType);
    }

    public void SetZoomFOV(float newValue)
    {
        if (VirtualCam == null)
            VirtualCam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        VirtualCam.m_Lens.FieldOfView = newValue;
    }

    CinemachineFramingTransposer transposer;
    IEnumerator currentZooming;

    public void StartZoom(float modifier, float duration, ZoomType zoomType, ValueType vt)
    {
        //No need to set zoom
        if (modifier == 0 && vt == ValueType.Absolute)
            return;

        // If we multiply, multiply by a positive number
        if (vt == ValueType.Relative)
            modifier = Mathf.Abs(modifier);

        //Cannot have 2 zooming sequence at the same time
        if (currentZooming != null)
            StopCoroutine(currentZooming);

        currentZooming = ZoomCoroutine(modifier, zoomDuration, zoomType, vt);
        StartCoroutine(currentZooming);
    }

    IEnumerator ZoomCoroutine(float modifier, float duration, ZoomType zoomType, ValueType vt)
    {
        float normalizedTime = 0;
        if (zoomType == ZoomType.FOV)
        {
            float originValue = VirtualCam.m_Lens.FieldOfView;
            float targetValue = vt == ValueType.Relative ? originValue * modifier : originValue - modifier;
            while (normalizedTime < 1)
            {
                normalizedTime += Time.deltaTime / duration;
                VirtualCam.m_Lens.FieldOfView = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(normalizedTime));
                yield return null;
            }

            if (!Application.isPlaying)
            {
                VirtualCam.m_Lens.FieldOfView = Mathf.Lerp(originValue, targetValue, zoomIntensityOverTime.Evaluate(0));
            }
        }
        else
        {
            float originValue = transposer.m_CameraDistance;
            float targetValue = vt == ValueType.Relative ? originValue * modifier : originValue - modifier;
            while (normalizedTime < 1)
            {
                normalizedTime += Time.deltaTime / duration;
                transposer.m_CameraDistance = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(normalizedTime));
                yield return null;
            }

            if (!Application.isPlaying)
            {
                transposer.m_CameraDistance = Mathf.Lerp(originValue, targetValue, zoomIntensityOverTime.Evaluate(0));
            }
        }      
    }
    #endregion
}
