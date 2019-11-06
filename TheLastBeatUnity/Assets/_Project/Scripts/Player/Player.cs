using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Sirenix.OdinInspector;

public class Player : Inputable
{
    [SerializeField]
    float speedMagnitude;

    [Header("Movement")]
    [SerializeField]
    float speed;
    Vector3 previousPos;

    [Header("Dash")]
    [SerializeField]
    [Tooltip("The longer it is , the longer it take to change frequency")]
    float dashImpactBeatDelay;
    [SerializeField]
    float dashDuration;
    [SerializeField]
    float dashStrength;
    [SerializeField]
    [Tooltip("The evolution of heart beat , must always end at 1")]
    AnimationCurve dashAnimationCurve;

    [Header("References")]
    [SerializeField] [Required]
    Health health;
    [SerializeField] [Required]
    FocusZone focusZone;
    
    IEnumerator currentAction;

    private void Start()
    {
        previousPos = transform.position;
    }

    //If you are doing something (dash , attack animation , etc...) temporary block input
    public override bool BlockInput => (blockInput || currentAction != null);

    public override void ProcessInput(Rewired.Player player)
    {
        previousPos = transform.position;

        Vector3 movement = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));

        // Rotation
        Enemy currentTarget = focusZone.GetCurrentTarget();
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

        if (player.GetButtonDown("Dash") && currentAction == null)
        {
            health.NewAction(1.5f, dashImpactBeatDelay);
            currentAction = Dash(dashDuration);
            StartCoroutine(currentAction);
        }
    }

    IEnumerator Dash(float duration)
    {
        Debug.Assert(duration > 0);
        float normalizedTime = 0;
        while (normalizedTime < 1)
        {
            normalizedTime += Time.deltaTime / duration;
            previousPos = transform.position;
            transform.Translate(transform.forward * Time.deltaTime * dashAnimationCurve.Evaluate(normalizedTime) * dashStrength, Space.World);
            yield return null;
        }
        currentAction = null;
    }
}
