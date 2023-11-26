using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Body : NetworkBehaviour
{
    [Networked(OnChanged = nameof(UpdateStateVector))] 
    public StateVector currentStateVector { get; set; }
    public iBodySolver bodySolver;

    public Trajectory trajectory;
    public float mass;
    public SolverType solverType;
    public bool isStationaryRelativeToParent;
    public Vector3 initialVelocity;

    public bool fullyInstantiated;

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
            currentStateVector = new StateVector(
                transform.position,
                initialVelocity,
                Vector3.zero,
                GravityManager.Instance.timestamp
            );
        }

        if (solverType == SolverType.FreeBody)
        {
            name = "Player (ID: " + Object.InputAuthority.PlayerId.ToString("00") + ")";
        }
    }

    private void Update()
    {
        if (!fullyInstantiated && GravityManager.Instance != null)
        {
            bodySolver.GenerateTrajectory();
            fullyInstantiated = true;
        }

        if (!isStationaryRelativeToParent && fullyInstantiated)
        {

            if (trajectory.Peek().timestamp <= GravityManager.Instance.timestamp)
            {
                trajectory.Dequeue();
            }
            if (trajectory.stateVectorQueue.Count <= trajectory.maxSize / 2f)
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

            trajectory.lineRenderer.SetPosition(0, transform.position);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!isStationaryRelativeToParent && fullyInstantiated)
        {
            float oldTimestamp = currentStateVector.timestamp;
            float newTimestamp = trajectory.Peek().timestamp - oldTimestamp;
            float currentTimestamp = GravityManager.Instance.timestamp - oldTimestamp;
            StateVector newStateVector = StateVector.LerpVector(currentStateVector, trajectory.Peek(), currentTimestamp / newTimestamp);

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