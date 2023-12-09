using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Engine", menuName = "ScriptableObjects/Engine", order = 1)]
public class Engine : ScriptableObject
{
    [Tooltip("The type of fuel that this engine consumes")]
    public FuelType fuelType;
    [Tooltip("Mass flow rate in kg/s")]
    public AnimationCurve massFlowCurve;
    [Tooltip("Specific impulse in s")]
    public float engineISP;
    [Tooltip("Mass of the engine in kg")]
    public float engineMass;

    [HideInInspector]
    public FuelTank fuelTank;
    [HideInInspector]
    public bool isInitialized = false;


    public void InitializeEngine(FuelTank fuelTank)
    {
        this.fuelTank = fuelTank;
        isInitialized = true;
    }

    public float FireEngine(float throttle, bool isRealBurn)
    {
        if (!isInitialized)
        {
            Debug.LogError("Engine not initialized");
            return 0;
        }

        float massFlow = massFlowCurve.Evaluate(throttle);
        float fuelMass = fuelTank.Drain(massFlow * Time.fixedDeltaTime, isRealBurn);
        float thrust = fuelMass * engineISP * Constants.STANDARD_GRAVITY;
        return thrust;
    }
}