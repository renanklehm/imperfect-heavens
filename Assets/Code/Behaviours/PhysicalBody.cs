using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PhysicalBody : NetworkBehaviour
{
    [Networked(OnChanged = nameof(UpdateStateVector))] public StateVector currentStateVector { get; set; }
    public iBodySolver bodySolver;

    public Trajectory trajectory;
    public PlotTrajectory plotTrajectory;
    public float mass;
    public SolverType solverType;
    public bool isStationaryRelativeToParent;
    public Vector3 initialVelocity;

    private bool hasTrajectory;

    private void Awake()
    {
        bodySolver = GetComponent<iBodySolver>();
        plotTrajectory = GetComponentInChildren<PlotTrajectory>();
        GravityController.Instance.RegisterBody(this);
    }

    private void Start()
    {
        if (HasStateAuthority)
        {
            currentStateVector = new StateVector(
                transform.position,
                initialVelocity,
                Vector3.zero,
                GravityController.Instance.timeStamp
            );
        }
    }

    private void Update()
    {
        if (!isStationaryRelativeToParent && hasTrajectory)
        {
            float simulationTimeStamp = GravityController.Instance.timeStamp;
            float currentTimeStamp = trajectory.Peek().timestamp;

            if (currentTimeStamp <= simulationTimeStamp)
            {
                trajectory.Dequeue();
                plotTrajectory.DropFirstPoint();
                bodySolver.GetNewPoint();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isStationaryRelativeToParent)
        {
            if (hasTrajectory && HasStateAuthority)
            {
                float oldTimestamp = currentStateVector.timestamp;
                float newTimestamp = trajectory.Peek().timestamp - oldTimestamp;
                float currentTimestamp = GravityController.Instance.timeStamp - oldTimestamp;
                currentStateVector = StateVector.LerpVector(currentStateVector, trajectory.Peek(), currentTimestamp / newTimestamp);
                plotTrajectory.UpdateCurrentPosition(transform.position);
            }
            else
            {
                bodySolver.GenerateTrajectory();
                hasTrajectory = true;
            }

        }
    }
    
    private static void UpdateStateVector(Changed<PhysicalBody> changed)
    {
        changed.Behaviour.transform.position = changed.Behaviour.currentStateVector.position;
    }
}