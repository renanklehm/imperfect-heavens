using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Body : NetworkBehaviour
{

    public iBodySolver bodySolver;
    public SolverType solverType;
    public Vector3 initialVelocity;
    public float mass;
    public bool isStationaryRelativeToParent;
    public bool fullyInstantiated;

    [HideInInspector]
    public Trajectory mainTrajectory;
    [HideInInspector]
    public Trajectory maneuverTrajectory;
    [HideInInspector]
    [Networked(OnChanged = nameof(UpdateStateVector))]
    public StateVector currentStateVector { get; set; }

    private void Awake()
    {
        bodySolver = GetComponent<iBodySolver>();
        mainTrajectory = GetComponentInChildren<Trajectory>();
        GravityManager.Instance.RegisterBody(this);
    }

    private void Start()
    {
        if (HasStateAuthority)
        {
            var motionVectors = GravityManager.Instance.GetMotionVectors(transform.position, initialVelocity, Vector3.zero);
            currentStateVector = new StateVector(
                transform.position,
                initialVelocity,
                Vector3.zero,
                motionVectors[MotionVector.Prograde],
                motionVectors[MotionVector.RadialOut],
                GravityManager.Instance.timestamp,
                Vector3.zero
            );
        }

        if (solverType == SolverType.FreeBody)
        {
            name = "Player (ID: " + Object.InputAuthority.PlayerId.ToString("00") + ")";
            maneuverTrajectory = Instantiate(mainTrajectory);
            maneuverTrajectory.InitializeTrajectory(this, name + " - ManeuverTrajectory");
            maneuverTrajectory.isManeuver = true;
        }
        mainTrajectory.InitializeTrajectory(this, name + " - MainTrajectory");
        mainTrajectory.isManeuver = false;
    }

    private void Update()
    {
        if (!fullyInstantiated && GravityManager.Instance != null && mainTrajectory != null)
        {
            bodySolver.GenerateTrajectory();
            fullyInstantiated = true;
        }

        if (!isStationaryRelativeToParent && fullyInstantiated)
        {

            if (mainTrajectory.Peek().timestamp <= GravityManager.Instance.timestamp)
            {
                mainTrajectory.Dequeue();
            }
            if (mainTrajectory.Count <= mainTrajectory.maxSize / 2f)
            {
                Debug.Log("Extending trajectory of " + name);
                bodySolver.GenerateTrajectory();
            }

            Vector3 directionOfMotion = (currentStateVector.position + currentStateVector.velocity) - currentStateVector.position;
            Vector3 originOfMotion = -currentStateVector.position;
            directionOfMotion.Normalize();
            originOfMotion.Normalize();
            Vector3 newUp = Vector3.Cross(originOfMotion, directionOfMotion).normalized;
            Quaternion newRotation = Quaternion.LookRotation(directionOfMotion, newUp);
            transform.rotation = newRotation;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isStationaryRelativeToParent && fullyInstantiated)
        {
            float oldTimestamp = currentStateVector.timestamp;
            float newTimestamp = mainTrajectory.Peek().timestamp - oldTimestamp;
            float currentTimestamp = GravityManager.Instance.timestamp - oldTimestamp;
            StateVector newStateVector = StateVector.LerpVector(currentStateVector, mainTrajectory.Peek(), currentTimestamp / newTimestamp);

            if (HasStateAuthority)
            {
                currentStateVector = newStateVector;
            }
            else
            {
                float errorScore = StateVector.ScoreDifference(currentStateVector, newStateVector);
                if (errorScore > Constants.DESYNC_MARGIN_OF_ERROR && solverType == SolverType.FreeBody)
                {
                    Debug.Log("Error Score: " + errorScore);
                    bodySolver.GenerateTrajectory();
                }
            }
        }
    }
    private static void UpdateStateVector(Changed<Body> changed)
    {
        changed.Behaviour.transform.position = changed.Behaviour.currentStateVector.position;
    }
}