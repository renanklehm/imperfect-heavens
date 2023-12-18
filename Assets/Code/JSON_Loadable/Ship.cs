using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ship
{
    [JsonProperty("Name")]
    public string name { get; set; }
    [JsonProperty("Skin")] public Skin skin { get; set; }
    [JsonProperty("Structure")] public Structure structure { get; set; }
    [JsonProperty("CICModule")] public Module cicModule { get; set; }
    [JsonProperty("CrewModules")] public Module[] crewModules { get; set; }
    [JsonProperty("PowerCell")] public PowerCell[] powerCell { get; set; }
    [JsonProperty("Storage")] public Storage[] storage { get; set; }
    [JsonProperty("Radiators")] public Radiator[] radiators { get; set; }
    [JsonProperty("Tanks")] public Tank[] tanks { get; set; }
    [JsonProperty("EngineClusters")] public EngineCluster[] engineClusters { get; set; }

    public Dictionary<int, string> engineClusterOptions
    {
        get
        {
            Dictionary<int, string> options = new Dictionary<int, string>();
            for (int i = 0; i < engineClusters.Length; i++)
            {
                options.Add(i, engineClusters[i].name);
            }
            return options;
        }
    }

    public string shipBreakdown
    {
        get
        {
            string breakDown = "---Ship breakdown---\n";
            breakDown += "Skin: " + skin.dryMass.ToString("0.00") + " kg\n";
            breakDown += "Structure: " + structure.dryMass.ToString("0.00") + " kg\n";
            breakDown += "CIC Module: " + cicModule.dryMass.ToString("0.00") + " kg\n";
            foreach (Module x in crewModules) breakDown += x.name + ": " + x.dryMass.ToString("0.00") + " kg\n";
            foreach (PowerCell x in powerCell) breakDown += x.name + " [" + x.powercellCount + " units]: " + x.dryMass.ToString("0.00") + " kg\n";
            foreach (Radiator x in radiators) breakDown += x.name + ": " + x.dryMass.ToString("0.00") + " kg\n";
            foreach (Storage x in storage) breakDown += "Storage (" + x.content.name + "): " + x.currentMass.ToString("0.00") + " kg\n";
            foreach (Tank x in tanks)
            {
                breakDown += "Tank (" + x.content.name + ") - Dry mass: " + x.dryMass.ToString("0.00") + "kg - ";
                breakDown += "Fuel mass: " + x.currentFuelMass.ToString("0.00") + " kg\n";
            }
            foreach (EngineCluster x in engineClusters)
            {
                breakDown += x.name + " [" + x.engineCount + " units]: " + x.dryMass.ToString("0.00") + " kg\n";
                foreach (EngineMode mode in x.engineModes)
                {
                    float modeDeltaV = mode.SpecificImpulse(0) * Constants.STANDARD_GRAVITY * Mathf.Log(GetWetDryMassRatio(mode.fuelMixture));
                    breakDown += "\t'-" + mode.name + "\n";
                    breakDown += "\t\t'-Fuel: " + mode.fuelMixture.fuel.name + "\n";
                    if (mode.fuelMixture.ratio != 0)
                    {
                        breakDown += "\t\t'-Oxidizer: " + mode.fuelMixture.oxidizer.name + "\n";
                        breakDown += "\t\t'-Fuel mixture ratio: " + mode.fuelMixture.ratio + "\n";
                    }
                    breakDown += "\t\t'-Max mass flow: " + mode.maxMassFlow.ToString("0.00") + " kg/s\n";
                    breakDown += "\t\t'-Min mass flow: " + mode.minMassFlow.ToString("0.00") + " kg/s\n";
                    breakDown += "\t\t'-Specific impulse: " + mode.specificImpulseVacumn.ToString("0.00") + " s\n";
                    breakDown += "\t\t'-TWR: " + (mode.Thrust(1, 0) / mass).ToString("0.00") + "\n";
                    breakDown += "\t\t'-Delta-V: " + modeDeltaV.ToString("0.00") + " m/s\n";
                }
            }
            return breakDown;
        }
    }

    public float mass
    {
        get
        {
            return dryMass + fuelMass;
        }
    }

    public float dryMass
    {
        get
        {
            float totalDryMass = 0;
            totalDryMass += skin.dryMass;
            totalDryMass += structure.dryMass;
            totalDryMass += cicModule.dryMass;
            foreach (Module module in crewModules) totalDryMass += module.dryMass;
            foreach (PowerCell cell in powerCell) totalDryMass += cell.dryMass;
            foreach (Storage storage in storage) totalDryMass += storage.currentMass;
            foreach (Tank tank in tanks) totalDryMass += tank.dryMass;
            foreach (EngineCluster cluster in engineClusters) totalDryMass += cluster.dryMass;
            return totalDryMass;
        } 
    }

    public float fuelMass { 
        get { 
            float totalFuelMass = 0;
            foreach (Tank tank in tanks) totalFuelMass += tank.currentFuelMass;
            return totalFuelMass;
        } 
    }

    [JsonConstructor]
    public Ship(
        string name,
        Skin skin,
        Structure structure,
        Module cicModule,
        Module[] crewModules,
        PowerCell[] powerCell,
        Storage[] storage,
        Radiator[] radiators,
        Tank[] tanks,
        EngineCluster[] engineClusters
        )
    {
        this.name = name;
        this.skin = skin;
        this.structure = structure;
        this.cicModule = cicModule;
        this.crewModules = crewModules;
        this.powerCell = powerCell;
        this.storage = storage;
        this.radiators = radiators;
        this.tanks = tanks;
        this.engineClusters = engineClusters;
    }

    public Ship(Ship source)
    {
        this.name = source.name;
        this.skin = source.skin;
        this.structure = source.structure;
        this.cicModule = source.cicModule;

        Module[] _crewModules = new Module[source.crewModules.Length];
        for (int i = 0; i < source.crewModules.Length; i++) _crewModules[i] = new Module(source.crewModules[i]);
        this.crewModules = _crewModules;

        PowerCell[] _powerCell = new PowerCell[source.powerCell.Length];
        for (int i = 0; i < source.powerCell.Length; i++) _powerCell[i] = new PowerCell(source.powerCell[i]);
        this.powerCell = _powerCell;

        Storage[] _storage = new Storage[source.storage.Length];
        for (int i = 0; i < source.storage.Length; i++) _storage[i] = new Storage(source.storage[i]);
        this.storage = _storage;

        Radiator[] _radiators = new Radiator[source.radiators.Length];
        for (int i = 0; i < source.radiators.Length; i++) _radiators[i] = new Radiator(source.radiators[i]);
        this.radiators = _radiators;

        Tank[] _tanks = new Tank[source.tanks.Length];
        for (int i = 0; i < source.tanks.Length; i++) _tanks[i] = new Tank(source.tanks[i]);
        this.tanks = _tanks;

        EngineCluster[] _engineClusters = new EngineCluster[source.engineClusters.Length];
        for (int i = 0; i < source.engineClusters.Length; i++) _engineClusters[i] = new EngineCluster(source.engineClusters[i]);
        this.engineClusters = _engineClusters;
    }

    public float FireEngines(float throttle, float deltaTime, float atmosphericPressure, int modeIndex = 0, int clusterIndex = 0)
    {
        Tank _fuelTank = null;
        Tank _oxidizerTank = null;
        bool needsOxidizer = engineClusters[clusterIndex].engineModes[modeIndex].fuelMixture.ratio != 0;
        string fuelName = engineClusters[clusterIndex].engineModes[modeIndex].fuelMixture.fuel.name;
        string oxidizerName = engineClusters[clusterIndex].engineModes[modeIndex].fuelMixture.oxidizer.name;
        foreach (Tank tank in tanks)
        {
            if (tank.content.name == fuelName && !tank.IsEmpty()) _fuelTank = tank;
            if (needsOxidizer && tank.content.name == oxidizerName && !tank.IsEmpty()) _oxidizerTank = tank;
            if (_fuelTank != null && (!needsOxidizer || _oxidizerTank != null)) break;
        }

        if (_fuelTank == null || (needsOxidizer && _oxidizerTank == null))
        {
            return 0;
        }
        else
        {
            return engineClusters[clusterIndex].FireEngine(modeIndex, throttle, deltaTime, _fuelTank, _oxidizerTank, atmosphericPressure);
        }
    }

    public float TWR(float throttle, float atmosphericPressure, int modeIndex = 0, int clusterIndex = 0)
    {
        return engineClusters[clusterIndex].engineModes[modeIndex].Thrust(throttle, atmosphericPressure) / mass;
    }

    public float DeltaV(float atmosphericPressure, int modeIndex = 0, int clusterIndex = 0)
    {
        EngineMode mode = engineClusters[clusterIndex].engineModes[modeIndex];
        return mode.SpecificImpulse(atmosphericPressure) * Constants.STANDARD_GRAVITY * Mathf.Log(GetWetDryMassRatio(mode.fuelMixture));
    }

    public float GetWetDryMassRatio(FuelMixture fuelMixture)
    {
        float totalWetMass = 0;
        foreach (Tank tank in tanks)
        {
            if (tank.content.name == fuelMixture.fuel.name || tank.content.name == fuelMixture.oxidizer.name)
            {
                totalWetMass += tank.currentFuelMass;
            }
        }
        return mass / (mass - totalWetMass);
    }
}

