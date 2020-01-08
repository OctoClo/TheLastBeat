using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;

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
    public bool InCombat { get; private set; }

    void Start()
    {
        if (Instance == null)
            Instance = this;
        
        if (stateDrive)
            anim = stateDrive.m_AnimatedTarget;
        
        InCombat = false;
        ChangeCamera();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            SetBoolCamera(!anim.GetBool("FOV"), "FOV");
        }
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
}
