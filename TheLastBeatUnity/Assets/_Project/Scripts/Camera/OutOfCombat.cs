using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfCombat : CameraState
{
    // Start is called before the first frame update
    [SerializeField]
    Player player;

    [SerializeField]
    CameraPosition cameraPos;

    public override void OnStateEnter()
    {

    }

    public override void OnStateExit()
    {

    }

    public override void OnStateUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 vec = new Vector2(player.DeltaMovement.x , player.DeltaMovement.z);
        cameraPos.InterpretMovement(vec);
        cameraPos.Decay(vec);
        cameraPos.Move();
    }
}
