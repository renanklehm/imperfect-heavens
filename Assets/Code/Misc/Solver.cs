using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Solver
{
    public static StateVector Solve(StateVector initialStateVector, float mass, float deltaTime, Vector3 activeForce = new Vector3(), float timeStamp = -1f)
    {
        if (timeStamp == -1f)
        {
            timeStamp = GravityManager.Instance.timestamp + deltaTime;
        }

        return EulerMethod(initialStateVector, mass, deltaTime, timeStamp, activeForce);
    }

    public static StateVector Solve(float eccentricAnomaly, float semiLatusRectum, float mass, OrbitalParameters orbitalParameters)
    {
        return KepplerEquation(eccentricAnomaly, semiLatusRectum, mass, orbitalParameters);
    }

    public static float GetOrbitalSpeed(float orbitalHeight, Body relativeBody)
    {
        return Mathf.Sqrt((Constants.GRAVITATIONAL_CONSTANT * relativeBody.mass) / (orbitalHeight * Constants.DISTANCE_FACTOR)) / Constants.DISTANCE_FACTOR;
    }

    private static StateVector EulerMethod(StateVector initialStateVector, float mass, float deltaTime, float timeStamp, Vector3 activeForce)
    {
        Vector3 currentNetForce = GravityManager.Instance.GetNetForce(initialStateVector, mass, timeStamp);
        Vector3 newGravityAcceleration = currentNetForce / mass;
        Vector3 newAcceleration = newGravityAcceleration + (activeForce / mass);
        Vector3 newVelocity = initialStateVector.velocity + (newAcceleration * deltaTime) / Constants.DISTANCE_FACTOR;
        Vector3 newPosition = initialStateVector.position + newVelocity * deltaTime;
        var motionVectors = GravityManager.Instance.GetMotionVectors(newPosition, newVelocity, newGravityAcceleration);

        return new StateVector(
            newPosition, 
            newVelocity, 
            newAcceleration, 
            motionVectors[MotionVector.Prograde], 
            motionVectors[MotionVector.RadialOut], 
            initialStateVector.timestamp + deltaTime,
            newGravityAcceleration,
            activeForce);
    }

    private static StateVector KepplerEquation(float eccentricAnomaly, float semiLatusRectum, float mass, OrbitalParameters orbitalParameters)
    {
        float[] time = new float[3];

        Vector3[] positions = new Vector3[3];
        Vector3[] velocities = new Vector3[3];
        Vector3 acceleration;

        for (int i = 0; i < 3; i++)
        {

            float localEccentricAnomaly = eccentricAnomaly + (i * 1e-6f);
            float meanAnomaly = localEccentricAnomaly - orbitalParameters.eccentricity * Mathf.Sin(localEccentricAnomaly);
            float trueAnomaly =
                2 * Mathf.Atan2(Mathf.Sqrt(1 + orbitalParameters.eccentricity) * Mathf.Sin(localEccentricAnomaly / 2), Mathf.Sqrt(1 - orbitalParameters.eccentricity) *
                Mathf.Cos(localEccentricAnomaly / 2));

            float radius = semiLatusRectum / (1 + orbitalParameters.eccentricity * Mathf.Cos(trueAnomaly));

            float x =
                radius * (
                Mathf.Cos(trueAnomaly + orbitalParameters.argumentPeriapsis * Constants.DEG2RAD) *
                Mathf.Cos(orbitalParameters.longAscNode * Constants.DEG2RAD) - Mathf.Sin(trueAnomaly + orbitalParameters.argumentPeriapsis * Constants.DEG2RAD) *
                Mathf.Sin(orbitalParameters.longAscNode * Constants.DEG2RAD) * Mathf.Cos(orbitalParameters.inclination * Constants.DEG2RAD)
                );
            float y =
                radius * (
                Mathf.Cos(trueAnomaly + orbitalParameters.argumentPeriapsis * Constants.DEG2RAD) *
                Mathf.Sin(orbitalParameters.longAscNode * Constants.DEG2RAD) + Mathf.Sin(trueAnomaly + orbitalParameters.argumentPeriapsis * Constants.DEG2RAD) *
                Mathf.Cos(orbitalParameters.longAscNode * Constants.DEG2RAD) * Mathf.Cos(orbitalParameters.inclination * Constants.DEG2RAD)
                );
            float z =
                radius *
                Mathf.Sin(trueAnomaly + orbitalParameters.argumentPeriapsis * Constants.DEG2RAD) * Mathf.Sin(orbitalParameters.inclination * Constants.DEG2RAD);

            float orbitalPeriod = 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(orbitalParameters.semiMajorAxis * Constants.DISTANCE_FACTOR, 3) / (Constants.GRAVITATIONAL_CONSTANT * mass));
            time[i] = meanAnomaly * orbitalPeriod / (2 * Mathf.PI);
            positions[i] = new Vector3(x, z, y) / Constants.DISTANCE_FACTOR;
        }

        velocities[0] = positions[1] - positions[0];
        velocities[1] = positions[2] - positions[1];
        acceleration = velocities[1] - velocities[0];

        var motionVector = GravityManager.Instance.GetMotionVectors(positions[0], velocities[0], acceleration);

        return new StateVector(positions[0], velocities[0], Vector3.zero, motionVector[MotionVector.Prograde], motionVector[MotionVector.RadialOut], time[0], acceleration);
    }

    public static float GetHandleSize(Vector3 position, Camera camera, float sizeFactor)
    {
        Vector3 cameraPosition = camera.transform.position;
        Vector3 cameraZDirection = camera.transform.forward;

        float z = (position.x - cameraPosition.x) * cameraZDirection.x +
                  (position.y - cameraPosition.y) * cameraZDirection.y +
                  (position.z - cameraPosition.z) * cameraZDirection.z;

        return z * sizeFactor;
    }

    public static string FormatTimeSpan(TimeSpan timeSpan)
    {
        string sign = (timeSpan.TotalSeconds < 0) ? "-" : "+";

        int days = Mathf.Abs(timeSpan.Days);
        int hours = Mathf.Abs(timeSpan.Hours);
        int minutes = Mathf.Abs(timeSpan.Minutes);
        int seconds = Mathf.Abs(timeSpan.Seconds);

        string formattedTime = $"{sign}{days:D3}:{hours:D2}:{minutes:D2}:{seconds:D2}";
        return formattedTime;
    }
}