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
    public Fuel fuel;
    [HideInInspector]
    public bool isInitialized = false;

    [HideInInspector]
    public float currentFuelVolume { get { return _currentFuelVolume; } private set { _currentFuelVolume = Mathf.Min(value, totalVolume); } }
    [HideInInspector]
    public float currentFuelMass { get { return currentFuelVolume * fuel.density; } }
    [HideInInspector]
    public float currentMass { get { return currentFuelMass + dryMass; } }
    [HideInInspector]
    public float currentFilledPercentage { get { return currentFuelVolume / totalVolume; } }

    private float _currentFuelVolume;

    public void InitializeTank(float filledPercentage)
    {
        Fuel[] availableFuels = Resources.LoadAll<Fuel>("ScriptableObjects/Fuel");
        foreach (Fuel _fuel in availableFuels)
        {
            if (fuelType == _fuel.fuelType) fuel = _fuel;
        }
        if (fuel == null)
        {
            Debug.LogError("Fuel type " + fuelType + " not found");
            return;
        }

        currentFuelVolume = totalVolume * filledPercentage;
        isInitialized = true;
    }   

    public bool IsEmpty()
    {
        return currentFuelMass == 0;
    }

    public void Fill(float filledPercentage)
    {
        if (!isInitialized)
        {
            Debug.LogError("Fuel tank not initialized");
        }

        currentFuelVolume += Mathf.Min(totalVolume * filledPercentage, currentFuelVolume);
    }

    public float Drain(float drainedMass) 
    {
        if (!isInitialized)
        {
            Debug.LogError("Fuel tank not initialized");
            return 0;
        }

        drainedMass = Mathf.Min(drainedMass, currentFuelMass);
        currentFuelVolume -= drainedMass / fuel.density;

        return drainedMass;
    }

    public FuelTank GetSimulationTank()
    {
        FuelTank simulationTank = Instantiate(this);
        simulationTank.InitializeTank(1);
        return simulationTank;
    }
}
