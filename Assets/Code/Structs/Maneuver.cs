using System.Collections.Generic;
using UnityEngine;

public struct Maneuver
{
    public float startTime;
    public float endTime;
    public float burnDuration;
    public float deltaV;
    public float fuelConsumption;
    public float remainingFuelMass;
    public bool success;

    private float deltaTime;
    private List<float> accelerationCurve;
    private Vector3 direction;

    public Maneuver(
        int modeIndex,
        int clusterIndex,
        Vector3 direction, 
        float startTime, 
        float throttle, 
        float targetDeltaV, 
        Ship simulatedShip,
        float atmosphericPressure = 0f)
    {
        this.startTime = startTime;
        this.deltaTime = 1f;
        accelerationCurve = new List<float>();

        float initialFuelMass = simulatedShip.fuelMass;
        float currentDeltaV = 0;
        float elapsedTime = 0;

        while (currentDeltaV < targetDeltaV)
        {
            float thrust = simulatedShip.FireEngines(throttle, deltaTime, atmosphericPressure, modeIndex, clusterIndex);
            if (thrust == 0) break;

            float acceleration = thrust / simulatedShip.mass;
            accelerationCurve.Add(acceleration);
            currentDeltaV += (acceleration * deltaTime);
            elapsedTime += deltaTime;
        }

        success = currentDeltaV >= targetDeltaV;
        burnDuration = elapsedTime;
        endTime = startTime + burnDuration;
        deltaV = currentDeltaV;
        fuelConsumption = initialFuelMass - simulatedShip.fuelMass;
        remainingFuelMass = simulatedShip.fuelMass;
        this.direction = direction;
    }

    public Vector3 GetThrust(float time)
    {
        int index = Mathf.FloorToInt((time - startTime) / deltaTime);
        if (0 <= index && index < accelerationCurve.Count)
        {
            return accelerationCurve[index] * direction;
        }
        else
        {
            throw new System.Exception("Index out of range");
        }
    }
}
