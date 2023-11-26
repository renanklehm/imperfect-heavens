using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    [Networked]
    public bool isBurning { get; set; }
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

    private void Update()
    {
        if (burnDuration + burnStartTimestamp <= GravityManager.Instance.timestamp) isBurning = false;
    }

    public void AddForce(Vector3 force, float _burnDuration)
    {
        Debug.Log(name + " is burning " + (force.magnitude / 1000f).ToString("0.00") + "kN for " + burnDuration.ToString("0.00") + "s");
        _activeForce = force;
        burnDuration = _burnDuration;
        burnStartTimestamp = GravityManager.Instance.timestamp;
        isBurning = true;
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