using System.Collections.Generic;
using UnityEngine;

public struct BurnData
{
    public float startTime;
    public float endTime;
    public float burnDuration;
    public float deltaV;
    public float fuelConsumption;
    public float remainingFuelMass;
    public bool success;

    private List<float> accelerationCurve;
    private Vector3 direction;

    public BurnData(Vector3 direction, float startTime, float throttle, float targetDeltaV, Engine[] engines, FuelTank[] fuelTanks)
    {
        this.startTime = startTime;
        accelerationCurve = new List<float>();
        float dryMass = 0;
        float fuelMass = 0;

        Engine[] simulatedEngines = new Engine[engines.Length];
        for (int i = 0; i < engines.Length; i++)
        {
            simulatedEngines[i] = engines[i].GetSimulationEngine();
            dryMass += simulatedEngines[i].dryMass;
        }

        FuelTank[] simulatedFuelTanks = new FuelTank[fuelTanks.Length];
        for (int i = 0; i < fuelTanks.Length; i++)
        {
            simulatedFuelTanks[i] = fuelTanks[i].GetSimulationTank();
            dryMass += simulatedFuelTanks[i].dryMass;
            fuelMass += simulatedFuelTanks[i].currentFuelMass;
        }

        float initialFuelMass = fuelMass;
        float deltaTime = Time.fixedDeltaTime;
        float currentDeltaV = 0;
        float elapsedTime = 0;
        while (currentDeltaV < targetDeltaV)
        {
            fuelMass = 0;
            FuelTank availableTank = null;
            for (int i = 0; i < simulatedFuelTanks.Length; i++)
            {
                if (simulatedFuelTanks[i].currentFuelMass > 0)
                {
                    availableTank = simulatedFuelTanks[i];
                }
                fuelMass += simulatedFuelTanks[i].currentFuelMass;
            }

            if (availableTank == null)
            {
                break;
            }

            float currentTotalMass = dryMass + fuelMass;
            float totalThrust = 0;

            foreach (Engine engine in simulatedEngines)
            {
                totalThrust += engine.FireEngine(availableTank, throttle, deltaTime);
            }

            float acceleration = totalThrust / currentTotalMass;
            accelerationCurve.Add(acceleration);
            currentDeltaV += (acceleration * deltaTime);
            elapsedTime += deltaTime;
        }

        success = currentDeltaV >= targetDeltaV;
        burnDuration = elapsedTime;
        endTime = startTime + burnDuration;
        deltaV = currentDeltaV;
        fuelConsumption = initialFuelMass - fuelMass;
        remainingFuelMass = fuelMass;
        this.direction = direction;
    }

    public Vector3 GetThrust(float time)
    {
        int index = Mathf.FloorToInt((time - startTime) / Time.fixedDeltaTime);
        if (index >= accelerationCurve.Count || index < 0)
        {
            return Vector3.zero;
        }
        else
        {
            return accelerationCurve[index] * direction;
        }
    }
}