[System.Serializable]
public partial class Module
{
    [JsonProperty("Name")] public string name { get; set; }
    [JsonProperty("DryMass")] public float dryMass { get; set; }
    [JsonProperty("CrewCapacity")] public int crewCapacity { get; set; }

    [JsonConstructor]
    public Module(string name, float dryMass, int crewCapacity)
    {
        this.name = name;
        this.dryMass = dryMass;
        this.crewCapacity = crewCapacity;
    }

    public Module(Module source)
    {
        this.name = source.name;
        this.dryMass = source.dryMass;
        this.crewCapacity = source.crewCapacity;
    }
}

[System.Serializable]
public partial class PowerCell
{
    [JsonProperty("Name")] public string name { get; set; }
    [JsonProperty("Count")] public int powercellCount { get; set; }
    [JsonProperty("DryMass")] public float dryMass { get; set; }
    [JsonProperty("PowerOutput")] public float powerOutput { get; set; }

    [JsonConstructor]
    public PowerCell(string name, int powercellCount, float dryMass, float powerOutput)
    {
        this.name = name;
        this.powercellCount = powercellCount;
        this.dryMass = dryMass * powercellCount;
        this.powerOutput = powerOutput;
    }

    public PowerCell(PowerCell source)
    {
        this.name = source.name;
        this.powercellCount = source.powercellCount;
        this.dryMass = source.dryMass;
        this.powerOutput = source.powerOutput;
    }
}

