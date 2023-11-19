using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditorDebuger : MonoBehaviour
{
    public SmoothCurve smoothCurve;
    [Range(1, 100)]
    public float maxTimestepMultiplier = 10f;
    [Range(0, 1)]
    public float dampFactor = 0.25f;
    [Range(1, 100)]
    public float testValue = 10f;

    void Update()
    {
        smoothCurve = new SmoothCurve(maxTimestepMultiplier, dampFactor);
        Debug.Log("smooth curve value: " + smoothCurve.Evaluate(testValue));
    }
}
