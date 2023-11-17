using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalBody : MonoBehaviour
{
    public Trajectory trajectory;
    public PlotTrajectory plotTrajectory;
    public float mass;
    public bool isOnRail;
    public bool isStationaryRelativeToParent;
    public Vector3 initialVelocity;
    public StateVector currentStateVector { get { return _currentStateVector; } set { _currentStateVector = value; UpdateStateVector(); } }
    private StateVector _currentStateVector;
    private iBodySolver bodySolver;

    private void Start()
    {
        plotTrajectory = GetComponentInChildren<PlotTrajectory>();
        bodySolver = GetComponent<iBodySolver>();
    }

    private void FixedUpdate()
    {
        if (trajectory != null && !isStationaryRelativeToParent)
        {
            while (trajectory.Peek().timestamp <= GravityController.Instance.timeStamp)
            {                
                trajectory.Dequeue();
                plotTrajectory.DropFirstPoint();
                bodySolver.GetNewPoint();
            }

            float oldTimestamp = currentStateVector.timestamp;
            float newTimestamp = trajectory.Peek().timestamp - oldTimestamp;
            float currentTimestamp = GravityController.Instance.timeStamp - oldTimestamp;
            currentStateVector = StateVector.LerpVector(currentStateVector, trajectory.Peek(), currentTimestamp / newTimestamp);
            plotTrajectory.UpdateCurrentPosition(transform.position);
        }
    }

    private void UpdateStateVector()
    {
        transform.position = _currentStateVector.position;
    }
}