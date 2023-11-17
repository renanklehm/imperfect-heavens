using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhysicalBody))]
public class OnRailsBody : MonoBehaviour, iBodySolver
{
    public PhysicalBody body;
    public PlotTrajectory plotTrajectory;
    public OrbitalParameters orbitalParameters;
    public int plotSteps;
    public float period;
    public float periapsisDistance;
    public float semiMajorAxis;
    public float eccentricity;
    public float inclination;
    public float longAscNode;
    public float argumentPeriapsis;

    private float timeAdjust;
    private float lastEccentricAnomaly;
    private float semiLatusRectum;

    private void Start()
    {
        body = GetComponent<PhysicalBody>();
        plotTrajectory = GetComponentInChildren<PlotTrajectory>();
        orbitalParameters = new OrbitalParameters(
            period, periapsisDistance, semiMajorAxis, eccentricity, inclination, longAscNode, argumentPeriapsis
        );
        semiLatusRectum = semiMajorAxis * Constants.DISTANCE_FACTOR * (1 - eccentricity * eccentricity);
        CalculateTrajectory();
    }

    public void GetNewPoint()
    {
        lastEccentricAnomaly += 2 * Mathf.PI / plotSteps;
        StateVector newStateVector = Solver.Solve(lastEccentricAnomaly, semiLatusRectum, body.mass, orbitalParameters);
        body.trajectory.Enqueue(newStateVector);
        plotTrajectory.DrawTrajectory(body.trajectory);
    }

    private void CalculateTrajectory()
    {
        for (int j = 0; j < plotSteps; j++)
        {
            lastEccentricAnomaly = 2 * Mathf.PI / plotSteps * j;
            StateVector newStateVector = Solver.Solve(lastEccentricAnomaly, semiLatusRectum, body.mass, orbitalParameters);
            
            if (j == 0)
            {
                timeAdjust = newStateVector.timestamp;
                body.currentStateVector = newStateVector;
                body.trajectory = new Trajectory(newStateVector, plotSteps);
            }
            else
            {
                body.trajectory.Enqueue(newStateVector);
            }
        }
        plotTrajectory.DrawTrajectory(body.trajectory);
    }


}