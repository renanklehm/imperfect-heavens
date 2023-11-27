using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarkerBehaviour : MonoBehaviour
{
    public Canvas canvas;
    public TMP_Text header;
    public TMP_Text label;
    public float sizeFactor;

    private void Start()
    {
        canvas.worldCamera = Camera.main;
    }


    private void Update()
    {
        float newSize = Solver.GetHandleSize( transform.position, Camera.main, sizeFactor);
        transform.localScale = Vector3.one * newSize;
        Vector3 cameraForward = -Camera.main.transform.forward;
        cameraForward.y = 0f;
        transform.forward = cameraForward;
    }

    public void SetTooltip(string bodyName, StateVector stateVector, float relativeSpeed = 0f, float deltaV = 0f, float burnTime = 0f)
    {
        header.text = bodyName;

        TimeSpan timeSpan = TimeSpan.FromSeconds(stateVector.timestamp - GravityManager.Instance.timestamp);
        label.text = "time: " + Solver.FormatTimeSpan(timeSpan) + "\n\n";
        label.text += "pos: " + stateVector.position + "\n";
        label.text += "vel: " + stateVector.velocity + "\n";
        label.text += "acc: " + stateVector.acceleration + "\n\n";
        label.text += "relative speed: " + relativeSpeed.ToString("0.00") + "\n";
        label.text += "delta-v to intercept:   " + deltaV.ToString("0.00") + "\n";
        label.text += "burn time to intercept: " + burnTime.ToString("0.00") + "\n";
    }
}
