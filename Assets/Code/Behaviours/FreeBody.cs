using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    public Body body { get; set; }

    private Vector3 _activeForce;
    private float burnDuration;
    private float burnStartTimestamp;

    public SolverType solverType { get { return SolverType.FreeBody; } set {} }

    private void Awake()
    {
        body = GetComponent<Body>();
        _activeForce = Vector3.zero;
    }

    public void AddForce(Vector3 force, float _burnDuration)
    {
        _activeForce = force;
        burnDuration = _burnDuration;
        burnStartTimestamp = GravityManager.Instance.timestamp;
        GenerateTrajectory();
    }

    public void GetNewPoint()
    {
        float deltaTime = GravityManager.Instance.smoothCurve.Evaluate(body.trajectory.newestStateVector.acceleration.magnitude);
        deltaTime *= Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.trajectory.newestStateVector, body.mass, deltaTime);
        body.trajectory.Enqueue(newStateVector);
    }

    public void GenerateTrajectory()
    {
        StopAllCoroutines();
        body.trajectory.isRedrawing = false;
        StartCoroutine(GenerateTrajectoryAsync());
    }

    IEnumerator GenerateTrajectoryAsync()
    {
        Debug.Log("Generating trajectory for " + name);

        while (body.trajectory.isRedrawing)
        {
            yield return new WaitForEndOfFrame();
        }

        StateVector initialStateVector = new StateVector(body.currentStateVector);
        body.trajectory.ClearQueue();
        float scaledDeltaTime = Time.fixedDeltaTime;
        float totalTime = 0;
        float lastTimeRecorded = 0;
        int counter = 0;
        while (totalTime <= Constants.TRAJECTORY_MAX_TIME_AHEAD)
        {
            if (totalTime >= burnDuration) _activeForce = Vector3.zero;
            StateVector newStateVector = Solver.Solve(initialStateVector, body.mass, scaledDeltaTime, _activeForce);
            if (totalTime - lastTimeRecorded > Constants.TRAJECTORY_TIME_INTERVAL)
            {
                body.trajectory.Enqueue(newStateVector);
                lastTimeRecorded = totalTime;
            }
            initialStateVector = new StateVector(newStateVector);
            totalTime += scaledDeltaTime;
            float acceleration = newStateVector.acceleration.magnitude;
            float scaleFactor = GravityManager.Instance.smoothCurve.Evaluate(acceleration);
            scaledDeltaTime = _activeForce.magnitude > 0 ? Time.fixedDeltaTime : scaleFactor * Time.fixedDeltaTime;
            counter++;
            if (counter >= Constants.COROUTINE_LOOP_BATCHSIZE)
            {
                counter = 0;
                yield return new WaitForEndOfFrame();
            }
        }
        body.trajectory.needRedraw = true;
    }
}