using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public abstract class Inputable : MonoBehaviour
{
    public abstract void ProcessInput(Rewired.Player player);

    private bool blockInputBeforePause;
    protected bool blockInput = false;
    public virtual bool BlockInput => blockInput;

    private void OnEnable()
    {
        EventManager.Instance.AddListener<PauseEvent>(OnPauseEvent);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveListener<PauseEvent>(OnPauseEvent);
    }

    private void OnPauseEvent(PauseEvent e)
    {
        if (e.pause)
        {
            blockInputBeforePause = blockInput;
            blockInput = true;
        }
        else
            blockInput = blockInputBeforePause;
    }
}
