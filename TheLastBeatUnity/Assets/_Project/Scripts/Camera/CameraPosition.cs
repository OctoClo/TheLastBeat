using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using Cinemachine;

[RequireComponent(typeof(CinemachineCameraOffset))]
[RequireComponent(typeof(CinemachineFramingTransposer))]
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraPosition : MonoBehaviour
{
    void LoadRefs()
    {
        virtualCam = GetComponent<CinemachineVirtualCamera>();
        transposer = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        offset = GetComponent<CinemachineCameraOffset>();
        ratio = Camera.main.aspect;
        offsetValueMax = new Vector2(virtualCam.m_Lens.OrthographicSize, virtualCam.m_Lens.OrthographicSize / ratio);
    }

    private void Start()
    {
        LoadRefs();
        sequencePitchControl = DOTween.Sequence();
        sequencePitchControl.AppendCallback(() => CheckOcclusionToPlayer());
        sequencePitchControl.AppendInterval(0.2f);
        sequencePitchControl.SetLoops(-1);
        sequencePitchControl.Play();

        Angle = pitchValueTest;
    }

    #region CameraAngle
    CinemachineVirtualCamera virtualCam;
    CinemachineFramingTransposer transposer;
    CinemachineCameraOffset offset;

    [TabGroup("CameraAngle")][SerializeField]
    float pitchValueTest;

    [TabGroup("CameraAngle")][SerializeField]
    float minAngle;

    [TabGroup("CameraAngle")][SerializeField]
    float maxAngle;

    [TabGroup("CameraAngle")][SerializeField]
    string tagContains;

    [TabGroup("CameraAngle")] [SerializeField]
    float durationPitchTransition;

    IEnumerator interpolation;

    float angle;

    public float Angle
    {
        get
        {
            return angle;
        }
        set
        {
            if (virtualCam == null)
                virtualCam = GetComponent<CinemachineVirtualCamera>();
            if (virtualCam.Follow)
            {
                Debug.Assert(minAngle < maxAngle, "minAngle greater than maxAngle");
                angle = Mathf.Clamp(value, minAngle, maxAngle);
                SetPitch(angle);
            }         
        }
    }

    Sequence sequencePitchControl;

    [TabGroup("CameraAngle")]
    [Button(ButtonSizes.Medium, Name = "Set camera pitch (degrees)")]
    public void Set()
    {
        SetPitch(pitchValueTest);
    }

    IEnumerator InterpolationCoroutine(float from , float to , float duration)
    {
        float normalizedTime = 0;
        while (normalizedTime < 1)
        {
            normalizedTime += Time.deltaTime / duration;
            Angle = Mathf.Lerp(from, to, normalizedTime);
            yield return null;
        }
        interpolation = null;
    }

    public void Interpolate(float to, float duration)
    {
        interpolation = InterpolationCoroutine(Angle, to, duration);
        StartCoroutine(interpolation);
    }

    public void SetPitch(float angle)
    {
        if (!Application.isPlaying)
            LoadRefs();

        Vector3 finalPosition = GetPitch(angle);
        transform.position = finalPosition;
        transform.LookAt(virtualCam.Follow);
    }

    public Vector3 GetPitch(float angle)
    {
        Transform target = virtualCam.Follow;
        Vector3 previousPosition = transform.position;
        Vector3 tempPosition = target.position - (Vector3.forward * transposer.m_CameraDistance);
        return RotatePointAroundPivot(tempPosition, virtualCam.Follow.position, Vector3.right * angle);
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    public void CheckOcclusionToPlayer()
    {
        if (IsSomethingBlocking(GetPitch(Angle)))
        {
            float newAngle = GetNonBlockingAngle();
            if (newAngle > 0 && interpolation == null)
            {
                Interpolate(newAngle, durationPitchTransition);
            }
        }
    }


    bool IsSomethingBlocking(Vector3 initialPoint)
    {
        Transform target = virtualCam.Follow;
        Ray ray = new Ray(initialPoint, target.position - initialPoint);

        foreach(RaycastHit hit in Physics.RaycastAll(ray , Vector3.Distance(target.position , initialPoint)))
        {
            if (hit.collider.tag.Contains(tagContains))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Get the smallest angle relative to the current angle with no obstacle , return -1 if not found
    /// </summary>
    /// <returns></returns>
    float GetNonBlockingAngle()
    {
        Debug.Assert(angle < maxAngle && angle > minAngle, "Angle out of bound");
        float maxOutput = angle;
        for (float higherAngle = angle + 1; higherAngle < maxAngle; higherAngle++)
        {
            if (!IsSomethingBlocking(GetPitch(higherAngle)))
            {
                maxOutput = higherAngle;
                break;
            }
        }

        float minOutput = angle;
        for (float lowerAngle = angle - 1; lowerAngle > minAngle; lowerAngle++)
        {
            if (!IsSomethingBlocking(GetPitch(lowerAngle)))
            {
                minOutput = lowerAngle;
                break;
            }
        }
        
        //No valid angle found
        if (maxOutput == angle && minOutput == angle)
            return -1;

        if (Mathf.Abs(maxOutput - angle) <= Mathf.Abs(minOutput - angle))
            return maxOutput;
        else
            return minOutput;
    }

    #endregion

    #region Offset
    //Normalized values of offset
    Vector2 movement = new Vector2();

    [TabGroup("Offset")][SerializeField]
    Vector2 maxRatio;

    [TabGroup("Offset")][SerializeField]
    AnimationCurve cameraSmoothing;

    //Automaticly sampled
    Vector2 offsetValueMax;
    float ratio;

    public void Move(float ratioX , float ratioY)
    {
        offset.m_Offset = new Vector3(offsetValueMax.x * cameraSmoothing.Evaluate(ratioX) * maxRatio.x, offsetValueMax.y * cameraSmoothing.Evaluate(ratioY) * maxRatio.y);
    }

    #endregion
}
