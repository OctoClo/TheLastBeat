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

    public void SetRushing(bool value)
    {
        animator.SetBool("rush", value);
    }

    public void SetMovement(Vector3 movement)
    {
        animator.SetBool("moving", (movement != Vector3.zero));    
    }
}