[System.Serializable]
public partial class EngineCluster
{
    [JsonProperty("Name")] public string name { get; set; }
    [JsonProperty("Count")] public int engineCount { get; set; }
    [JsonProperty("DryMass")] public float dryMass { get; set; }
    [JsonProperty("Modes")] public EngineMode[] engineModes { get; set; }

    public Dictionary<int, string> engineModeOptions
    {
        get
        {
            Dictionary<int, string> options = new Dictionary<int, string>();
            for (int i = 0; i < engineModes.Length; i++)
            {
                options.Add(i, engineModes[i].name);
            }
            return options;
        }
    }

    [JsonConstructor]
    public EngineCluster(string name, int engineCount, float dryMass, EngineMode[] modes)
    {
        this.name = name;
        this.engineCount = engineCount;
        this.dryMass = dryMass * engineCount;
        this.engineModes = modes;
    }

    public EngineCluster(EngineCluster source)
    {
        this.name = source.name;
        this.engineCount = source.engineCount;
        this.dryMass = source.dryMass;

        EngineMode[] _modes = new EngineMode[source.engineModes.Length];
        for (int i = 0; i < source.engineModes.Length; i++) _modes[i] = new EngineMode(source.engineModes[i]);
        this.engineModes = _modes;
    }
}

[System.Serializable]
public partial class Radiator
{
    [JsonProperty("Name")] public string name { get; set; }
    [JsonProperty("DryMass")] public float dryMass { get; set; }
    [JsonProperty("Cooling")] public float coolingPower { get; set; }

    [JsonConstructor]
    public Radiator(string name, float dryMass, float coolingPower)
    {
        this.name = name;
        this.dryMass = dryMass;
        this.coolingPower = coolingPower;
    }

    public Radiator(Radiator source)
    {
        this.name = source.name;
        this.dryMass = source.dryMass;
        this.coolingPower = source.coolingPower;
    }
}

[System.Serializable]
public partial class Tank
{
    [JsonProperty("Content")] public Substance content { get; set; }
    [JsonProperty("DryMass")] public float dryMass { get; set; }
    [JsonProperty("Capacity")] public float capacity { get; set; }

    [JsonConstructor]
    public Tank(Substance content, float dryMass, float capacity)
    {
        this.content = content;
        this.dryMass = dryMass;
        this.capacity = capacity;
        this.currentFuelMass = capacity;
    }

    public Tank(Tank source)
    {
        this.content = new Substance(source.content);
        this.dryMass = source.dryMass;
        this.capacity = source.capacity;
        this.currentFuelMass = source.currentFuelMass;
    }
}

[System.Serializable]
public partial class Skin
{
    [JsonProperty("Material")] public Substance material { get; set; }
    [JsonProperty("Thickness")] public float thickness { get; set; }
    [JsonProperty("DryMass")] public float dryMass { get; set; }

    [JsonConstructor]
    public Skin(Substance material, float thickness, float dryMass)
    {
        this.material = material;
        this.thickness = thickness;
        this.dryMass = dryMass;
    }

