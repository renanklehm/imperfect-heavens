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

    public void GenerateTrajectory(BurnData burnData = default)
    {
        throw new Exception("Not implemented");
    }
}