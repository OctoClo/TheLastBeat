using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using Cinemachine;

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
    }

    #region CameraAngle
    CinemachineVirtualCamera virtualCam;
    CinemachineFramingTransposer transposer;
    CinemachineCameraOffset offset;

    [TabGroup("CameraAngle")]
    [SerializeField]
    float pitchValueTest;

    [TabGroup("CameraAngle")]
    [SerializeField]
    string tagContains;

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

    [TabGroup("CameraAngle")]
    [Button(ButtonSizes.Medium, Name = "Set camera pitch (degrees)")]
    public void Set()
    {
        SetPitch(pitchValueTest);
    }

    public void Interpolate(float to, float duration)
    {
        Transform follow = virtualCam.Follow;
        Sequence seq = DOTween.Sequence();
        seq.Append(DOTween.To(() => Angle, x => Angle = x, to, duration));
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

    #region Offset
    //Since how long a movement is occuring, X = right , Y = up (negative value = opposide side)
    Vector2 movement = new Vector2();

    [TabGroup("Offset")][SerializeField]
    Player player;

    [TabGroup("Offset")][SerializeField]
    float maxOffsetDuration;

    [TabGroup("Offset")][SerializeField]
    float decayPerSecond;

    [TabGroup("Offset")][SerializeField]
    Vector2 maxRatio;

    Vector2 offsetValueMax;
    float ratio;

    void Update()
    {
        Vector3 delta = player.DeltaMovement;
        InterpretMovement(new Vector2(delta.x, delta.z));
        Decay(delta);
        offset.m_Offset = new Vector3(offsetValueMax.x * movement.x * maxRatio.x, offsetValueMax.y * movement.y * maxRatio.y, 0);
    }

    void InterpretMovement(Vector2 value)
    {
        float potentialX = (Mathf.Sign(value.x) * Time.deltaTime / maxOffsetDuration);
        float potentialY = (Mathf.Sign(value.y) * Time.deltaTime / maxOffsetDuration);

        if (Mathf.Abs(value.x) > 0.0001f)
        {
            movement += new Vector2(potentialX, 0);
        }

        if (Mathf.Abs(value.y) > 0.0001f)
        {
            potentialY = Mathf.Max(0.041f, Mathf.Abs(potentialY)) * Mathf.Sign(potentialY);
            movement += new Vector2(0, potentialY);
        }

        movement = new Vector2(Mathf.Clamp(movement.x, -1, 1), Mathf.Clamp(movement.y, -1, 1));
    }

    void Decay(Vector2 value)
    {
        float tempX = movement.x;
        if (value.x == 0 && tempX != 0)
        {
            tempX += (decayPerSecond * Time.deltaTime * -Mathf.Sign(movement.x));
            if (tempX * movement.x < 0)
            {
                tempX = 0;
            }
        }

        float tempY = movement.y;
        if (value.y == 0 && tempY != 0)
        {       
            tempY += (decayPerSecond * Time.deltaTime * -Mathf.Sign(movement.y));
            if (tempY * movement.y < 0)
            {
                tempY = 0;
            }
        }

        Vector2 previous = movement;
        movement = new Vector2(tempX, tempY);
    }
    #endregion
}
