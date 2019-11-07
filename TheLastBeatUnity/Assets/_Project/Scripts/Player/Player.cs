using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Sirenix.OdinInspector;
using DG.Tweening;

public class Player : Inputable
{
    [SerializeField]
    float speedMagnitude;

    [Header("Movement")]
    [SerializeField]
    float speed;
    Vector3 previousPos;

    [Header("Dash")]
    [SerializeField] [Tooltip("The longer it is, the longer it take to change frequency")]
    float dashImpactBeatDelay;
    [SerializeField] [ValidateInput("Positive", "This value must be > 0")]
    float dashDuration;
    [SerializeField] [Tooltip("The evolution of heart beat , must always end at 1")]
    AnimationCurve dashAnimationCurve;
    bool dashing;
    [SerializeField]
    float slowMotionDuration;

    [Header("References")]
    [SerializeField] [Required]
    Health health;
    [SerializeField] [Required]
    FocusZone focusZone;
    
    IEnumerator currentAction;
    Enemy currentTarget;

    public bool Positive(float value)
    {
        return value > 0;
    }

    private void Start()
    {
        TimeManager.Instance.SetPlayer(this);

        previousPos = transform.position;
        dashing = false;
    }

    //If you are doing something (dash , attack animation , etc...) temporary block input
    public override bool BlockInput => (blockInput || dashing);

    public override void ProcessInput(Rewired.Player player)
    {
        previousPos = transform.position;

        Vector3 movement = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));

        // Rotation
        currentTarget = focusZone.GetCurrentTarget();
        if (currentTarget)
            transform.forward = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z) - transform.position;
        else if (movement != Vector3.zero)
        {
            Vector3 direction = movement;
            direction.Normalize();
            transform.forward = direction;
        }

        // Translation
        movement *= Time.deltaTime * speed;
        transform.Translate(movement, Space.World);

        if (player.GetButtonDown("Dash") && !dashing && currentTarget)
            Dash();
    }

    void Dash()
    {
        dashing = true;
        AkSoundEngine.PostEvent("DashFX", gameObject);
        health.NewAction(1.5f, dashImpactBeatDelay);
        TimeManager.Instance.SlowEnemies();
        gameObject.layer = LayerMask.NameToLayer("Player Dashing");

        Sequence seq = DOTween.Sequence();

        Vector3 goalPosition = new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z) - transform.position;
        goalPosition *= 1.3f;
        goalPosition += transform.position;
        seq.Append(transform.DOMove(goalPosition, dashDuration));

        seq.AppendCallback(() =>
        {
            dashing = false;
            TimeManager.Instance.ResetEnemies();
            currentTarget.GetAttacked();
            gameObject.layer = LayerMask.NameToLayer("Default");

            Time.timeScale = 0.1f;
            StartCoroutine(WaitDuringSlowMotion());
        });
        
        seq.Play();
    }

    IEnumerator WaitDuringSlowMotion()
    {
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1;
    }
}
