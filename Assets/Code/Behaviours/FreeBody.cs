using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(Body))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    public Body body { get; set; }

    public int nStepsAhead = 10000;
    private Vector3 _activeForce;
    private float burnDuration = 10f;

    public SolverType solverType { get { return SolverType.FreeBody; } set {} }

    private void Awake()
    {
        body = GetComponent<Body>();
        _activeForce = Vector3.zero;
    }

    public void AddForce(Vector3 force, float _burnDuration)
    {
        Debug.Log(name + " is burning " + (force.magnitude / 1000f).ToString("0.00") + "kN for " + burnDuration.ToString("0.00") + "s");
        _activeForce = force;
        burnDuration = _burnDuration;
        StopAllCoroutines();
        StartCoroutine(GenerateTrajectoryAsync());
    }

    public void GetNewPoint()
    {
        float deltaTime = GravityManager.Instance.smoothCurve.Evaluate(body.trajectory.newestStateVector.acceleration.magnitude);
        deltaTime *= Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.trajectory.newestStateVector, body.mass, deltaTime);
        body.trajectory.Enqueue(newStateVector, TrajectoryRedrawMode.Incremental);
    }

    public void GenerateTrajectory()
    {
        StopAllCoroutines();
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
        int counter = 0;
        for (int i = 1; i < nStepsAhead; i++)
        {
            if (totalTime >= burnDuration)
            {
                _activeForce = Vector3.zero;
            }
            StateVector newStateVector = Solver.Solve(initialStateVector, body.mass, scaledDeltaTime, _activeForce);
            body.trajectory.Enqueue(newStateVector);
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
        if (HasStateAuthority)
        {
            body.currentStateVector = body.trajectory.Dequeue();
        }
        else
        {
            body.trajectory.Dequeue();
        }
    }
}