    public Skin(Skin source)
    {
        this.material = new Substance(source.material);
        this.thickness = source.thickness;
        this.dryMass = source.dryMass;
    }
}

[System.Serializable]
public partial class Storage
{
    [JsonProperty("Content")] public Substance content { get; set; }
    [JsonProperty("Capacity")] public float capacity { get; set; }

    public float currentMass;

    [JsonConstructor]
    public Storage(Substance content, float capacity)
    {
        this.content = content;
        this.capacity = capacity;
        this.currentMass = capacity;
    }

    public Storage(Storage source)
    {
        this.content = new Substance(source.content);
        this.capacity = source.capacity;
        this.currentMass = source.currentMass;
    }

    public float Consume(float consumedMass)
    {
        consumedMass = Mathf.Min(consumedMass, currentMass);
        currentMass -= consumedMass;
        return consumedMass;
    }

    public float Fill(float filledMass)
    {
        filledMass = Mathf.Min(filledMass, capacity - currentMass);
        currentMass += filledMass;
        return filledMass;
    }
}

[System.Serializable]
public partial class Structure
{
    [JsonProperty("Material")] public Substance material { get; set; }

    [JsonProperty("DryMass")] public float dryMass { get; set; }

    [JsonConstructor]
    public Structure(Substance material, float dryMass)
    {
        this.material = material;
        this.dryMass = dryMass;
    }

    public Structure(Structure source)
    {
        this.material = new Substance(source.material);
        this.dryMass = source.dryMass;
    }
}

[System.Serializable]
public struct EngineMode
{
    [JsonProperty("Name")] public string name { get; set; }
    [JsonProperty("FuelMisture")] public FuelMixture fuelMixture { get; set; }
    [JsonProperty("MaxMassFlow")] public float maxMassFlow { get; set; }
    [JsonProperty("MinMassFlow")] public float minMassFlow { get; set; }
    [JsonProperty("SpecificImpulseVacumn")] public float specificImpulseVacumn { get; set; }
    [JsonProperty("SpecificImpulseAtmosphere")] public float specificImpulseAtmosphere { get; set; }

    [JsonConstructor]
    public EngineMode(
        string name,
        FuelMixture fuelMisture,
        float maxMassFlow,
        float minMassFlow,
        float specificImpulseVacumn,
        float specificImpulseAtmosphere)
    {
        this.name = name;
        this.fuelMixture = fuelMisture;
        this.maxMassFlow = maxMassFlow;
        this.minMassFlow = minMassFlow;
        this.specificImpulseVacumn = specificImpulseVacumn;
        this.specificImpulseAtmosphere = specificImpulseAtmosphere;
    }

    public EngineMode(EngineMode source)
    {
        this.name = source.name;
        this.fuelMixture = new FuelMixture(source.fuelMixture);
        this.maxMassFlow = source.maxMassFlow;
        this.minMassFlow = source.minMassFlow;
        this.specificImpulseVacumn = source.specificImpulseVacumn;
        this.specificImpulseAtmosphere = source.specificImpulseAtmosphere;
    }

    public float MassFlow(float throttle)
    {
        return Mathf.Lerp(minMassFlow, maxMassFlow, throttle);
    }

    public float SpecificImpulse(float atmosphericPressure)
    {
        return Mathf.Lerp(specificImpulseVacumn, specificImpulseAtmosphere, atmosphericPressure);
    }

    public float Thrust(float throttle, float atmosphericPressure)
    {
        return MassFlow(throttle) * SpecificImpulse(atmosphericPressure) * Constants.STANDARD_GRAVITY;
    }
}

[System.Serializable]
public struct FuelMixture
{
    [JsonProperty("Fuel")] public Substance fuel { get; set; }
    [JsonProperty("Oxidizer", NullValueHandling = NullValueHandling.Ignore)] public Substance oxidizer { get; set; }
    [JsonProperty("Ratio", NullValueHandling = NullValueHandling.Ignore)] public float ratio { get; set; }

    [JsonConstructor]
    public FuelMixture(Substance fuel, Substance oxidizer, float ratio)
    {
        this.fuel = fuel;
        this.oxidizer = oxidizer;
        this.ratio = ratio;
    }

    public FuelMixture(FuelMixture source)
    {
        this.fuel = source.fuel;
        this.oxidizer = source.oxidizer;
        this.ratio = source.ratio;
    }
}

[System.Serializable]
public struct Substance
{
    [JsonProperty("Name")] public string name { get; set; }
    [JsonProperty("Density")] public float density { get; set; }

    [JsonConstructor]
    public Substance(string name, float density)
    {
        this.name = name;
        this.density = density;
    }

    public Substance(Substance source)
    {
        this.name = source.name;
        this.density = source.density;
    }
}