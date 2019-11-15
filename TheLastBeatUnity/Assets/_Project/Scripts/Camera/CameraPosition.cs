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
    }

    private void Start()
    {
        LoadRefs();
    }

    #region CameraAngle
    CinemachineVirtualCamera virtualCam;
    CinemachineFramingTransposer transposer;
    CinemachineCameraOffset offset;

    [SerializeField]
    readonly float minAngle = 0;

    [SerializeField]
    readonly float maxAngle = 0;

    [SerializeField]
    readonly string tagContains = "";

    IEnumerator interpolation = null;

    float angle = 0;

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

    Sequence interpolationSequence;

    public void Interpolate(float to, float duration)
    {
        if (interpolationSequence != null && interpolationSequence.IsPlaying())
            interpolationSequence.Kill();

        interpolationSequence = DOTween.Sequence();
        interpolationSequence.Append(DOTween.To(() => Angle, x => Angle = x, to, duration));
        interpolationSequence.Play();
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

    public void CheckOcclusionToPlayer(float defaultAngle)
    {
        if (IsSomethingBlocking(GetPitch(Angle)))
        {
            float newAngle = GetNonBlockingAngle();
            if (newAngle > 0 && interpolation == null)
            {
                Interpolate(newAngle, 0.5f);
            }
        }
        else
        {
            if (defaultAngle != Angle && !IsSomethingBlocking(GetPitch(defaultAngle)))
            {
                Interpolate(defaultAngle, 0.5f);
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
        bool foundMax = false;
        bool foundMin = false;
        for (float higherAngle = angle + 1; higherAngle < maxAngle; higherAngle++)
        {
            if (!IsSomethingBlocking(GetPitch(higherAngle)))
            {
                foundMax = true;
                maxOutput = higherAngle;
                break;
            }
        }

        float minOutput = angle;
        for (float lowerAngle = angle - 1; lowerAngle > minAngle; lowerAngle--)
        {
            if (!IsSomethingBlocking(GetPitch(lowerAngle)))
            {
                foundMin = true;
                minOutput = lowerAngle;
                break;
            }
        }
        
        //No valid angle found
        if (!foundMax && !foundMin)
            return -1;

        if (foundMax)
        {
            //Both found an angle , get the smallest
            if (foundMin)
            {
                if (Mathf.Abs(maxOutput - angle) <= Mathf.Abs(minOutput - angle))
                    return maxOutput;
                else
                    return minOutput;
            }
            else
            {
                return maxOutput;
            }
        }
        else
        {
            return minOutput;
        }
    }

    #endregion
}
