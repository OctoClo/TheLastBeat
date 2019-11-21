using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    CinemachineStateDrivenCamera stateDrive;
    Animator anim;

    void Start()
    {
        stateDrive = GetComponent<CinemachineStateDrivenCamera>();
        if (stateDrive)
            anim = stateDrive.m_AnimatedTarget;

        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void CameraStateChange(string triggerName)
    {
        Debug.Log(triggerName);
        anim.SetTrigger(triggerName);
    }

}
