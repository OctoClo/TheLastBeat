using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;
using DG.Tweening;

public class ChangeCameraEvent : GameEvent {}

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [SerializeField]
    CinemachineVirtualCamera cameraOutOfCombat = null;
    [SerializeField]
    CinemachineVirtualCamera cameraInCombat = null;
    [SerializeField]
    CinemachineStateDrivenCamera stateDrive = null;
    Animator anim = null;

    public CinemachineVirtualCamera LiveCamera { get; private set; }
    public CameraEffect[] AllCameras => GameObject.FindObjectsOfType<CameraEffect>();
    public bool InCombat { get; private set; }

    List<float> defaultBlendingTime = new List<float>();

    void Start()
    {
        if (Instance == null)
            Instance = this;
        
        if (stateDrive)
        {
            anim = stateDrive.m_AnimatedTarget;
            defaultBlendingTime = stateDrive.m_CustomBlends.m_CustomBlends.Select(x => x.m_Blend.m_Time).ToList();
        }
        
        InCombat = false;
        ChangeCamera();
    }

    public void CameraStateChange(string triggerName)
    {
        anim.SetTrigger(triggerName);
        InCombat = !InCombat;
        ChangeCamera();
    }

    public void SetBoolCamera(bool value , string paramName)
    {
        anim.SetBool(paramName, value);
    }

    private void ChangeCamera()
    {
        LiveCamera = (InCombat ? cameraInCombat : cameraOutOfCombat);
        EventManager.Instance.Raise(new ChangeCameraEvent());
    }

    public void SetBlend(string from , string to , float tempValue)
    {
        List<CinemachineBlenderSettings.CustomBlend> blends = stateDrive.m_CustomBlends.m_CustomBlends.Where(x => x.m_From == from && x.m_To == to).ToList();
        if (blends.Count > 0)
        {
            int indexList = stateDrive.m_CustomBlends.m_CustomBlends.ToList().IndexOf(blends[0]);
            CinemachineBlenderSettings.CustomBlend blending = blends[0];
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                blending.m_Blend.m_Time = tempValue;
                stateDrive.m_CustomBlends.m_CustomBlends[indexList] = blending;
            });
            seq.AppendInterval(tempValue * 2);
            seq.AppendCallback(() =>
            {
                blending.m_Blend.m_Time = defaultBlendingTime[indexList];
                stateDrive.m_CustomBlends.m_CustomBlends[indexList] = blending;
            });
            seq.Play();
        }
    }
}
