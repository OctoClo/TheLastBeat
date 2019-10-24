using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player : MonoBehaviour
{
    [SerializeField]
    float speed;

    [SerializeField]
    float strength;

    [SerializeField]
    float durationDash;

    [SerializeField]
    AnimationCurve ac;

    Rewired.Player player;
    List<PositionModifier> allModifiers = new List<PositionModifier>();

    // Start is called before the first frame update
    void Start()
    {
        player = ReInput.players.GetPlayer(0);
        allModifiers.Add(new Dash(durationDash, strength, ac));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 vec = new Vector3(player.GetAxis("MoveX"), 0, player.GetAxis("MoveY"));
        vec *= Time.deltaTime * speed;
        transform.Translate(vec);

        if (player.GetButtonDown("Dash"))
        {
            Debug.Log("Dash");
            allModifiers[0].StartModifier();
        }

        foreach(PositionModifier pm in allModifiers)
        {
            if (pm.IsActive)
            {
                pm.ApplyDelta(transform, Time.deltaTime);
            }
        }
    }
}
