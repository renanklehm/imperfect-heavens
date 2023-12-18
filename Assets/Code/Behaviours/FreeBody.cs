using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    public Body body { get; set; }
    public SolverType solverType { get { return SolverType.FreeBody; } }

    private List<Maneuver> plannedManeuvers;

    private void Awake()
    {
        body = GetComponent<Body>();
        plannedManeuvers = new List<Maneuver>();
    }

    public void GetNewPoint()
    {
        float deltaTime = DynamicTimestep.Evaluate(body.trajectory.newestStateVector.acceleration.magnitude) * Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.trajectory.newestStateVector, body.mass, deltaTime, body.trajectory.newestStateVector.timestamp + deltaTime);
        body.trajectory.Enqueue(newStateVector);
    }

    public void SetManeuver(Maneuver newManeuver)
    {
        plannedManeuvers.Add(newManeuver);
        GenerateTrajectory();
    }

    public void GenerateTrajectory(Maneuver newManeuver = default)
    {
        StopAllCoroutines();
        StartCoroutine(GenerateTrajectoryAsync(newManeuver));
    }

    IEnumerator GenerateTrajectoryAsync(Maneuver newManeuver)
    {
        body.trajectory.ClearQueue();
        float lastTimestamp = 0;
        float elapsedTime = GravityManager.Instance.timestamp;
        float finalTime = GravityManager.Instance.timestamp + Constants.TRAJECTORY_MAX_TIME_AHEAD;
        float dynamicTimestamp = Constants.DYNAMIC_TIMESTEP_MIN_VALUE;
        StateVector stateVector = body.currentStateVector;
        int counter = 0;

        while (elapsedTime <= finalTime)
        {
            Vector3 externalAcceleration = Vector3.zero;
            if (newManeuver.startTime <= elapsedTime && elapsedTime <= newManeuver.endTime)
            {
                externalAcceleration = newManeuver.GetThrust(elapsedTime);
                dynamicTimestamp = Constants.DYNAMIC_TIMESTEP_MIN_VALUE;
            }
            foreach (Maneuver maneuver in plannedManeuvers)
            {
                if (maneuver.startTime <= elapsedTime && elapsedTime <= maneuver.endTime)
                {
                    externalAcceleration += maneuver.GetThrust(elapsedTime);
                    dynamicTimestamp = Constants.DYNAMIC_TIMESTEP_MIN_VALUE;
                }
            }
            
            elapsedTime += dynamicTimestamp;
            stateVector = Solver.Solve(stateVector, body.mass, dynamicTimestamp, elapsedTime, externalAcceleration);
            
            if (elapsedTime - lastTimestamp > Constants.TRAJECTORY_TIME_INTERVAL)
            {
                body.trajectory.Enqueue(stateVector);
                lastTimestamp = elapsedTime;
            }
         
            counter++;
            if (counter >= Constants.COROUTINE_LOOP_BATCHSIZE)
            {
                counter = 0;
                yield return new WaitForEndOfFrame();
            }
            dynamicTimestamp = DynamicTimestep.Evaluate(stateVector.acceleration.magnitude);
        }
    }
}