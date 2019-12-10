using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomHelper
{
    static System.Random random = new System.Random();

    public static float GetRandom()
    {
        return (float)random.NextDouble();
    }

    public static float GetRandom(float min, float max)
    {
        return (float)random.NextDouble() * (max - min) + min;
    }
}
