using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[CreateAssetMenu(fileName = "Engine", menuName = "ScriptableObjects/Engine", order = 1)]
public class Engine : ScriptableObject
{
    [Tooltip("The type of fuel that this engine consumes")]
    public FuelType fuelType;
    [Tooltip("Mass flow rate in kg/s")]
    public AnimationCurve massFlowCurve;
    [Tooltip("The specific impulse of the engine in kg/(N*s)")]
    public float specificImpulse;
    [Tooltip("Chamber pressure in kPa")]
    public float chamberPressure;
    [Tooltip("Chamber temperature in K")]
    public float chamberTemperature;
    [Tooltip("Cross sectional area of the throat of the nozzle in m^2")]
    public float throatCrossSectionalArea;
    [Tooltip("Cross sectional area of the exhaust of the nozzle in m^2")]
    public float exhaustCrossSectionalArea;
    [Tooltip("Mass of the engine in kg")]
    public float dryMass;

    private float gamma = 0f;
    private float exitMach = 0f;
    private float exitPressure = 0f;
    private float exitTemperature = 0f;
    private float exitVelocity = 0f;


    public void InitializeEngine()
    {
        Fuel[] availableFuels = Resources.LoadAll<Fuel>("ScriptableObjects/Fuel");
        foreach (Fuel _fuel in availableFuels)
        {
            if (fuelType == _fuel.fuelType) gamma = _fuel.gamma;
        }
        if (gamma == 0)
        {
            Debug.LogError("Fuel type " + fuelType + " not found");
            return;
        }

        exitMach = GetExitMach();
        exitPressure = GetExitPressure();
        exitTemperature = GetExitTemperature();
        exitVelocity = specificImpulse * Constants.STANDARD_GRAVITY;
    }

    public float FireEngine(FuelTank fuelTank, float throttle, float deltaTime)
    {
        float massFlow = massFlowCurve.Evaluate(throttle) * deltaTime;
        float thrust = fuelTank.Drain(massFlow) * exitVelocity + exitPressure * exhaustCrossSectionalArea;
        return thrust;
    }

    public Engine GetSimulationEngine()
    {
        Engine simulationEngine = Instantiate(this);
        simulationEngine.InitializeEngine();
        return simulationEngine;
    }

    private float GetExitTemperature()
    {
        return chamberTemperature * Mathf.Pow(1 + (gamma - 1) / 2 * exitMach * exitMach, -1);
    }

    private float GetExitPressure()
    {
        return Mathf.Pow(1 + (gamma - 1) / 2 * exitMach * exitMach, -gamma / (gamma - 1)) * chamberPressure;
    }

    private float GetExitMach(int maxIterations = 1000, float tolerance = 1e-6f, float simulationStep = 1e-4f)
    {
        float desiredAreaRatio = exhaustCrossSectionalArea / throatCrossSectionalArea;
        float _exitMach = 1.5f;
        for (int i = 0; i < maxIterations; i++)
        {
            float f_0 = _AreaRatioEquation(_exitMach, desiredAreaRatio);
            float f_1 = _AreaRatioEquation(_exitMach + simulationStep, desiredAreaRatio);
            float df = (f_1 - f_0) / simulationStep;
            _exitMach -= (f_0 / df);
            if (Mathf.Abs(f_0) < tolerance)
            {
                return _exitMach;
            }
        }
        Debug.LogError("Failed to find exit mach.");
        return 0;
    }

    private float _AreaRatioEquation(float _exitMach, float desiredAreaRatio)
    {
        float expoent = (gamma + 1) / (2 * (gamma - 1));
        float leftSide = Mathf.Pow((gamma + 1) / 2, -expoent);
        float rightSide = Mathf.Pow(1 + ((gamma - 1) / 2) * _exitMach * _exitMach, expoent) / _exitMach;
        return leftSide * rightSide - desiredAreaRatio;
    }
}