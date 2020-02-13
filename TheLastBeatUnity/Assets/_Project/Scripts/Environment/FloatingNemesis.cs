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

    Vector3 initialPos = Vector3.zero;
    public float elapsedTime = 0;

    void OnEnable()
    {
        initialPos = transform.position;
        xSpeed /= 10;
        ySpeed /= 10;
        xAmplitude /= 100;
        yAmplitude /= 100;
        elapsedTime = 0;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        xOffset = transform.position.x - initialPos.x;
        yOffset = transform.position.y - initialPos.y;

        if (rollX)
            xOffset = Mathf.Sin(elapsedTime * xSpeed) * xAmplitude;
        if (rollY)
            yOffset = Mathf.Cos(elapsedTime * ySpeed) * yAmplitude;

        transform.position = new Vector3(initialPos.x + xOffset, initialPos.y + yOffset, transform.position.z);
    }
}
