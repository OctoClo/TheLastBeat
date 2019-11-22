using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    Animator anim;

    [SerializeField]
    ICinemachineCamera combatCamera;

    [SerializeField]
    CinemachineStateDrivenCamera stateDrive;

    public bool InCombat => stateDrive.LiveChild == combatCamera;

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
}
