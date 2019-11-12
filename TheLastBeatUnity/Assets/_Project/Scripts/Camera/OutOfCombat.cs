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

    [SerializeField]
    float maxOffsetDuration;

    [SerializeField]
    float decayPerSecond;

    float movementX;
    float movementY;

    public override void StateEnter()
    {
    }

    public override void StateExit()
    {

    }

    public override void StateUpdate()
    {
        Vector2 vec = new Vector2(player.DeltaMovement.x, player.DeltaMovement.z);
        InterpretMovement(vec);
        Decay(vec);
        cameraPos.Move(movementX, movementY);
    }

    public void InterpretMovement(Vector2 value)
    {
        float potentialX = (Mathf.Sign(value.x) * Time.deltaTime / maxOffsetDuration);
        float potentialY = (Mathf.Sign(value.y) * Time.deltaTime / maxOffsetDuration);

        if (Mathf.Abs(value.x) > 0.0001f)
        {
            movementX += potentialX;
        }

        if (Mathf.Abs(value.y) > 0.0001f)
        {
            movementY += potentialY;
        }

        movementX = Mathf.Clamp(movementX, -1, 1);
        movementY = Mathf.Clamp(movementY, -1, 1);
    }

    /// <summary>
    /// Allow to reduce the offset if the player dont go in any direction
    /// </summary>
    /// <param name="value"></param>
    public void Decay(Vector2 value)
    {
        float tempX = movementX;
        if (value.x == 0 && tempX != 0)
        {
            tempX += (decayPerSecond * Time.deltaTime * -Mathf.Sign(movementX));
            if (tempX * movementX < 0)
            {
                tempX = 0;
            }
        }

        float tempY = movementY;
        if (value.y == 0 && tempY != 0)
        {
            tempY += (decayPerSecond * Time.deltaTime * -Mathf.Sign(movementY) * Mathf.Abs(movementY / movementX));
            if (tempY * movementY < 0)
            {
                tempY = 0;
            }
        }

        movementY = tempY;
        movementX = tempX;
    }
}
