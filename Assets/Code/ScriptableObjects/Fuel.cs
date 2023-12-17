using UnityEngine;


[CreateAssetMenu(fileName = "Fuel", menuName = "ScriptableObjects/Fuel", order = 1)]
public class Fuel : ScriptableObject
{
    public FuelType fuelType;
    [Tooltip("Fuel density in kg/m^3")]
    public float density;
    [Tooltip("Specific heat ratio of the fluid (dimensionless)")]
    public float gamma;
}