using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager
{
    float currentTimeScale = 1;

    public float CurrentTimeScale
    {
        get
        {
            return currentTimeScale;
        }
        set
        {
            currentTimeScale = Mathf.Max(0, value);
            Time.timeScale = currentTimeScale;
        }
    }

    public float SampleCurrentTime()
    {
        return Time.realtimeSinceStartup;
    }

    static TimeManager timeManager = null;
    public static TimeManager Instance
    {
        get
        {
            if (timeManager == null)
                timeManager = new TimeManager();
            return timeManager;
        }
    }
}
