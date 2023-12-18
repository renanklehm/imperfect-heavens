using Fusion;
using UnityEngine;

public partial class Tank
{
    public float currentFuelMass;

    public float currentMass { get { return currentFuelMass + dryMass; } }
    public float currentFilledPercentage { get { return currentFuelMass / capacity; } }

    public bool IsEmpty()
    {
        return currentFuelMass == 0;
    }

    public void Fill(float filledPercentage)
    {
        currentFuelMass += Mathf.Min(capacity * filledPercentage, capacity);
    }

    public float Drain(float drainedMass) 
    {
        drainedMass = Mathf.Min(drainedMass, currentFuelMass);
        currentFuelMass -= drainedMass;
        return drainedMass;
    }
}
