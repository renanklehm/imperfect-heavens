using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(PhysicalBody))]
public class FreeBody : NetworkBehaviour, iBodySolver
{
    [Networked] private NetworkBool needsRedraw { get; set; }
    public PhysicalBody body { get; set; }

    public int nStepsAhead = 10000;

    private PlotTrajectory plotTrajectory;
    private Vector3 _activeForce;
    private float burnDuration = 10f;

    public SolverType solverType { get { return SolverType.FreeBody; } set {} }

    private void Awake()
    {
        body = GetComponent<PhysicalBody>();
        plotTrajectory = GetComponentInChildren<PlotTrajectory>();
        _activeForce = Vector3.zero;
    }

    public override void FixedUpdateNetwork()
    {
        if (needsRedraw)
        {
            plotTrajectory.DrawTrajectory(body.trajectory);
            needsRedraw = false;
        }
    }

    public void AddForce(Vector3 force)
    {
        _activeForce = force;
        StartCoroutine(GenerateTrajectoryAsync());
    }

    public void GetNewPoint()
    {
        float deltaTime = GravityController.Instance.smoothCurve.Evaluate(body.trajectory.newestStateVector.acceleration.magnitude);
        deltaTime *= Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.trajectory.newestStateVector, body.mass, deltaTime);
        body.trajectory.Enqueue(newStateVector);
        plotTrajectory.AddPoint(newStateVector.position);
        needsRedraw = true;
    }

    public void GenerateTrajectory()
    {
        StartCoroutine(GenerateTrajectoryAsync());
    }

    IEnumerator GenerateTrajectoryAsync()
    {
        StateVector initialStateVector = new StateVector(body.currentStateVector);
        Debug.Log(body.currentStateVector);
        body.trajectory = new Trajectory(body.currentStateVector, nStepsAhead);
        float scaledDeltaTime = Time.fixedDeltaTime;
        float totalTime = 0;
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
            float scaleFactor = GravityController.Instance.smoothCurve.Evaluate(newStateVector.acceleration.magnitude);
            scaledDeltaTime = _activeForce.magnitude > 0 ? Time.fixedDeltaTime : scaleFactor * Time.fixedDeltaTime;
            yield return null;
        }
        body.currentStateVector = body.trajectory.Dequeue();
        needsRedraw = true;
    }
}