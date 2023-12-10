using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MarkerBehaviour : MonoBehaviour
{
    public bool isPlayerOwned;
    public bool isManeuvering;
    public bool isHovering;

    public LayerMask layerMask;
    public float sizeFactor;
    public StateVector stateVector;
    public GameObject graphics;
    public Transform faceCamera;
    public Canvas tooltipCanvas;
    public Canvas maneuverCanvas;
    public ShipController shipController;

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
    public Vector3 deltaV {
        get { return _deltaV; } 
        set
        {
            var burnSimulation = shipController.SimulateBurn(1, value.magnitude);
            maneuverLabel.text = "";
            maneuverLabel.text += "burn duration:    " + burnSimulation["burnDuration"].ToString("0.00") + "s\n";
            maneuverLabel.text += "delta-v:          " + burnSimulation["deltaV"].ToString("0.00") + " m/s\n";
            maneuverLabel.text += "fuel consumption: " + burnSimulation["fuelConsumption"].ToString("0.00") + " kg\n";
            maneuverLabel.text += "remaining fuel:   " + burnSimulation["remainingFuelMass"].ToString("0.00") + " kg\n";
            _deltaV = value.normalized * burnSimulation["deltaV"];
            StateVector newStateVector = new StateVector(stateVector);
            newStateVector.velocity += (_deltaV.x / Constants.DISTANCE_FACTOR) * newStateVector.radialOut;
            newStateVector.velocity += (_deltaV.y / Constants.DISTANCE_FACTOR) * newStateVector.normal;
            newStateVector.velocity += (_deltaV.z / Constants.DISTANCE_FACTOR) * newStateVector.prograde;
            shipController.body.bodySolver.GenerateTrajectory(newStateVector, true);
        }
    }
    private Vector3 _deltaV = Vector3.zero;
    public float exponentialFactor = 5f;
    public float maxDeltaV = 10f;
    public float thrust;
    public Slider thrustSlider;
    public TMP_Text thrustLabel;
    public TMP_Text maneuverLabel;
    public Slider[] deltaVSliders;
    public TMP_Text[] deltaVLabels;
    public ArrowHandler[] arrows;

    private void Start()
    {
        tooltipCanvas.worldCamera = Camera.main;
        maneuverCanvas.worldCamera = Camera.main;
        maneuverCanvas.gameObject.SetActive(false);
        SetArrowsVisibility(false);
    }

    private void Update()
    {
        HandleSize();
        FaceCamera();
        HandleMarkerPosition();

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
            if (Input.GetMouseButtonDown(0) && isPlayerOwned && !isManeuvering)
            {
                deltaV = Vector3.zero;
                isManeuvering = true;
                maneuverCanvas.gameObject.SetActive(true);
                tooltipCanvas.gameObject.SetActive(false);
                SetArrowsVisibility(true);
                GameManager.Instance.isPlanningManeuver = true;
            }
        }
        else
        {
            graphics.SetActive(isManeuvering);
        }

    }

    private void SetArrowsVisibility(bool visibility)
    {
        foreach (ArrowHandler x in arrows)
        {
            x.gameObject.SetActive(visibility);
        }
    }

    private void HandleMarkerPosition()
    {
        if (isManeuvering) return;

        float minDistance = float.PositiveInfinity;
        StateVector selectedStateVector = new StateVector();
        Vector2 mousePosition = Input.mousePosition;

        foreach (Trajectory trajectory in FindObjectsOfType<Trajectory>())
        {
            if (trajectory.IsEmpty()) continue;

            if (!GameManager.Instance.isPlanningManeuver && !GameManager.Instance.isRotatingCamera)
            {
                List<List<Vector3>> renderedPositions = trajectory.lineRenderer.Positions;
                for (int i = 0; i < renderedPositions[0].Count - 1; i++)
                {
                    Vector2 startPoint = Camera.main.WorldToScreenPoint(renderedPositions[0][i]);
                    Vector2 endPoint = Camera.main.WorldToScreenPoint(renderedPositions[0][i + 1]);
                    Vector2 lineVector = endPoint - startPoint;
                    Vector2 mouseVector = endPoint - mousePosition;
                    float lerpFactor = Mathf.Clamp(Vector2.Dot(mouseVector, lineVector) / Vector2.Dot(lineVector, lineVector), 0f, 1f);
                    lerpFactor = 1 - lerpFactor;
                    Vector2 _closestPoint = new Vector2(startPoint.x + lerpFactor * (endPoint.x - startPoint.x), startPoint.y + lerpFactor * (endPoint.y - startPoint.y));
                    float distance = Vector2.Distance(mousePosition, _closestPoint);
                    if (distance <= minDistance)
                    {
                        minDistance = distance;
                        selectedStateVector = StateVector.LerpVector(trajectory[i], trajectory[i + 1], lerpFactor);
                    }
                }

                if (minDistance <= Constants.MOUSE_HOVER_SCREEN_DISTANCE)
                {
                    isHovering = true;
                    UpdateMarker(selectedStateVector, trajectory.body);
                    break;
                }
                else
                {
                    isHovering = false;
                }
            }
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

    public void UpdateMarker(StateVector _stateVector, Body body)
    {
        graphics.SetActive(true);
        stateVector = _stateVector;
        Quaternion targetRotation = Quaternion.LookRotation(stateVector.prograde, stateVector.normal);
        transform.rotation = targetRotation;
        transform.position = stateVector.position;
        isPlayerOwned = body.HasInputAuthority;
        SetTooltip(body.name, stateVector);
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

    public void SetManeuver()
    {
        //shipController.body.mainTrajectory.SetManeuver();
        DiscardManeuver();
    }

    public void DiscardManeuver()
    {
        isManeuvering = false;
        maneuverCanvas.gameObject.SetActive(false);
        tooltipCanvas.gameObject.SetActive(true);
        SetArrowsVisibility(false);
        GameManager.Instance.isPlanningManeuver = false;
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
