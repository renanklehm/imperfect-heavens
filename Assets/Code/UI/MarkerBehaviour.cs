using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarkerBehaviour : MonoBehaviour
{
    public bool isPlayerOwned;
    public bool isManeuver;
    public bool isHovering;

    public LayerMask layerMask;
    public float sizeFactor;
    public StateVector stateVector;
    public GameObject graphics;
    public Transform faceCamera;
    public Canvas tooltipCanvas;
    public Canvas maneuverCanvas;

    //Arrows
    public float minArrowSize = .2f;
    public float maxArrowSize = .6f;
    public float springiness = 0.33f;
    public float maxDragCourse = 2f;
    public float visualOffsetFactor = 0.33f;

    //Tooltip
    public TMP_Text tooltipHeader;
    public TMP_Text tooltipLabel;

    //Maneuver Node
    public float exponentialFactor = 5f;
    public float maxDeltaV = 10f;
    public float thrust;
    public Vector3 deltaV;
    public Slider thrustSlider;
    public TMP_Text thrustLabel;
    public Slider[] deltaVSliders;
    public TMP_Text[] deltaVLabels;
    public ArrowHandler[] arrows;

    private void Start()
    {
        tooltipCanvas.worldCamera = Camera.main;
        maneuverCanvas.worldCamera = Camera.main;
        maneuverCanvas.gameObject.SetActive(false);
        deltaV = Vector3.zero;
    }

    private void Update()
    {
        HandleSize();
        FaceCamera();

        foreach (ArrowHandler x in arrows)
        {
            if (x.isDragging)
            {
                deltaV += x.unitVector * x.deltaV;
            }
        }

        deltaVLabels[0].text = deltaV.x.ToString("0.00");
        deltaVLabels[1].text = deltaV.y.ToString("0.00");
        deltaVLabels[2].text = deltaV.z.ToString("0.00");

        if (isHovering)
        {
            if (Input.GetMouseButtonDown(0) && isPlayerOwned)
            {
                isManeuver = true;
                maneuverCanvas.gameObject.SetActive(true);
                tooltipCanvas.gameObject.SetActive(false);
                SetArrowsVisibility(true);
            }
        }
        else
        {
            graphics.SetActive(isManeuver);
        }

    }

    private void SetArrowsVisibility(bool visibility)
    {
        foreach (ArrowHandler x in arrows)
        {
            x.gameObject.SetActive(visibility);
        }
    }

    private void HandleSize()
    {
        float newSize = Solver.GetHandleSize(transform.position, Camera.main, sizeFactor);
        transform.localScale = Vector3.one * newSize;
    }

    private void FaceCamera()
    {
        Vector3 cameraForward = -Camera.main.transform.forward;
        cameraForward.y = 0f;
        faceCamera.forward = cameraForward;
    }

    public void UpdateMarker(StateVector stateVector, Body body)
    {
        if (!isManeuver)
        {
            graphics.SetActive(true);
            Quaternion targetRotation = Quaternion.LookRotation(stateVector.prograde, stateVector.radialOut);
            transform.rotation = targetRotation;
            transform.position = stateVector.position;
            isPlayerOwned = body.HasInputAuthority;
            SetTooltip(body.name, stateVector);
        }
    }

    public void SetTooltip(string bodyName, StateVector stateVector, float relativeSpeed = 0f, float deltaV = 0f, float burnTime = 0f)
    {
        tooltipHeader.text = bodyName;

        TimeSpan timeSpan = TimeSpan.FromSeconds(stateVector.timestamp - GravityManager.Instance.timestamp);
        tooltipLabel.text = "time: " + Solver.FormatTimeSpan(timeSpan) + "\n\n";
        tooltipLabel.text += "pos: " + stateVector.position + "\n";
        tooltipLabel.text += "vel: " + stateVector.velocity + "\n";
        tooltipLabel.text += "acc: " + stateVector.acceleration + "\n\n";
        tooltipLabel.text += "relative speed: " + relativeSpeed.ToString("0.00") + "\n";
        tooltipLabel.text += "delta-v to intercept:   " + deltaV.ToString("0.00") + "\n";
        tooltipLabel.text += "burn time to intercept: " + burnTime.ToString("0.00") + "\n";
    }

    public void SetThrust(float value)
    {
        thrust = value;
        thrustLabel.text = (value * 100).ToString("0.00") + "%";
    }

    public void ResetDeltaV()
    {
        deltaV = Vector3.zero;
        ClearSliders();
    }

    public void SetDeltaV(Vector3 unitVector, float value)
    {
        deltaV += unitVector * GetScaledDeltaV(Mathf.Abs(value)) * Mathf.Sign(value);
    }

    public void ClearSliders()
    {
        foreach (Slider x in deltaVSliders)
        {
            x.value = 0f;
        }
    }

    public float GetScaledDeltaV(float lerpFactor)
    {
        return Mathf.Pow(lerpFactor, exponentialFactor) * maxDeltaV;
    }
}
