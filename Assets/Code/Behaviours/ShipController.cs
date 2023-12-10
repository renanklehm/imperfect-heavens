using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

[RequireComponent(typeof(FreeBody))]
public class ShipController : NetworkBehaviour
{
    public FuelTank[] fuelTanks;
    public Engine[] engines;

    [HideInInspector]
    public Body body;

    private void Start()
    {
        body = GetComponent<Body>();
        body.mass = 0;
        Fuel[] availableFuels = Resources.LoadAll<Fuel>("ScriptableObjects/Fuel");
        foreach (FuelTank tank in fuelTanks)
        {
            foreach (Fuel fuel in availableFuels)
            {
                if (tank.fuelType == fuel.fuelType) tank.InitializeTank(Instantiate(fuel), 1);
            }
            if (!tank.isInitialized) Debug.LogError("Fuel tank not initialized");
            body.mass += tank.currentMass;
            body.mass += tank.dryMass;
        }

        foreach (Engine engine in engines)
        {
            foreach (FuelTank tank in fuelTanks)
            {
                if (engine.fuelType == tank.fuelType) engine.InitializeEngine(tank);
            }
            if (!engine.isInitialized) Debug.LogError("Engine fuel tank not found");
            body.mass += engine.engineMass;
        }

    }

    public float GetCurrentFuelMass()
    {
        float totalFuelMass = 0;
        foreach (FuelTank tank in fuelTanks)
        {
            totalFuelMass += tank.currentMass;
        }
        return totalFuelMass;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AddManeuver(MotionVector burnDirection, float burnStrength, float burnDuration, PlayerRef playerRef)
    {
        if (body.Object.InputAuthority == playerRef)
        {
            Vector3 direction;

            switch (burnDirection)
            {
                case MotionVector.Prograde:
                    direction = transform.forward;
                    break;
                case MotionVector.Retrograde:
                    direction = -transform.forward;
                    break;
                case MotionVector.Normal:
                    direction = transform.up;
                    break;
                case MotionVector.AntiNormal:
                    direction = -transform.up;
                    break;
                case MotionVector.RadialOut:
                    direction = transform.right;
                    break;
                case MotionVector.RadialIn:
                    direction = -transform.right;
                    break;
                default:
                    direction = Vector3.zero;
                    break;
            }
        }
    }

    //Simulate the burn, reducing the ship mass and returns the burn duration, fuel consumption and remaining fuel mass
    public Dictionary<string, float> SimulateBurn(float throttle, float deltaV)
    {
        float currentTotalMass = body.mass;
        float currentFuelMass = GetCurrentFuelMass();
        float currentDryMass = currentTotalMass - currentFuelMass;

        float consumedFuelMass = 0;
        float currentDeltaV = 0;
        float elapsedTime = 0; 
        float simulationTimestep = 1f;

        while (currentDeltaV <= deltaV)
        {
            currentTotalMass = currentFuelMass + currentDryMass;

            if (currentFuelMass < 0)
            {
                break;
            }
            else
            {
                float totalThrust = 0;
                float totalMassFlow = 0;

                foreach (Engine engine in engines)
                {
                    float massFlow = engine.massFlowCurve.Evaluate(throttle) * simulationTimestep;
                    totalThrust += massFlow * engine.exhaustVelocity;
                    totalMassFlow += massFlow;
                }

                float acceleration = totalThrust / currentTotalMass;
                consumedFuelMass += totalMassFlow;
                currentFuelMass -= totalMassFlow;
                currentDeltaV += acceleration;
                elapsedTime += simulationTimestep;
            }
        }

        return new Dictionary<string, float>
        {
            { "burnDuration", elapsedTime },
            { "deltaV", currentDeltaV },
            { "fuelConsumption", consumedFuelMass },
            { "remainingFuelMass", currentFuelMass }
        };
    }


    /*
    public float GetBurnDuration(float throttle, float deltaV)
    {
        float totalThrust = 0;
        foreach (Engine engine in engines)
        {
            totalThrust += engine.FireEngine(throttle, false);
        }
        float acceleration = totalThrust / body.mass;
        return deltaV / acceleration;
    }

    public float GetFuelConsumption(float throttle, float burnDuration)
    {
        float totalMassFlow = 0;
        foreach (Engine engine in engines)
        {
            totalMassFlow += engine.massFlowCurve.Evaluate(throttle);
        }
        return totalMassFlow * burnDuration;
    }

    public float GetRemainingFuelMass(float deltaMass)
    {
        float totalFuelMass = 0;
        foreach (FuelTank tank in fuelTanks)
        {
            totalFuelMass += tank.currentMass;
        }
        return totalFuelMass - deltaMass;
    }
    */
}
