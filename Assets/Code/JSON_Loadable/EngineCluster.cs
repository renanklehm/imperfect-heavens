using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Newtonsoft.Json;

public partial class EngineCluster
{
    public float FireEngine(int modeIndex, float throttle, float deltaTime, Tank fuelTank, Tank oxidizerTank = null, float atmosphericPressure = 0)
    {
        float massFlow = engineModes[modeIndex].MassFlow(throttle) * deltaTime * engineCount;
        float specificImpulse = engineModes[modeIndex].SpecificImpulse(atmosphericPressure);
        float fuelMixture = engineModes[modeIndex].fuelMixture.ratio;
        float thrust = 0f;
        if (fuelMixture == 0)
            thrust = fuelTank.Drain(massFlow) * Constants.STANDARD_GRAVITY * specificImpulse;
        else
        {
            if (oxidizerTank == null) throw new System.Exception("Oxidizer tank is null");
            float fuelMassFlow = massFlow * (1 / (fuelMixture + 1));
            float oxidizerMassFlow = massFlow * (fuelMixture / (fuelMixture + 1));
            thrust += fuelTank.Drain(fuelMassFlow) * Constants.STANDARD_GRAVITY * specificImpulse;
            thrust += oxidizerTank.Drain(oxidizerMassFlow) * Constants.STANDARD_GRAVITY * specificImpulse;
        }
        return thrust;
    }
}