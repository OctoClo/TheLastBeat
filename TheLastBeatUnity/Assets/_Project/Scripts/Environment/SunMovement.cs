﻿using System;
using UnityEngine;

public class SunMovement : MonoBehaviour
{
    public Vector3andSpace moveUnitsPerSecond;
    public Vector3andSpace rotateDegreesPerSecond;
    public bool ignoreTimescale;
    private float m_LastRealTime;

    private float _timestamp;

    private void Start()
    {
        m_LastRealTime = Time.realtimeSinceStartup;
        _timestamp = Time.time;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if (ignoreTimescale)
        {
            deltaTime = (Time.realtimeSinceStartup - m_LastRealTime);
            m_LastRealTime = Time.realtimeSinceStartup;
        }
        transform.Translate(moveUnitsPerSecond.value * deltaTime, moveUnitsPerSecond.space);
        transform.Rotate(rotateDegreesPerSecond.value * deltaTime, rotateDegreesPerSecond.space);
    }

    [Serializable]
    public class Vector3andSpace
    {
        public Vector3 value;
        public Space space = Space.Self;
    }
}
