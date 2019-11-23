using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    Animator anim;

    [SerializeField]
    ICinemachineCamera combatCamera;

    [SerializeField]
    CinemachineStateDrivenCamera stateDrive;

    public bool InCombat => stateDrive.LiveChild == combatCamera;
    public CinemachineVirtualCamera LiveCamera { get; private set; }

    void Start()
    {
        if (stateDrive)
            anim = stateDrive.m_AnimatedTarget;

        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void CameraStateChange(string triggerName)
    {
        anim.SetTrigger(triggerName);
    }

    public void NewLiveCamera(ICinemachineCamera newCam , ICinemachineCamera oldCam)
    {
        LiveCamera = newCam as CinemachineVirtualCamera;
    }
}
