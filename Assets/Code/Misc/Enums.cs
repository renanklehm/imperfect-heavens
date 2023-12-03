public enum MotionVector
{
    Prograde = 0,
    Retrograde = 1,
    Normal = 2,
    AntiNormal = 3,
    RadialOut = 4,
    RadialIn = 5,
    nullDir = 6
}

public enum Planets
{
    Mercury = 1,
    Venus = 2,
    Earth = 3,
    Mars = 4,
    Jupter = 5,
    Saturn = 6,
    Uranus = 7,
    Neptune = 8,
}

public enum SolverType
{
    OnRails,
    FreeBody
}

public enum TrajectoryRedrawMode
{
    Incremental,
    Decremental,
    Update,
    NoRedraw
}