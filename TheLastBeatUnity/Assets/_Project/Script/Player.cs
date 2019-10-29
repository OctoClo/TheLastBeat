using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player : Inputable
{
    [Header("Movement")]
    [SerializeField]
    float speed;
    Vector3 previousPos;

    [Header("Dash")]
    [SerializeField]
    float dashDuration;
    [SerializeField]
    float dashStrength;
    [SerializeField]
    AnimationCurve dashAnimationCurve;

    IEnumerator currentAction;
    Camera cam;

    private void Start()
    {
        previousPos = transform.position;
        cam = Camera.main;
    }

    //If you are doing something (dash , attack animation , etc...) temporary block input
    public override bool BlockInput => currentAction != null;

    public override void ProcessInput(Rewired.Player player)
    {
        previousPos = transform.position;

        Vector3 movement = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        movement *= Time.deltaTime * speed;
        transform.Translate(movement, Space.World);
        
        if (movement != Vector3.zero)
            transform.forward = movement;

        if (player.GetButtonDown("Dash") && currentAction == null)
        {
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

    void Update()
    {
        cam.transform.Translate(transform.position - previousPos, Space.World);
    }

    private void OnTriggerStay(Collider other)
    {
        if (ReInput.players.GetPlayer(0).GetButtonDown("Attack"))
        {
            if (other.CompareTag("Enemy"))
                other.GetComponent<Enemy>().GetAttacked();
        }
    }
}
