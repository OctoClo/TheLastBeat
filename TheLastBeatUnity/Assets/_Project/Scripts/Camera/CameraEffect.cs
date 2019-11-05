using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cinemachine;

[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))] [ExecuteInEditMode]
public class CameraEffect : MonoBehaviour
{
    [TabGroup("ScreenShake")] [SerializeField]
    float intensityScreenShake;

    [TabGroup("ScreenShake")] [SerializeField]
    float screenShakeDuration;

    [TabGroup("ScreenShake")] [SerializeField]
    AnimationCurve intensityOverTime;

    CinemachineBasicMultiChannelPerlin perlin;

    public void StartScreenShake()
    {
        StartCoroutine(ScreenShakeSequence(screenShakeDuration));
    }

    private void Start()
    {
        perlin = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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
}
