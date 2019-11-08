using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraState : MonoBehaviour
{
    protected CameraMachine machine;
    protected bool started = false;

    public abstract void StateEnter();
    public abstract void StateExit();
    public abstract void StateUpdate();
    public void SetMachine(CameraMachine mach)
    {
        machine = mach;
    }
}
