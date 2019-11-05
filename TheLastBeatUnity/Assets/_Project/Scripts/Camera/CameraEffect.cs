using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cinemachine;

[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
public class CameraEffect : MonoBehaviour
{
    private void Start()
    {
        perlin = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    #region ScreenShake
    [TabGroup("ScreenShake")] [SerializeField]
    float intensityScreenShake;

    [TabGroup("ScreenShake")] [SerializeField]
    float screenShakeDuration;

    [TabGroup("ScreenShake")] [SerializeField]
    AnimationCurve intensityOverTime;

    CinemachineBasicMultiChannelPerlin perlin;

    [TabGroup("ScreenShake")][InfoBox("Ne fonctionne que si le mode solo de la camera est activé", InfoMessageType.None)][Button(ButtonSizes.Medium)]
    public void Test()
    {
        StartScreenShake();
    }

    public void StartScreenShake()
    {
        StartCoroutine(ScreenShakeSequence(screenShakeDuration));
    }

    IEnumerator ScreenShakeSequence(float duration)
    {
        Vector3 originPosition = transform.position;
        Debug.Assert(duration > 0);
        float normalizedTime = 0;

        while (normalizedTime < 1)
        {
            normalizedTime += Time.deltaTime / duration;
            perlin.m_AmplitudeGain = intensityScreenShake * intensityOverTime.Evaluate(normalizedTime);
            yield return null;
        }
    }
    #endregion

    #region Zoom

    enum ZoomType
    {
        FOV,
        Distance
    };

    enum ValueType
    {
        Relative,
        Absolute
    }

    [TabGroup("Zoom")] [SerializeField]
    ValueType valueType;

    [TabGroup("Zoom")] [SerializeField]
    ZoomType modifierType;

    [TabGroup("Zoom")] [SerializeField]
    float durationZoom;

    [TabGroup("Zoom")] [SerializeField]
    AnimationCurve zoomOverTime;

    [TabGroup("Zoom")] [SerializeField]
    float valueForTest;

    [TabGroup("Zoom")] [Button(ButtonSizes.Medium)] [InfoBox("Plus fluide si le mode solo est activé", InfoMessageType.None)]
    public void TestZoom()
    {
        transposer = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        StartZoom(valueForTest);
    }

    CinemachineFramingTransposer transposer;
    IEnumerator currentZooming;

    public void StartZoom(float modifier)
    {
        modifier = Mathf.Abs(modifier);
        Debug.Assert(modifier > 0, "Zoom cannot be set to 0");
        if (currentZooming != null)
            StopCoroutine(currentZooming);

        currentZooming = ZoomCoroutine(modifier, durationZoom);
        StartCoroutine(currentZooming);
    }

    IEnumerator ZoomCoroutine(float modifier , float duration)
    {
        float normalizedTime = 0;
        if (modifierType == ZoomType.FOV)
        {
            float originValue = GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
            float targetValue = valueType == ValueType.Relative ? originValue * modifier : originValue + modifier;
            while (normalizedTime < 1)
            {
                normalizedTime += Time.deltaTime / duration;
                GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView =
                    Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(normalizedTime));
                yield return null;
            }

            //Reset at end in editor mode
            if (!Application.isPlaying)
            {
                GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = originValue;
            }
        }
        else
        {
            float originValue = transposer.m_CameraDistance;
            float targetValue = valueType == ValueType.Relative ? originValue * modifier : originValue + modifier;
            while (normalizedTime < 1)
            {
                normalizedTime += Time.deltaTime / duration;
                transposer.m_CameraDistance = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(normalizedTime));
                yield return null;
            }

            //Reset at end in editor mode
            if (!Application.isPlaying)
            {
                transposer.m_CameraDistance = originValue;
            }
        }
    }

    #endregion
}
