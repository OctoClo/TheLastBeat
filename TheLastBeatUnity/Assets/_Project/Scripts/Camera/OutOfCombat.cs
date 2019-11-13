using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

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

    [SerializeField]
    Vector2 maxRatio;

    [SerializeField]
    AnimationCurve cameraSmoothing;

    float ratio;
    Vector2 offsetValueMax;

    float movementX;
    float movementY;

    CinemachineCameraOffset offset;

    public override void StateEnter()
    {
        movementX = 0;
        movementY = 0;

        ratio = Camera.main.aspect;
        offsetValueMax = new Vector2(machine.virtualCam.m_Lens.OrthographicSize, machine.virtualCam.m_Lens.OrthographicSize / ratio);
        offset = GetComponent<CinemachineCameraOffset>();
        Move(0, 0);
    }

    public void Move(float ratioX, float ratioY)
    {
        offset.m_Offset = new Vector3((float)(offsetValueMax.x * cameraSmoothing.Evaluate(ratioX) * maxRatio.x), (float)(offsetValueMax.y * cameraSmoothing.Evaluate(ratioY) * maxRatio.y));
    }

    public override void StateExit()
    {
    }

    public override void StateUpdate()
    {
        Vector2 vec = new Vector2(player.CurrentDirection.x, player.CurrentDirection.z);
        InterpretMovement(vec);
        Decay(vec);
        Move(movementX, movementY);
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
