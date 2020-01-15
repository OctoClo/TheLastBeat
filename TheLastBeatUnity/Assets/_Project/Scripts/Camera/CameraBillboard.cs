using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraBillboard : MonoBehaviour
{
    CinemachineVirtualCamera cam;

    private void OnEnable()
    {
        EventManager.Instance.AddListener<ChangeCameraEvent>(OnChangeCameraEvent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<ChangeCameraEvent>(OnChangeCameraEvent);
    }

    private void Start()
    {
        if (CameraManager.Instance)
            cam = CameraManager.Instance.LiveCamera;
    }

    void OnChangeCameraEvent(ChangeCameraEvent e)
    {
        cam = CameraManager.Instance.LiveCamera;
    }

    void LateUpdate()
    {
        if (cam)
            transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
    }
}
