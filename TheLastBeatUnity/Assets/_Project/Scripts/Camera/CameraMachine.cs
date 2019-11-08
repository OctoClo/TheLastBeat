using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraMachine : MonoBehaviour
{
    CameraState currentState;

    [SerializeField]
    CameraState firstState;

    public CinemachineVirtualCamera virtualCam => GetComponent<CinemachineVirtualCamera>();

    private void Start()
    {
        SetState(firstState);
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.StateUpdate();
        }
    }

    public void SetState(CameraState state)
    {
        if (currentState != null)
        {
            currentState.StateExit();
            currentState.SetMachine(null);
        }
            
        currentState = state;
        currentState.SetMachine(this);
        currentState.StateEnter();
    }


}
