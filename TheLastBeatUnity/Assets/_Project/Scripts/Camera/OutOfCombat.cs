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

    Vector2 movement = new Vector2();

    public override void StateEnter()
    {

    }

    public override void StateExit()
    {

    }

    public override void StateUpdate()
    {
        Vector2 vec = new Vector2(player.CurrentDirection.x, player.CurrentDirection.z);
        InterpretMovement(vec);
        Decay(vec);
        cameraPos.Move(movement.x, movement.y);
    }

    public void InterpretMovement(Vector2 value)
    {
        float potentialX = (Mathf.Sign(value.x) * Time.deltaTime / maxOffsetDuration);
        float potentialY = (Mathf.Sign(value.y) * Time.deltaTime / maxOffsetDuration);

        if (Mathf.Abs(value.x) > 0.0001f)
        {
            movement += new Vector2(potentialX, 0);
        }

        if (Mathf.Abs(value.y) > 0.0001f)
        {
            //Due to low precision we are forced to have a minimum value
            potentialY = Mathf.Max(0.041f, Mathf.Abs(potentialY)) * Mathf.Sign(potentialY);
            movement += new Vector2(0, potentialY);
        }

        movement = new Vector2(Mathf.Clamp(movement.x, -1, 1), Mathf.Clamp(movement.y, -1, 1));
    }

    /// <summary>
    /// Allow to reduce the offset if the player dont go in any direction
    /// </summary>
    /// <param name="value"></param>
    public void Decay(Vector2 value)
    {
        float tempX = movement.x;
        if (value.x == 0 && tempX != 0)
        {
            tempX += (decayPerSecond * Time.deltaTime * -Mathf.Sign(movement.x));
            if (tempX * movement.x < 0)
            {
                tempX = 0;
            }
        }

        float tempY = movement.y;
        if (value.y == 0 && tempY != 0)
        {
            tempY += (decayPerSecond * Time.deltaTime * -Mathf.Sign(movement.y));
            if (tempY * movement.y < 0)
            {
                tempY = 0;
            }
        }

        Vector2 previous = movement;
        movement = new Vector2(tempX, tempY);
    }
}
