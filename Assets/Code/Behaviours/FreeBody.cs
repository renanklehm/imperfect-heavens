using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    public bool isRunningCoroutine;
    public Body body { get; set; }

    public SolverType solverType { get { return SolverType.FreeBody; } set {} }

    private void Awake()
    {
        body = GetComponent<Body>();
    }

    public void GetNewPoint()
    {
        float deltaTime = GravityManager.Instance.dynamicTimestamp.Evaluate(body.mainTrajectory.newestStateVector.acceleration.magnitude) * Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.mainTrajectory.newestStateVector, body.mass, deltaTime, body.mainTrajectory.newestStateVector.timestamp + deltaTime);
        body.mainTrajectory.Enqueue(newStateVector);
    }

    public void GenerateTrajectory()
    {
        GenerateTrajectory(body.currentStateVector, false);
    }

    public void GenerateTrajectory(StateVector initialVector, bool isManeuver)
    {
        StopAllCoroutines();
        Trajectory trajectory = isManeuver ? body.maneuverTrajectory : body.mainTrajectory;
        StartCoroutine(GenerateTrajectoryAsync(initialVector, trajectory));
    }

    IEnumerator GenerateTrajectoryAsync(StateVector initialStateVector, Trajectory trajectory)
    {
        isRunningCoroutine = true;
        trajectory.ClearQueue();
        float lastTimestamp = 0;
        float dynamicTimestamp = Time.fixedDeltaTime;
        float elapsedTime = GravityManager.Instance.timestamp;
        float finalTime = GravityManager.Instance.timestamp + Constants.TRAJECTORY_MAX_TIME_AHEAD;
        int counter = 0;
        Vector3 prevAcceleration = initialStateVector.acceleration;
        while (elapsedTime <= finalTime)
        {
            if (elapsedTime >= initialStateVector.timestamp)
            {
                StateVector newStateVector = Solver.Solve(initialStateVector, body.mass, dynamicTimestamp, elapsedTime);
                if (elapsedTime - lastTimestamp > Constants.TRAJECTORY_TIME_INTERVAL)
                {
                    trajectory.Enqueue(newStateVector);
                    lastTimestamp = elapsedTime;
                }
                initialStateVector = new StateVector(newStateVector);
                Vector3 accelerationGradient = newStateVector.acceleration - prevAcceleration;
                dynamicTimestamp = Time.fixedDeltaTime * GravityManager.Instance.dynamicTimestamp.Evaluate(accelerationGradient.magnitude);
                prevAcceleration = newStateVector.acceleration;
                counter++;
                if (counter >= Constants.COROUTINE_LOOP_BATCHSIZE)
                {
                    counter = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
            elapsedTime += dynamicTimestamp;
        }
        isRunningCoroutine = false;
    }
}