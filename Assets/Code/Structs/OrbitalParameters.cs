public struct OrbitalParameters
{
    public float period;
    public float periapsisDistance;
    public float semiMajorAxis;
    public float eccentricity;
    public float inclination;
    public float longAscNode;
    public float argumentPeriapsis;

    public OrbitalParameters(
        float _period,
        float _periapsisDistance,
        float _semiMajorAxis,
        float _eccentricity,
        float _inclination,
        float _longAscNode,
        float _argumentPeriapsis
        )
    {
        period = _period;
        periapsisDistance = _periapsisDistance;
        semiMajorAxis = _semiMajorAxis;
        eccentricity = _eccentricity;
        inclination = _inclination;
        longAscNode = _longAscNode;
        argumentPeriapsis = _argumentPeriapsis;
    }
}