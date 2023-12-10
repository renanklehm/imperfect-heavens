using Fusion;
using UnityEngine;

[CreateAssetMenu(fileName = "FuelTank", menuName = "ScriptableObjects/FuelTank", order = 1)]
public class FuelTank : ScriptableObject
{
    [Tooltip("The type of fuel that this tank holds")]
    public FuelType fuelType;   
    [Tooltip("Total volume of the tank in m^3")]
    public float totalVolume;
    [Tooltip("Dry mass of the tank in kg")]
    public float dryMass;

    [HideInInspector]
    public float currentVolume;
    [HideInInspector]
    public float currentMass;
    [HideInInspector]
    public Fuel fuel;
    [HideInInspector]
    public bool isInitialized = false;

    public void InitializeTank(Fuel fuel, float filledPercentage)
    {
        this.fuel = fuel;
        currentVolume = totalVolume * filledPercentage;
        currentMass = currentVolume * fuel.density;
        isInitialized = true;
    }   

    public bool IsEmpty()
    {
        return currentMass == 0;
    }

    public float Fill(float filledMass)
    {
        if (!isInitialized)
        {
            Debug.LogError("Fuel tank not initialized");
            return 0;
        }

        filledMass = Mathf.Min(filledMass, fuel.density * (totalVolume - currentVolume));
        float filledVolume = filledMass / fuel.density;
        currentVolume += filledVolume;
        currentMass += filledMass;
        return filledMass;
    }

    public float Drain(float drainedMass, bool isRealBurn) 
    {
        if (!isInitialized)
        {
            Debug.LogError("Fuel tank not initialized");
            return 0;
        }

        drainedMass = Mathf.Min(drainedMass, currentMass);

        if (isRealBurn)
        {
            float drainedVolume = drainedMass / fuel.density;
            currentVolume -= drainedVolume;
            currentMass -= drainedMass;
        }

        return drainedMass;
    }
}
