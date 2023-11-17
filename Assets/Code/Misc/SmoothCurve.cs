using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCurve
{
    public float maxValue;
    public float dampFactor;

    public SmoothCurve (float _maxValue, float _dampFactor)
    {
        maxValue = _maxValue;
        dampFactor = _dampFactor;
    }

    public float Evaluate (float value)
    {
        return 1 + Mathf.Pow((maxValue + 1) * dampFactor, value);
    }
}
