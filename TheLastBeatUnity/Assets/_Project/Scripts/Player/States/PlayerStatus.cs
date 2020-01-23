using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum EPlayerStatus
{
    DEFAULT,
    RUSHING,
    BLINKING,
    STUNNED
}

public class PlayerStatus : MonoBehaviour
{
    public EPlayerStatus CurrentStatus { get; private set; }

    [TabGroup("Stun")] [SerializeField]
    float kickbackForce = 1.5f;
    [TabGroup("Stun")] [SerializeField]
    float stunDuration = 0.5f;
    [TabGroup("Stun")] [SerializeField]
    AnimationClip stunRecoverAnim = null;
    [TabGroup("Stun")] [SerializeField]
    Color stunColor = Color.blue;
    Color normalColor = Color.white;
    [TabGroup("Stun")] [SerializeField]
    AK.Wwise.Event stunMusicSXF = null;

    public Animator Animator = null;
    private Coroutine stunCoroutine = null;
    private Rigidbody rb = null;

    private void Awake()
    {
        CurrentStatus = EPlayerStatus.DEFAULT;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stunDuration -= stunRecoverAnim.length;
    }

    public void SetMoving(bool moving)
    {
        Animator.SetBool("moving", moving);
    }

    public void StartRushing()
    {
        CurrentStatus = EPlayerStatus.RUSHING;
        Animator.SetTrigger("rush");
    }

    public void StopRushing()
    {
        CurrentStatus = EPlayerStatus.DEFAULT;
        Animator.SetTrigger("rushEnd");
    }

    public void StartBlink()
    {
        CurrentStatus = EPlayerStatus.BLINKING;
    }

    public void StopBlink()
    {
        CurrentStatus = EPlayerStatus.DEFAULT;
    }

    public void GetHit()
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            Animator.SetBool("stunned", false);
            CurrentStatus = EPlayerStatus.DEFAULT;
        }
        Animator.SetTrigger("hit");
    }

    public void StopHit()
    {
        Animator.SetTrigger("hitEnd");
    }

    public void GetStunned(Vector3 kickbackDirection)
    {
        CurrentStatus = EPlayerStatus.STUNNED;
        stunMusicSXF.Post(gameObject);
        rb.AddForce(kickbackDirection * kickbackForce, ForceMode.VelocityChange);
        Animator.SetBool("stunned", true);
        stunCoroutine = StartCoroutine(WaitBeforeAnimStunEnd());
    }

    private IEnumerator WaitBeforeAnimStunEnd()
    {
        yield return new WaitForSecondsRealtime(stunDuration);
        Animator.SetBool("stunned", false);
        StartCoroutine(WaitBeforeStunEnd());
    }

    private IEnumerator WaitBeforeStunEnd()
    {
        yield return new WaitForSecondsRealtime(stunRecoverAnim.length);
        CurrentStatus = EPlayerStatus.DEFAULT;
    }
}
