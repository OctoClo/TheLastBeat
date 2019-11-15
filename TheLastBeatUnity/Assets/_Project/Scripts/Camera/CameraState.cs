using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraState : MonoBehaviour
{
    protected CameraMachine machine = null;
    protected bool started = false;

    [SerializeField]
    CameraProfile profile = null;
    public CameraProfile Profile => profile;

    public virtual void StateEnter() { }
    public virtual void StateExit() { }
    public virtual void StateUpdate() { }
    public void SetMachine(CameraMachine mach)
    {
        machine = mach;
    }
}
