using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    public Body body { get; set; }

    public SolverType solverType { get { return SolverType.FreeBody; } set {} }

    private void Awake()
    {
        body = GetComponent<Body>();
    }

    public void GetNewPoint()
    {
        float deltaTime = GravityManager.Instance.dynamicTimestamp.Evaluate(body.trajectory.newestStateVector.acceleration.magnitude);
        deltaTime *= Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.trajectory.newestStateVector, body.mass, deltaTime, body.trajectory.newestStateVector.timestamp + deltaTime);
        body.trajectory.Enqueue(newStateVector, false);
    }

    public void GenerateTrajectory()
    {
        GenerateTrajectory(body.currentStateVector, false);
    }

    public void GenerateTrajectory(StateVector initialVector, bool isManeuver)
    {
        StopAllCoroutines();
        body.trajectory.isRedrawing = false;
        StartCoroutine(GenerateTrajectoryAsync(initialVector, isManeuver));
    }

    IEnumerator GenerateTrajectoryAsync(StateVector initialStateVector, bool isManeuver)
    {
        while (body.trajectory.isRedrawing) yield return new WaitForEndOfFrame();
        body.trajectory.ClearQueue(isManeuver);
        float lastTimestamp = 0;
        float dynamicTimestamp = Time.fixedDeltaTime;
        float elapsedTime = GravityManager.Instance.timestamp;
        float maxSimulationTime = elapsedTime + Constants.TRAJECTORY_MAX_TIME_AHEAD;
        int counter = 0;
        while (elapsedTime <= maxSimulationTime)
        {
            if (elapsedTime >= initialStateVector.timestamp)
            {
                StateVector newStateVector = Solver.Solve(initialStateVector, body.mass, dynamicTimestamp, elapsedTime);
                if (elapsedTime - lastTimestamp > Constants.TRAJECTORY_TIME_INTERVAL)
                {
                    body.trajectory.Enqueue(newStateVector, isManeuver);
                    lastTimestamp = elapsedTime;
                }
                initialStateVector = new StateVector(newStateVector);
                dynamicTimestamp = Time.fixedDeltaTime * GravityManager.Instance.dynamicTimestamp.Evaluate(newStateVector.acceleration.magnitude);
                counter++;
                if (counter >= Constants.COROUTINE_LOOP_BATCHSIZE)
                {
                    counter = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
            elapsedTime += dynamicTimestamp;
        }
    }
}