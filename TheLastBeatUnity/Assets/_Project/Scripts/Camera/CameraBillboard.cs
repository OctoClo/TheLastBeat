using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraBillboard : MonoBehaviour
{
    CinemachineStateDrivenCamera driven;

    private void Start()
    {
        if (CameraManager.Instance)
            driven = CameraManager.Instance.StateDrive;
    }

    void LateUpdate()
    {
        GameObject cam = driven.LiveChild.VirtualCameraGameObject;
        if (cam != null)
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
    }
}
