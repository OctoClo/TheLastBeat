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
    float defaultIntensityScreenShake = 0;

    [TabGroup("ScreenShake")]
    [SerializeField]
    float defaultScreenShakeDuration = 0;

    [TabGroup("ScreenShake")][SerializeField]
    AnimationCurve intensityOverTime = null;

    CinemachineBasicMultiChannelPerlin perlin;

    [TabGroup("ScreenShake")]
    [InfoBox("Ne fonctionne que si le mode solo de la camera est activé", InfoMessageType.None)]
    [Button(ButtonSizes.Medium)]
    void Test()
    {
        LoadRefs();
        StartScreenShake(defaultScreenShakeDuration, defaultIntensityScreenShake);
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
            perlin.m_AmplitudeGain = intensity * intensityOverTime.Evaluate(normalizedTime);
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

    [TabGroup("Zoom")]
    [SerializeField]
    readonly ValueType valueType = ValueType.Absolute;

    [TabGroup("Zoom")]
    [SerializeField]
    readonly ZoomType modifierType = ZoomType.Distance;

    [TabGroup("Zoom")]
    [SerializeField]
    readonly float durationZoom = 0;

    [TabGroup("Zoom")]
    [SerializeField]
    readonly AnimationCurve zoomOverTime = null;

    [TabGroup("Zoom")]
    [SerializeField]
    readonly float valueForTest = 0;

    [TabGroup("Zoom")]
    [Button(ButtonSizes.Medium)]
    [InfoBox("Plus fluide si le mode solo est activé", InfoMessageType.None)]
    void TestZoom()
    {
        DOTween.Init();
        LoadRefs();
        StartZoom(valueForTest, durationZoom, modifierType, valueType);
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

        currentZooming = ZoomCoroutine(modifier, durationZoom, zoomType, vt);
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
                normalizedTime += Time.deltaTime * TimeManager.Instance.CurrentTimeScale / duration;
                VirtualCam.m_Lens.FieldOfView = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(normalizedTime));
                yield return null;
            }

            if (!Application.isPlaying)
            {
                VirtualCam.m_Lens.FieldOfView = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(0));
            }
        }
        else
        {
            float originValue = transposer.m_CameraDistance;
            float targetValue = vt == ValueType.Relative ? originValue * modifier : originValue - modifier;
            while (normalizedTime < 1)
            {
                normalizedTime += Time.deltaTime * TimeManager.Instance.CurrentTimeScale / duration;
                transposer.m_CameraDistance = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(normalizedTime));
                yield return null;
            }

            if (!Application.isPlaying)
            {
                transposer.m_CameraDistance = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(0));
            }
        }      
    }
    #endregion
}
