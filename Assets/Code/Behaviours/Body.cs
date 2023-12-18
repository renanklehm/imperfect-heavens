using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Body : NetworkBehaviour
{

    public iBodySolver bodySolver;
    public SolverType solverType;

    public float mass;
    public bool isStationaryRelativeToParent;
    public bool fullyInstantiated;

    [HideInInspector]
    public Vector3 initialVelocity;
    [HideInInspector]
    public Trajectory trajectory;
    [HideInInspector]
    [Networked(OnChanged = nameof(UpdateStateVector))]
    public StateVector currentStateVector { get; set; }

    private void Awake()
    {
        bodySolver = GetComponent<iBodySolver>();
        trajectory = GetComponentInChildren<Trajectory>();
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
                Vector3.zero,
                GravityManager.Instance.timestamp
            );
        }
        trajectory.InitializeTrajectory(this);
        trajectory.isManeuver = false;
    }

    private void Update()
    {
        if (!fullyInstantiated && GravityManager.Instance != null && trajectory != null && mass != 0)
        {
            bodySolver.GenerateTrajectory();
            fullyInstantiated = true;
        }

        if (!isStationaryRelativeToParent && fullyInstantiated && trajectory.Peek().timestamp <= GravityManager.Instance.timestamp)
        {
            trajectory.Dequeue();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isStationaryRelativeToParent && fullyInstantiated && HasStateAuthority)
        {
            currentStateVector = Solver.Solve(currentStateVector, mass, GravityManager.Instance.deltaTime, GravityManager.Instance.timestamp);
        }
    }

    public void SetName(string newName)
    {
        name = newName + " (ID: " + Object.InputAuthority.PlayerId.ToString("00") + ")";
        trajectory.name = name + " - Trajectory";
    }

    private static void UpdateStateVector(Changed<Body> changed)
    {
        changed.Behaviour.transform.position = changed.Behaviour.currentStateVector.position;
    }
}