using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Cinemachine;
using DG.Tweening;

[RequireComponent(typeof(Cinemachine.CinemachineVirtualCamera))]
public class CameraEffect : MonoBehaviour
{
    CinemachineVirtualCamera virtualCam;
    Vector3 pivot;

    private void Start()
    {
        LoadRefs();
    }

    void LoadRefs()
    {
        virtualCam = GetComponent<CinemachineVirtualCamera>();
        perlin = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        transposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    #region ScreenShake
    [TabGroup("ScreenShake")] [SerializeField]
    float defaultIntensityScreenShake;

    [TabGroup("ScreenShake")] [SerializeField]
    float defaultScreenShakeDuration;

    [TabGroup("ScreenShake")] [SerializeField]
    AnimationCurve intensityOverTime;

    CinemachineBasicMultiChannelPerlin perlin;

    [TabGroup("ScreenShake")][InfoBox("Ne fonctionne que si le mode solo de la camera est activé", InfoMessageType.None)][Button(ButtonSizes.Medium)]
    void Test()
    {
        LoadRefs();
        StartScreenShake(defaultScreenShakeDuration , defaultIntensityScreenShake);
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
    void TestZoom()
    {
        LoadRefs();
        StartZoom(valueForTest, durationZoom, modifierType , valueType);
    }

    CinemachineFramingTransposer transposer;
    IEnumerator currentZooming;

    public void StartZoom(float modifier, float duration, ZoomType zoomType, ValueType vt)
    {
        modifier = Mathf.Abs(modifier);
        Debug.Assert(modifier > 0, "Zoom cannot be set to 0");

        //Cannot have 2 zooming sequence at the same time
        if (currentZooming != null)
            StopCoroutine(currentZooming);

        currentZooming = ZoomCoroutine(modifier, durationZoom, zoomType , vt);
        StartCoroutine(currentZooming);
    }

    IEnumerator ZoomCoroutine(float modifier , float duration, ZoomType zoomType , ValueType vt)
    {
        float normalizedTime = 0;
        if (zoomType == ZoomType.FOV)
        {
            float originValue = virtualCam.m_Lens.FieldOfView;
            float targetValue = vt == ValueType.Relative ? originValue * modifier : originValue + modifier;
            while (normalizedTime < 1)
            {
                normalizedTime += Time.deltaTime / duration;
                virtualCam.m_Lens.FieldOfView = Mathf.Lerp(originValue, targetValue, zoomOverTime.Evaluate(normalizedTime));
                yield return null;
            }

            //Reset at end in editor mode
            if (!Application.isPlaying)
            {
                virtualCam.m_Lens.FieldOfView = originValue;
            }
        }
        else
        {
            float originValue = transposer.m_CameraDistance;
            float targetValue = vt == ValueType.Relative ? originValue * modifier : originValue + modifier;
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

    #region CameraAngle

    [TabGroup("CameraAngle")][SerializeField]
    float pitchValueTest;

    float angle = 20;
    public float Angle
    {
        get
        {
            return angle;
        }
        set
        {
            angle = value;
            SetPitch(angle);
        }
    }

    [TabGroup("CameraAngle")] [Button(ButtonSizes.Medium,Name = "Set camera pitch (degrees)")] 
    public void Set()
    {
        SetPitch(pitchValueTest);
    }

    public void Interpolate(float to, float duration)
    {
        Transform follow = virtualCam.Follow;
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => transposer.m_YDamping = 8);
        seq.Append(DOTween.To(() => Angle, x => Angle = x, to, duration));
        seq.Append(DOTween.To(() => transposer.m_YDamping, x => transposer.m_YDamping = x, 1, 1f));
        seq.Play();
    }

    public void SetPitch(float angle)
    {
        LoadRefs();
        Transform target = virtualCam.Follow;
        Vector3 previousPosition = transform.position;
        Vector3 tempPosition = target.position - (Vector3.forward * transposer.m_CameraDistance);
        Vector3 finalPosition = RotatePointAroundPivot(tempPosition, virtualCam.Follow.position, Vector3.right * angle);
        transform.position = finalPosition;
        transform.LookAt(target);
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }
    #endregion
}
