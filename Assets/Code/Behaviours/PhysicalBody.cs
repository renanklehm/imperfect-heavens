using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PhysicalBody : NetworkBehaviour
{
    public Trajectory trajectory;
    public PlotTrajectory plotTrajectory;
    public float mass;
    public bool isOnRail;
    public bool isStationaryRelativeToParent;
    public Vector3 initialVelocity;

    [Networked(OnChanged = nameof(UpdateStateVector))] public StateVector currentStateVector { get; set; }
    public iBodySolver bodySolver;

    private void Awake()
    {
        bodySolver = GetComponent<iBodySolver>();
        plotTrajectory = GetComponentInChildren<PlotTrajectory>();
        //GravityController.Instance.RegisterBody(bodySolver);
    }

    public override void FixedUpdateNetwork()
    {
        if (!isStationaryRelativeToParent)
        {
            if (trajectory == null)
            {
                while (trajectory.Peek().timestamp <= GravityController.Instance.timeStamp)
                {
                    Debug.Log("Dequeueing " + name);
                    trajectory.Dequeue();
                    plotTrajectory.DropFirstPoint();
                    bodySolver.GetNewPoint();
                }

                if (HasStateAuthority == true)
                {
                    Debug.Log("Updating " + name);
                    float oldTimestamp = currentStateVector.timestamp;
                    float newTimestamp = trajectory.Peek().timestamp - oldTimestamp;
                    float currentTimestamp = GravityController.Instance.timeStamp - oldTimestamp;
                    currentStateVector = StateVector.LerpVector(currentStateVector, trajectory.Peek(), currentTimestamp / newTimestamp);
                }

                plotTrajectory.UpdateCurrentPosition(transform.position);
            }
            else
            {
                bodySolver.GenerateTrajectory();
            }

        }
    }
    
    private static void UpdateStateVector(Changed<PhysicalBody> changed)
    {
        changed.Behaviour.transform.position = changed.Behaviour.currentStateVector.position;
    }
}