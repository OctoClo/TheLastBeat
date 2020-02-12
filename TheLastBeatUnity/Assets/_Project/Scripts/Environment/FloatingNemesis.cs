using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingNemesis : MonoBehaviour
{
    [Header("X values")] [SerializeField]
    bool rollX = true;
    [Range(0, 100)] [SerializeField]
    float xAmplitude = 10f;
    [Range(0, 100)] [SerializeField]
    float xSpeed = 8f;
    float xOffset;

    [Header("Y values")] [SerializeField]
    bool rollY = true;
    [Range(0, 100)] [SerializeField]
    float yAmplitude = 5f;
    [Range(0, 100)] [SerializeField]
    float ySpeed = 10f;
    float yOffset;

    [Header("Z values")] [SerializeField]
    bool rollZ = true;
    [Range(0, 100)] [SerializeField]
    float zAmplitude = 1f;
    [Range(0, 100)] [SerializeField]
    float zSpeed = 10f;
    float zOffset;

    Vector3 initialPos = Vector3.zero;
    float elapsedTime = 0;

    void OnEnable()
    {
        initialPos = transform.position;
        elapsedTime = 0;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        xOffset = transform.position.x - initialPos.x;
        yOffset = transform.position.y - initialPos.y;
        zOffset = transform.position.z - initialPos.z;

        if (rollX)
            xOffset = Mathf.Sin(elapsedTime * (xSpeed / 10)) * (xAmplitude / 100);
        if (rollY)
            yOffset = Mathf.Cos(elapsedTime * (ySpeed / 10)) * (yAmplitude / 100);
        if (rollZ)
            zOffset = Mathf.Cos(elapsedTime * (zSpeed / 10)) * (zAmplitude / 100);

        transform.position = new Vector3(initialPos.x + xOffset, initialPos.y + yOffset, transform.position.z + zOffset);
    }
}
