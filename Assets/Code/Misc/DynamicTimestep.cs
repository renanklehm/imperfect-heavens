using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicTimestep
{
    public static float Evaluate (float accelerationGradient)
    {
        float minValue = Constants.DYNAMIC_TIMESTEP_MIN_VALUE;
        float maxValue = Constants.DYNAMIC_TIMESTEP_MAX_VALUE;
        float dampFactor = Constants.DYNAMIC_TIMESTEP_DAMP_FACTOR;
        float newTimestep = minValue + ((maxValue - minValue) * Mathf.Pow(dampFactor, accelerationGradient));

        return newTimestep;
    }
}