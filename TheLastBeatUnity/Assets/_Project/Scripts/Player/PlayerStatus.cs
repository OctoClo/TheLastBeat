using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

public enum EPlayerStatus
{
    BEGIN,
    DEFAULT,
    RUSHING,
    BLINKING,
    STUNNED
}

public class PlayerStatus : MonoBehaviour
{
    public EPlayerStatus CurrentStatus { get; private set; }

    [TabGroup("Beginning")] [SerializeField]
    float beginPoseDuration = 2;
    [TabGroup("Beginning")] [SerializeField]
    float waitBeforeStartMoving = 2;

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
    private float rushTimer = 0;

    private void Awake()
    {
        CurrentStatus = EPlayerStatus.BEGIN;
        Sequence seq = DOTween.Sequence()
                                .InsertCallback(beginPoseDuration, () => Animator.SetTrigger("getUp"))
                                .InsertCallback(waitBeforeStartMoving, () => CurrentStatus = EPlayerStatus.DEFAULT);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stunDuration -= stunRecoverAnim.length;
    }

    private void Update()
    {
        if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Rush"))
        {
            rushTimer += Time.deltaTime;
            if (rushTimer > 0.5f)
                Animator.SetTrigger("rushEnd");
        }
        else
        {
            rushTimer = 0;
        }
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
            Animator.ResetTrigger("rush");
            Animator.ResetTrigger("rushEnd");
            Animator.SetBool("stunned", false);
            CurrentStatus = EPlayerStatus.DEFAULT;
        }
        
        Animator.ResetTrigger("hitEnd");
        Animator.SetTrigger("hit");
    }

    public void StopHit()
    {
        Animator.ResetTrigger("hit");
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
        stunCoroutine = StartCoroutine(WaitBeforeStunEnd());
    }

    private IEnumerator WaitBeforeStunEnd()
    {
        yield return new WaitForSecondsRealtime(stunRecoverAnim.length);
        CurrentStatus = EPlayerStatus.DEFAULT;
        stunCoroutine = null;
    }
}
