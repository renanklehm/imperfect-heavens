using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(Body))]
public class OnRailsBody : NetworkBehaviour, iBodySolver
{
    public SolverType solverType { get { return SolverType.OnRails; } }
    public Body body { get; set; }
    public OrbitalParameters orbitalParameters;
    public int plotSteps;
    public float period;
    public float periapsisDistance;
    public float semiMajorAxis;
    public float eccentricity;
    public float inclination;
    public float longAscNode;
    public float argumentPeriapsis;

    private float lastEccentricAnomaly;
    private float semiLatusRectum;

    private void Awake()
    {
        body = GetComponent<Body>();
        orbitalParameters = new OrbitalParameters(
            period, periapsisDistance, semiMajorAxis, eccentricity, inclination, longAscNode, argumentPeriapsis
        );
        semiLatusRectum = semiMajorAxis * Constants.DISTANCE_FACTOR * (1 - eccentricity * eccentricity);
    }

    public void GetNewPoint()
    {
        lastEccentricAnomaly += 2 * Mathf.PI / plotSteps;
        StateVector newStateVector = Solver.Solve(lastEccentricAnomaly, semiLatusRectum, body.mass, orbitalParameters);
        body.trajectory.Enqueue(newStateVector);
    }

    public void GenerateTrajectory(Maneuver burnData = default)
    {
        StartCoroutine(GenerateTrajectoryAsync());
    }

    IEnumerator GenerateTrajectoryAsync()
    {
        body.trajectory.Enqueue(body.currentStateVector);
        for (int j = 0; j <= plotSteps; j++)
        {
            lastEccentricAnomaly = 2 * Mathf.PI / plotSteps * j;
            StateVector newStateVector = Solver.Solve(lastEccentricAnomaly, semiLatusRectum, body.mass, orbitalParameters);

            if (j == 0)
            {
                body.currentStateVector = newStateVector;
            }
            else
            {
                body.trajectory.Enqueue(newStateVector);
            }
        }
        body.trajectory.needRedraw = true;
        yield return null;
    }
}