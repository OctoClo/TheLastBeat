using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.SceneManagement;

public enum EPlayerStatus
{
    BEGIN,
    DEFAULT,
    RUSHING,
    BLINKING,
    STUNNED,
    DYING
}

public class PlayerStatus : MonoBehaviour
{
    public EPlayerStatus CurrentStatus { get; private set; }

    [TabGroup("Beginning")] [SerializeField]
    bool launchIntro = false;
    [TabGroup("Beginning")] [SerializeField]
    float fadeDuration = 0.5f;
    [TabGroup("Beginning")] [SerializeField]
    float beginPoseDuration = 2;
    [TabGroup("Beginning")] [SerializeField]
    float waitBeforeStartMoving = 2;
    [TabGroup("Beginning")] [SerializeField]
    GameObject monolithCheckpoint = null;
    Rock[] checkpointRocks = null;

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

    [TabGroup("Death")] [SerializeField]
    float deathAnimDuration = 1.5f;
    [TabGroup("Death")] [SerializeField] [Range(0, 3)]
    float dissolveDuration = 1f;
    [TabGroup("Death")] [SerializeField]
    Material[] dissolveMats;
    [TabGroup("Death")] [SerializeField]
    float screenshakeIntensity = 2;

    [SerializeField]
    Animator animator = null;

    private Coroutine stunCoroutine = null;
    private Rigidbody rb = null;
    private float rushTimer = 0;

    private void Awake()
    {
        animator.SetBool("intro", launchIntro);
        checkpointRocks = monolithCheckpoint.GetComponentsInChildren<Rock>();
        
        if (launchIntro)
        {
            CurrentStatus = EPlayerStatus.BEGIN;
            foreach (Rock rock in checkpointRocks)
                rock.ChangeState(ERockState.ILLUMINATE);
            SceneHelper.Instance.StartFade(() => GetUp(), fadeDuration, new Color(0, 0, 0, 0), true, true);
        }
        else
            CurrentStatus = EPlayerStatus.DEFAULT;
    }

    private void GetUp()
    {
        Sequence seq = DOTween.Sequence()
                                .InsertCallback(beginPoseDuration, () => animator.SetTrigger("getUp"))
                                .InsertCallback(waitBeforeStartMoving, () =>
                                {
                                    CurrentStatus = EPlayerStatus.DEFAULT;
                                    foreach (Rock rock in checkpointRocks)
                                        rock.ChangeState(ERockState.PULSE_ON_BEAT);
                                });
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        stunDuration -= stunRecoverAnim.length;
    }

    private void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Rush"))
        {
            rushTimer += Time.deltaTime;
            if (rushTimer > 0.5f)
                animator.SetTrigger("rushEnd");
        }
        else
        {
            rushTimer = 0;
        }
    }

    public void SetMoving(bool moving)
    {
        animator.SetBool("moving", moving);
    }

    public void StartRushing()
    {
        CurrentStatus = EPlayerStatus.RUSHING;
        animator.SetTrigger("rush");
    }

    public void StopRushing()
    {
        CurrentStatus = EPlayerStatus.DEFAULT;
        animator.SetTrigger("rushEnd");
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
            animator.ResetTrigger("rush");
            animator.ResetTrigger("rushEnd");
            animator.SetBool("stunned", false);
            CurrentStatus = EPlayerStatus.DEFAULT;
        }
        
        animator.ResetTrigger("hitEnd");
        animator.SetTrigger("hit");
    }

    public void StopHit()
    {
        animator.ResetTrigger("hit");
        animator.SetTrigger("hitEnd");
    }

    public void GetStunned(Vector3 kickbackDirection)
    {
        CurrentStatus = EPlayerStatus.STUNNED;
        stunMusicSXF.Post(gameObject);
        rb.AddForce(kickbackDirection * kickbackForce, ForceMode.VelocityChange);
        animator.SetBool("stunned", true);
        stunCoroutine = StartCoroutine(WaitBeforeAnimStunEnd());
    }

    private IEnumerator WaitBeforeAnimStunEnd()
    {
        yield return new WaitForSecondsRealtime(stunDuration);
        animator.SetBool("stunned", false);
        stunCoroutine = StartCoroutine(WaitBeforeStunEnd());
    }

    private IEnumerator WaitBeforeStunEnd()
    {
        yield return new WaitForSecondsRealtime(stunRecoverAnim.length);
        CurrentStatus = EPlayerStatus.DEFAULT;
        stunCoroutine = null;
    }

    public void DieAnimation()
    {
        CurrentStatus = EPlayerStatus.DYING;
        SetMoving(false);
        animator.SetTrigger("die");
        DOTween.Sequence().InsertCallback(deathAnimDuration, () => DiePart2());
    }

    public void DiePart2()
    {
        foreach (Material mat in dissolveMats)
        {
            mat.SetFloat("_BeginTime", Time.timeSinceLevelLoad);
            mat.SetFloat("_DissolveDuration", dissolveDuration);
        }

        SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        renderer.receiveShadows = false;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.materials = dissolveMats;

        foreach (CameraEffect ce in CameraManager.Instance.AllCameras)
            ce.StartScreenShake(dissolveDuration, screenshakeIntensity);

        float dissolveHalfDuration = dissolveDuration * 0.5f;
        DOTween.Sequence().InsertCallback(dissolveHalfDuration,
            () => SceneHelper.Instance.StartFade(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name), dissolveHalfDuration + 1, Color.black));
    }
}
