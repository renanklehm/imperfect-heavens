using System;
using UnityEngine;

public interface iBodySolver
{
    public SolverType solverType { get; }
    public Body body { get; set; }

    public void GetNewPoint()
    {
        throw new Exception("Not implemented");
    }

    public void GenerateTrajectory(Maneuver burnData = default)
    {
        throw new Exception("Not implemented");
    }

    public void SetManeuver(Maneuver burnData)
    {
        throw new Exception("Not implemented");
    }
}