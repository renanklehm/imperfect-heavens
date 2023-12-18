using Fusion;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManeuverNode : NetworkBehaviour
{
    public bool isManeuvering;
    public bool isHovering;

    public LayerMask layerMask;
    public float sizeFactor;
    public GameObject graphics;
    public Transform faceCamera;
    public Canvas tooltipCanvas;
    public Canvas maneuverCanvas;

    [HideInInspector]
    public StateVector stateVector;

    [Header("Arrow Settings")]
    public float minArrowSize = .2f;
    public float maxArrowSize = .6f;
    public float springiness = 0.33f;
    public float maxDragCourse = 2f;
    public float visualOffsetFactor = 0.33f;

    [Header("Tooltip Settings")]
    public TMP_Text tooltipHeader;
    public TMP_Text tooltipLabel;

    [Header("Maneuver Settings")]
    public float exponentialFactor = 5f;
    public float maxDeltaV = 10f;
    public Slider throttleSlider;
    public TMP_Text throttleLabel;
    public TMP_Text maneuverLabel;
    public Slider[] deltaVSliders;
    public TMP_Text[] deltaVLabels;
    public ArrowHandler[] arrows;

    [Header("Engine Settings")]
    public TMP_Dropdown engineModeDropdown;
    public TMP_Text engineClusterDetails;
    public Button[] engineClusterSelectors;

    private Body currentBody;
   
    Dictionary<int, string> engineClusterOptions;
    private int currentEngineCluster = 0;
    Dictionary<int, string> engineModeOptions;
    private int currentEngineMode = 0;

    public float throttle
    {
        get { return _throttle; }
        set
        {
            _throttle = value;
            deltaV = deltaV;
        }
    }
    public Vector3 deltaV
    {
        get { return _deltaV; }
        set
        {
            if (value == Vector3.zero)
            {
                _deltaV = Vector3.zero;
                SetDeltaVLabels();
                return;
            }

            maneuverSimulation = new Maneuver(
                currentEngineMode,
                currentEngineCluster,
                ToBodyMotionFrame(value).normalized,
                stateVector.timestamp, 
                throttle, 
                value.magnitude, 
                new Ship(shipController.ship)
                );

            if (maneuverSimulation.success) _deltaV = value;
            shipController.body.bodySolver.GenerateTrajectory(maneuverSimulation);
            SetDeltaVLabels();
        }
    }
    private float _throttle = 0f;
    private Vector3 _deltaV = Vector3.zero;
    private ShipController shipController;
    private Maneuver maneuverSimulation;

    private void Start()
    {
        if (!HasInputAuthority) Destroy(this);

        shipController = GetComponentInParent<ShipController>();
        tooltipCanvas.worldCamera = Camera.main;
        maneuverCanvas.worldCamera = Camera.main;
        maneuverCanvas.gameObject.SetActive(false);
        transform.parent = null;

        engineClusterOptions = shipController.ship.engineClusterOptions;
        engineModeOptions = shipController.ship.engineClusters[currentEngineCluster].engineModeOptions;
        engineClusterSelectors[0].interactable = false;
        engineClusterSelectors[1].interactable = engineClusterOptions.Count > 1;
        SetEngineModeDropdown();
        SetClusterDetails();
        SetArrowsVisibility(false);
    }

    private void Update()
    {
        HandleSize();
        FaceCamera();
        HandleMarkerPosition();

        foreach (ArrowHandler x in arrows)
        {
            if (x.isDragging.IsOn())
            {
                deltaV += x.unitVector * x.deltaV;
            }
        }

        if (isHovering)
        {
            if (Input.GetMouseButtonDown(0) && !isManeuvering && currentBody.HasInputAuthority)
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
                    currentBody = trajectory.body;
                    UpdateMarker(selectedStateVector);
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
        faceCamera.forward = cameraForward;
    }

    private Vector3 ToBodyMotionFrame(Vector3 velocity)
    {
        StateVector tempStateVector = new StateVector(stateVector);
        tempStateVector.velocity += (velocity.x / Constants.DISTANCE_FACTOR) * tempStateVector.radialOut * Mathf.Sign(_deltaV.x);
        tempStateVector.velocity += (velocity.y / Constants.DISTANCE_FACTOR) * tempStateVector.normal * Mathf.Sign(_deltaV.y);
        tempStateVector.velocity += (velocity.z / Constants.DISTANCE_FACTOR) * tempStateVector.prograde * Mathf.Sign(_deltaV.z);
        return tempStateVector.velocity;
    }

    public void UpdateMarker(StateVector _stateVector)
    {
        graphics.SetActive(true);
        stateVector = _stateVector;
        Quaternion targetRotation = Quaternion.LookRotation(stateVector.prograde, stateVector.normal);
        transform.rotation = targetRotation;
        transform.position = stateVector.position;
        SetTooltip(currentBody.name, stateVector);
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
        shipController.AddManeuver(maneuverSimulation);
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

    public void SetThrottle(float value)
    {
        throttle = value;
        throttleLabel.text = (value * 100).ToString("0.00") + "%";
    }

    public void ResetDeltaV()
    {
        deltaV = Vector3.zero;
        ClearSliders();
    }

    public void ChangeEngineMode(int index)
    {
        currentEngineMode = index;
        SetClusterDetails();
    }

    public void ChangeEngineCluster(bool isNext)
    {
        if (isNext) currentEngineCluster++;
        else currentEngineCluster--;

        engineClusterSelectors[0].interactable = currentEngineCluster > 0;
        engineClusterSelectors[1].interactable = currentEngineCluster < engineClusterOptions.Count - 1;

        SetClusterDetails();
        SetEngineModeDropdown();
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

    private void SetMinimunThrottle()
    {
        float maxMassFlow = shipController.ship.engineClusters[currentEngineCluster].engineModes[currentEngineMode].maxMassFlow;
        float minMassFlow = shipController.ship.engineClusters[currentEngineCluster].engineModes[currentEngineMode].minMassFlow;
        float minThrottle = minMassFlow / maxMassFlow;
        throttleSlider.minValue = minThrottle;
    }

    private void SetEngineModeDropdown()
    {
        engineModeOptions = shipController.ship.engineClusters[currentEngineCluster].engineModeOptions;
        engineModeDropdown.ClearOptions();
        engineModeDropdown.AddOptions(new List<string>(engineModeOptions.Values));
        engineModeDropdown.value = 0;
        SetMinimunThrottle();
    }

    private void SetClusterDetails()
    {
        string clusterDetails = engineClusterOptions[currentEngineCluster] + "\n";
        clusterDetails += "TWR    : " + shipController.ship.TWR(throttle, 0f, currentEngineMode, currentEngineCluster).ToString("0.000") + "\n";
        clusterDetails += "Delta-V: " + shipController.ship.DeltaV(0f, currentEngineMode, currentEngineCluster).ToString("0") + "\n";
        engineClusterDetails.text = clusterDetails;
        deltaV = _deltaV;
        SetMinimunThrottle();
    }

    private void SetDeltaVLabels()
    {
        maneuverLabel.text = "";
        maneuverLabel.text += "burn duration : " + maneuverSimulation.burnDuration.ToString("0.00") + "s\n";
        maneuverLabel.text += "delta-v       : " + maneuverSimulation.deltaV.ToString("0.00") + " m/s\n";
        maneuverLabel.text += "remaining fuel: " + maneuverSimulation.remainingFuelMass.ToString("0.00") + " kg\n";

        deltaVLabels[0].text = _deltaV.x.ToString("0.00");
        deltaVLabels[1].text = _deltaV.y.ToString("0.00");
        deltaVLabels[2].text = _deltaV.z.ToString("0.00");
    }
}
