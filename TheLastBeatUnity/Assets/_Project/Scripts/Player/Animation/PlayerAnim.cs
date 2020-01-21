using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPlayerAnim
{
    IDLE,
    MOVING,
    RUSHING,
    BLINKING
}

public class PlayerAnim : MonoBehaviour
{
    public Animator Animator = null;

    public void SetMoving(bool moving)
    {
        Animator.SetBool("moving", moving);
    }

    public void SetRushing(bool value)
    {
        Animator.SetBool("rush", value);
    }

    public void GetHit()
    {
        Animator.SetTrigger("hit");
    }

    public void EndInvicibility()
    {
        Animator.SetTrigger("hitEnd");
    }
}
