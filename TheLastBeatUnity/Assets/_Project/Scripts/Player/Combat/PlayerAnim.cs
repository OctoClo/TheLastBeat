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
    Animator animator = null;
    public Animator Animator => animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void LaunchAnim(EPlayerAnim anim)
    {
        switch (anim)
        {
            case EPlayerAnim.RUSHING:
                animator.SetTrigger("rush");
                break;
        }
    }

    public void SetMovement(Vector3 movement)
    {
        animator.SetBool("moving", (movement != Vector3.zero));    
    }
}
