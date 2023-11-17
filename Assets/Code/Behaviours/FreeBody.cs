using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicalBody))]
public class FreeBody : MonoBehaviour, iBodySolver
{
    public float timeWarp = 1f;
    public PhysicalBody body;
    public PlotTrajectory plotTrajectory;
    public int nStepsAhead;


    public Vector3 _activeForce;
    private float burnDuration = 10f;

    private void Start()
    {
        body = GetComponent<PhysicalBody>();
        plotTrajectory = GetComponentInChildren<PlotTrajectory>();
        _activeForce = Vector3.zero;
        StartCoroutine(GenerateTrajectory());
    }

    public void AddForce(Vector3 force)
    {
        _activeForce = force;
        StartCoroutine(GenerateTrajectory());
    }

    public void GetNewPoint()
    {
        float deltaTime = GravityController.Instance.smoothCurve.Evaluate(body.trajectory.newestStateVector.acceleration.magnitude);
        deltaTime *= Time.fixedDeltaTime;
        StateVector newStateVector = Solver.Solve(body.trajectory.newestStateVector, body.mass, deltaTime);
        body.trajectory.Enqueue(newStateVector);
    }

    IEnumerator GenerateTrajectory()
    {
        StateVector initialStateVector = new StateVector(transform.position, body.initialVelocity, Vector3.zero, GravityController.Instance.timeStamp);
        body.currentStateVector = initialStateVector;
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
            initialStateVector = newStateVector;
            totalTime += scaledDeltaTime;
            if (_activeForce.magnitude > 0)
            {
                scaledDeltaTime = Time.fixedDeltaTime;
            }
            else
            {
                scaledDeltaTime = GravityController.Instance.smoothCurve.Evaluate(newStateVector.acceleration.magnitude) * Time.fixedDeltaTime;
            }
        }
        body.currentStateVector = body.trajectory.Peek();
        plotTrajectory.DrawTrajectory(body.trajectory);
        yield return null;
    }
}