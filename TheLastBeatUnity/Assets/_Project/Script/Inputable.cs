using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public abstract class Inputable : MonoBehaviour
{
    public abstract void ProcessInput(Rewired.Player player);
    protected bool blockInput = false;
    public virtual bool BlockInput => blockInput;
}
