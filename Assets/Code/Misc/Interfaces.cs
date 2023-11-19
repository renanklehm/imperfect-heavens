using System;

public interface iBodySolver
{
    public SolverType solverType { get; set; }
    public PhysicalBody body { get; set; }

    public void GetNewPoint()
    {
        throw new Exception("Not implemented");
    }

    public void GenerateTrajectory()
    {
        throw new Exception("Not implemented");
    }
}