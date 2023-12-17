using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    public Body body { get; set; }
    public SolverType solverType { get { return SolverType.FreeBody; } }

    private void Awake()
    {
        body = GetComponent<Body>();
    }

    public void GetNewPoint()
    {
        float deltaTime = GravityManager.Instance.dynamicTimestamp.Evaluate(body.trajectory.newestStateVector.acceleration.magnitude) * Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.trajectory.newestStateVector, body.mass, deltaTime, body.trajectory.newestStateVector.timestamp + deltaTime);
        body.trajectory.Enqueue(newStateVector);
    }

    public void InitiateBurn(BurnData burnData)
    {

    }

    public void GenerateTrajectory(BurnData burnData = default)
    {
        StopAllCoroutines();
        StartCoroutine(GenerateTrajectoryAsync(burnData));
    }

    IEnumerator GenerateTrajectoryAsync(BurnData burnData)
    {
        body.trajectory.ClearQueue();
        float lastTimestamp = 0;
        float dynamicTimestamp = Time.fixedDeltaTime;
        float elapsedTime = GravityManager.Instance.timestamp;
        float finalTime = GravityManager.Instance.timestamp + Constants.TRAJECTORY_MAX_TIME_AHEAD;
        StateVector stateVector = body.currentStateVector;
        Vector3 prevAcceleration = stateVector.acceleration;
        int counter = 0;

        while (elapsedTime <= finalTime)
        {
            
            Vector3 externalAcceleration = Vector3.zero;
            if (!burnData.Equals(default(BurnData)))
            {
                externalAcceleration = burnData.GetThrust(elapsedTime);
                dynamicTimestamp = Time.fixedDeltaTime;
            }

            elapsedTime += dynamicTimestamp;
            stateVector = Solver.Solve(stateVector, body.mass, dynamicTimestamp, elapsedTime, externalAcceleration);
            if (elapsedTime - lastTimestamp > Constants.TRAJECTORY_TIME_INTERVAL)
            {
                body.trajectory.Enqueue(stateVector);
                lastTimestamp = elapsedTime;
            }
            Vector3 accelerationGradient = stateVector.acceleration - prevAcceleration;
            dynamicTimestamp = Time.fixedDeltaTime * GravityManager.Instance.dynamicTimestamp.Evaluate(accelerationGradient.magnitude);
            prevAcceleration = stateVector.acceleration;
            counter++;
            if (counter >= Constants.COROUTINE_LOOP_BATCHSIZE)
            {
                counter = 0;
                yield return new WaitForEndOfFrame();
            }
            
        }
    }
}