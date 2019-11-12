using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraState : MonoBehaviour
{
    public abstract void OnStateEnter();
    public abstract void OnStateExit();
    public abstract void OnStateUpdate();
}
