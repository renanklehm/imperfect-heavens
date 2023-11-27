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
        Vector3 currentNetForce = GravityManager.Instance.GetNetForce(initialStateVector, mass, timeStamp) + activeForce;
        Vector3 newAcceleration = currentNetForce / mass;
        Vector3 newVelocity = initialStateVector.velocity + (newAcceleration * deltaTime) / Constants.DISTANCE_FACTOR;
        Vector3 newPosition = initialStateVector.position + newVelocity * deltaTime;

        return new StateVector(newPosition, newVelocity, newAcceleration, initialStateVector.timestamp + deltaTime, activeForce);
    }

    private static StateVector KepplerEquation(float eccentricAnomaly, float semiLatusRectum, float mass, OrbitalParameters orbitalParameters)
    {
        float meanAnomaly = eccentricAnomaly - orbitalParameters.eccentricity * Mathf.Sin(eccentricAnomaly);

        float trueAnomaly = 
            2 * Mathf.Atan2(Mathf.Sqrt(1 + orbitalParameters.eccentricity) * Mathf.Sin(eccentricAnomaly / 2), Mathf.Sqrt(1 - orbitalParameters.eccentricity) * 
            Mathf.Cos(eccentricAnomaly / 2));
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

        float speed = Mathf.Sqrt(Constants.GRAVITATIONAL_CONSTANT * mass * (2 / radius - 1 / (orbitalParameters.semiMajorAxis * Constants.DISTANCE_FACTOR)));
        float vx = -speed * Mathf.Sin(trueAnomaly + orbitalParameters.argumentPeriapsis * Constants.DEG2RAD);
        float vy = speed * (orbitalParameters.eccentricity + Mathf.Cos(trueAnomaly + orbitalParameters.argumentPeriapsis * Constants.DEG2RAD));
        float vz = 0f;

        float orbitalPeriod = 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(orbitalParameters.semiMajorAxis * Constants.DISTANCE_FACTOR, 3) / (Constants.GRAVITATIONAL_CONSTANT * mass));
        float time = meanAnomaly * orbitalPeriod / (2 * Mathf.PI);
        return new StateVector(new Vector3(x, z, y) / Constants.DISTANCE_FACTOR, new Vector3(vx, vz, vy) / Constants.DISTANCE_FACTOR, Vector3.zero, time);
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