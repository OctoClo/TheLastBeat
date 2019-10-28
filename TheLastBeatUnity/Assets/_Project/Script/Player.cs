using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player : Inputable
{
    [SerializeField]
    float speed;

    [SerializeField]
    float strength;

    [SerializeField]
    float durationDash;

    [SerializeField]
    AnimationCurve ac;

    Vector3 previousPos;
    IEnumerator currentAction;

    //If you are doing something (dash , attack animation , etc...) temporary block input
    public override bool BlockInput => currentAction != null;

    public override void ProcessInput(Rewired.Player player)
    {
        previousPos = transform.position;
        Vector3 vec = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        vec *= Time.deltaTime * speed;
        transform.Translate(vec, Space.World);
        transform.forward = transform.position - previousPos;

        if (player.GetButtonDown("Dash") && currentAction == null)
        {
            currentAction = Dash(durationDash);
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
            transform.Translate(transform.forward * Time.deltaTime * ac.Evaluate(normalizedTime) * strength, Space.World);
            yield return null;
        }
        currentAction = null;
    }

    void Update()
    {
        //Horrible ! just for proto
        Camera.main.transform.Translate(transform.position - previousPos, Space.World);
    }
}
